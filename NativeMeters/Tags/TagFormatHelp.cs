namespace NativeMeters.Tags;

public static class TagFormatHelp
{
    public const string BasicTooltip =
        "Format syntax: [tag_part: modifier.precision]\n" +
        "- :r = Raw (no commas)\n" +
        "- :c = Compact units\n" +
        "- :k/:m = Kilo/Mega units\n" +
        "- .N = Decimals\n" +
        "Example: [dps:c.1] -> 12.3K";

    public const string ComponentTooltip =
        "Format syntax: [tag_part: modifier.precision]\n" +
        "- :r = Raw (no commas)\n" +
        "- :c = Compact units\n" +
        "- :k/:m = Kilo/Mega units\n" +
        "- .N = Decimals or Text length\n" +
        "- _first/_last = Name parts\n" +
        "- _skill/_val = MaxHit parts\n\n" +
        "Example: [name_first.1].:[dps:c.1] -> J.: 12.3K";
}
