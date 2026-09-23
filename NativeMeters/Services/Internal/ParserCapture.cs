using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace NativeMeters.Services.Internal;

internal sealed class ParserCapture : IDisposable
{
    private const int MaxQueuedRecords = 4096;
    private const long MaxFileBytes = 32 * 1024 * 1024;

    private readonly object gate = new();
    private readonly Queue<string> pending = new();
    private volatile StreamWriter? writer;
    private long writtenBytes;
    private int droppedRecords;

    public string? FilePath { get; private set; }
    public bool IsActive => writer != null;

    public string Start()
    {
        if (!System.Config.General.DebugEnabled) throw new InvalidOperationException("Parser capture requires Debug Mode.");

        lock (gate)
        {
            if (writer != null) return FilePath!;

            var directory = Path.Combine(Service.PluginInterface.GetPluginConfigDirectory(), "Debug", "Captures");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"NativeMeters-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.jsonl");
            writer = new StreamWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read), new UTF8Encoding(false));
            writer.NewLine = "\n";
            FilePath = path;
            writtenBytes = 0;
            droppedRecords = 0;
            pending.Clear();
            return path;
        }
    }

    public void Record(string type, object data)
    {
        lock (gate)
        {
            if (writer == null) return;
            if (pending.Count >= MaxQueuedRecords)
            {
                droppedRecords++;
                return;
            }

            try
            {
                pending.Enqueue(JsonSerializer.Serialize(new { TimestampUtc = DateTimeOffset.UtcNow, Type = type, Data = data }));
            }
            catch (Exception ex)
            {
                droppedRecords++;
                Service.Logger.Error(ex, "Could not serialize parser capture record.");
            }
        }
    }

    public void Flush()
    {
        lock (gate)
        {
            if (writer == null) return;

            try
            {
                while (pending.TryDequeue(out var line))
                {
                    writer.WriteLine(line);
                    writtenBytes += Encoding.UTF8.GetByteCount(line) + 1;
                    if (writtenBytes < MaxFileBytes) continue;

                    WriteEnd("SizeLimit");
                    Close();
                    Service.Logger.Warning("Parser capture stopped at the 32 MiB limit.");
                    return;
                }

                writer.Flush();
            }
            catch (Exception ex)
            {
                Service.Logger.Error(ex, "Parser capture stopped after a file error.");
                Close();
            }
        }
    }

    private void WriteEnd(string reason)
    {
        writer?.WriteLine(JsonSerializer.Serialize(new
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            Type = "CaptureEnd",
            Data = new { Reason = reason, DroppedRecords = droppedRecords + pending.Count },
        }));
    }

    private void Close()
    {
        var current = writer;
        writer = null;
        pending.Clear();

        try
        {
            current?.Dispose();
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "Could not close parser capture.");
        }
    }

    public void Dispose()
    {
        lock (gate)
        {
            Flush();

            try
            {
                WriteEnd("Stopped");
            }
            catch (Exception ex)
            {
                Service.Logger.Error(ex, "Could not finish parser capture.");
            }

            Close();
        }
    }
}
