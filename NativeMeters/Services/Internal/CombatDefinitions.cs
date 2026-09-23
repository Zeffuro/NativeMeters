using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NativeMeters.Services.Internal;

internal sealed class CombatDefinitions
{
    private const string ResourcePrefix = "NativeMeters.Data.Combat.";
    private readonly Dictionary<uint, CombatStatusDefinition> statuses = new();
    private readonly Dictionary<uint, CombatActionDefinition> actions = new();

    public byte CalibrationMinLevel { get; }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    public CombatDefinitions()
    {
        var assembly = typeof(CombatDefinitions).Assembly;
        CalibrationMinLevel = Read(assembly, ResourcePrefix + "Overrides.settings.json")["CalibrationMinLevel"]!.GetValue<byte>();
        var manifest = Read(assembly, ResourcePrefix + "Definitions.manifest.json");
        var files = manifest["Files"]?.AsArray() ?? throw new InvalidOperationException("Missing combat definition manifest.");

        foreach (var file in files)
        {
            var name = file!.GetValue<string>();
            var upstream = Read(assembly, ResourcePrefix + "Definitions." + name);
            var document = new JsonObject
            {
                ["Statuses"] = ReadEntries(upstream["statuseffects"]!),
                ["Actions"] = ReadEntries(upstream["actions"]!),
            };

            var overrideName = ResourcePrefix + "Overrides." + name;
            if (assembly.GetManifestResourceInfo(overrideName) != null) Merge(document, Read(assembly, overrideName));

            Load(document["Statuses"], statuses);
            Load(document["Actions"], actions);
        }
    }

    public CombatStatusDefinition? GetStatus(uint id) => statuses.GetValueOrDefault(id);
    public CombatActionDefinition? GetAction(uint id) => actions.GetValueOrDefault(id);

    private static JsonObject Read(Assembly assembly, string resource)
    {
        using var stream = assembly.GetManifestResourceStream(resource)
                           ?? throw new InvalidOperationException($"Missing combat definitions: {resource}");
        var options = new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
        return JsonNode.Parse(stream, new JsonNodeOptions { PropertyNameCaseInsensitive = true }, options)?.AsObject()
               ?? throw new InvalidOperationException($"Invalid combat definitions: {resource}");
    }

    private static JsonObject ReadEntries(JsonNode entries)
    {
        var result = new JsonObject();
        foreach (var entry in entries.AsArray())
        {
            var row = entry!.DeepClone().AsObject();
            var idField = row.Single(property => uint.TryParse(property.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _));
            var id = uint.Parse(idField.Key, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            row.Remove(idField.Key);
            row["Name"] = idField.Value;
            result.Add(id.ToString(CultureInfo.InvariantCulture), row);
        }

        return result;
    }

    private static void Merge(JsonObject target, JsonObject changes)
    {
        foreach (var (key, value) in changes)
        {
            if (value is JsonObject child && target[key] is JsonObject existing) Merge(existing, child);
            else target[key] = value?.DeepClone();
        }
    }

    private static void Load<T>(JsonNode? node, Dictionary<uint, T> destination) where T : class
    {
        if (node is not JsonObject entries) return;

        foreach (var (key, value) in entries)
        {
            if (value == null) continue;

            var id = uint.Parse(key, CultureInfo.InvariantCulture);
            var entry = value.Deserialize<T>(Options) ?? throw new InvalidOperationException($"Invalid definition {key}");
            destination.Add(id, entry);
        }
    }
}

internal sealed class CombatStatusDefinition
{
    public string Name { get; set; } = "";
    public bool Disabled { get; set; }
    public PeriodicDefinition? TimeProc { get; set; }
    public CombatModifier[] Potency { get; set; } = [];
    public uint ActionId { get; set; }
    public string ApplicationSide { get; set; } = "target";
    public Dictionary<uint, uint> PotencyStatusByAction { get; set; } = new();
    public bool IsDamage => !Disabled && TimeProc?.Type.ToLowerInvariant() is "dot" or "grounddamage";
    public bool IsGround => TimeProc?.Type.Equals("grounddamage", StringComparison.OrdinalIgnoreCase) == true;
}

internal sealed class PeriodicDefinition
{
    public string Type { get; set; } = "";
    public decimal Potency { get; set; }
    public string DamageType { get; set; } = "";
    public PotencyByLevel[] Levels { get; set; } = [];

    public decimal GetPotency(byte level)
    {
        foreach (var range in Levels)
            if (level >= range.MinLevel && level <= range.MaxLevel)
                return range.Potency;

        return Potency;
    }
}

internal sealed class PotencyByLevel
{
    public byte MinLevel { get; set; }
    public byte MaxLevel { get; set; } = byte.MaxValue;
    public decimal Potency { get; set; }
}

internal sealed class CombatActionDefinition
{
    public string Name { get; set; } = "";
    public bool Disabled { get; set; }
    public byte? CalibrationMinLevel { get; set; }
    public DamageDefinition[] Damage { get; set; } = [];
    public bool CanCalibrate => !Disabled && Damage.Length == 1 && Damage[0] is
        { Combo: null, TargetIndex: null, Potency: > 0, Extra.Count: 0 };

    public decimal? GetCalibrationPotency(byte level, byte defaultMinimumLevel, decimal? levelPotency)
    {
        if (!CanCalibrate) return null;

        var entry = Damage[0];

        return level < (CalibrationMinLevel ?? defaultMinimumLevel) ? levelPotency : entry.Potency;
    }
}

internal sealed class DamageDefinition
{
    public decimal Potency { get; set; }
    public decimal? Combo { get; set; }
    public int? TargetIndex { get; set; }
    [JsonExtensionData] public Dictionary<string, JsonElement> Extra { get; set; } = new();
}

internal sealed class CombatModifier
{
    public string Type { get; set; } = "";
    public decimal Amount { get; set; }
    public int? AmountByte { get; set; }
    public string? LimitToDamageType { get; set; }
    public string? LimitToActionCategory { get; set; }
    [JsonExtensionData] public Dictionary<string, JsonElement> Extra { get; set; } = new();
}
