using KamiToolKit.BaseTypes;

namespace NativeMeters.Localization;

internal static class WindowTitleExtensions
{
    internal static void SetLocalizedTitle(this NativeAddon addon, string key)
    {
        var (title, subtitle) = Loc.GetWindowHeader(key);
        addon.Title = title;
        addon.Subtitle = subtitle is null ? null : subtitle;
    }
}
