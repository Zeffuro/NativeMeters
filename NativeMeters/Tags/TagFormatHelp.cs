namespace NativeMeters.Tags;

public static class TagFormatHelp
{
    public const string BasicTooltip =
        "Format syntax: [tag_part: modifier.precision]\n" +
        "- :r = Raw (no commas)\n" +
        "- :k/:m = Kilo/Mega units\n" +
        "- .N = Decimals\n" +
        "Example: [dps:k.1] -> 12.3k";

    public const string ComponentTooltip =
        "Format syntax: [tag_part: modifier.precision]\n" +
        "- :r = Raw (no commas)\n" +
        "- :k/:m = Kilo/Mega units\n" +
        "- .N = Decimals or Text length\n" +
        "- _first/_last = Name parts\n" +
        "- _skill/_val = MaxHit parts\n\n" +
        "Example: [name_first.1].:[dps:k.1] -> J.: 12.3k";
}
