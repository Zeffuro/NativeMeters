using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using NativeMeters;
using NativeMeters.Services;

namespace StatusTimers.Helpers;

// Taken and adapted for StatusTimers using zips from https://github.com/Caraxi/SimpleHeels/blob/0a0fe3c02a0a2c5a7c96b3304952d5078cd338aa/Plugin.cs#L392
// Thanks Caraxi
public static class BackupHelper {
    private const int MaxBackups = 10;
    private const string Name = "NativeMeters";
    public static void DoConfigBackup(IDalamudPluginInterface pluginInterface) {
        Service.Logger.Debug("Backup configuration start.");
        try {
            var configDirectory = pluginInterface.ConfigDirectory;
            if (!configDirectory.Exists) {
                return;
            }

            var backupDir = Path.Join(configDirectory.Parent!.Parent!.FullName, "backups", Name);
            var dir = new DirectoryInfo(backupDir);
            if (!dir.Exists) {
                dir.Create();
            }

            if (!dir.Exists) {
                throw new Exception("Backup Directory does not exist");
            }

            var latestFile = new FileInfo(Path.Join(backupDir, $"{Name}.latest.zip"));
            var tempFile = Path.Join(backupDir, $"{Name}.tmp.zip");

            var needsBackup = false;

            if (latestFile.Exists) {
                string lastBackupHash = ZipJsonHash(latestFile.FullName);
                string currentConfigDirHash = DirJsonHash(configDirectory.FullName);
                if (currentConfigDirHash != lastBackupHash) {
                    needsBackup = true;
                }
            } else {
                needsBackup = true;
            }

            if (!needsBackup) {
                return;
            }

            ZipFile.CreateFromDirectory(configDirectory.FullName, tempFile);
            if (latestFile.Exists) {
                var t = latestFile.LastWriteTime;
                string archiveName = $"{Name}.{t.Year}{t.Month:00}{t.Day:00}{t.Hour:00}{t.Minute:00}{t.Second:00}.zip";
                string archivePath = Path.Join(backupDir, archiveName);

                bool moved = false;
                for (int i = 0; i < 5 && !moved; i++) {
                    try {
                        File.Move(latestFile.FullName, archivePath);
                        moved = true;
                    } catch (IOException ioEx) when (i < 4) {
                        Service.Logger.Debug($"Move failed, retrying in 100ms: {ioEx.Message}");
                        System.Threading.Thread.Sleep(100);
                    }
                }
                if (!moved) {
                    throw new IOException($"Could not move {latestFile.FullName} after several retries.");
                }
            }

            if (File.Exists(latestFile.FullName)) {
                File.Delete(latestFile.FullName);
            }
            File.Move(tempFile, latestFile.FullName);

            var allBackups = dir.GetFiles().Where(f => f.Name.StartsWith($"{Name}.2") && f.Name.EndsWith(".zip"))
                .OrderBy(f => f.LastWriteTime.Ticks).ToList();
            if (allBackups.Count > MaxBackups) {
                Service.Logger.Debug($"Removing Oldest Backup: {allBackups[0].FullName}");
                File.Delete(allBackups[0].FullName);
            }
        } catch (Exception exception) {
            Service.Logger.Warning(exception, "Backup Skipped");
        }
    }

    private static string ComputeCombinedJsonHash(IEnumerable<(string name, byte[] contents)> files) {
        using var sha256 = SHA256.Create();
        foreach (var file in files.OrderBy(f => f.name, StringComparer.OrdinalIgnoreCase)) {
            sha256.TransformBlock(file.contents, 0, file.contents.Length, null, 0);
        }
        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return BitConverter.ToString(sha256.Hash).Replace("-", "");
    }

    private static string DirJsonHash(string dirPath) =>
        ComputeCombinedJsonHash(
            new DirectoryInfo(dirPath)
                .GetFiles("*.json", SearchOption.TopDirectoryOnly)
                .Where(f => !f.Name.EndsWith(".addon.json", StringComparison.OrdinalIgnoreCase))
                .Select(f => (f.Name, File.ReadAllBytes(f.FullName)))
        );

    private static string ZipJsonHash(string zipPath) {
        byte[] zipBytes = File.ReadAllBytes(zipPath);
        using var msZip = new MemoryStream(zipBytes);
        using var zip = new ZipArchive(msZip, ZipArchiveMode.Read);
        var files = zip.Entries
            .Where(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                        && !e.FullName.EndsWith(".addon.json", StringComparison.OrdinalIgnoreCase)
                        && !e.FullName.Contains("/"))
            .Select(e => {
                using var ms = new MemoryStream();
                using (var s = e.Open()) {
                    s.CopyTo(ms);
                }
                return (e.FullName, ms.ToArray());
            });
        return ComputeCombinedJsonHash(files);
    }
}

