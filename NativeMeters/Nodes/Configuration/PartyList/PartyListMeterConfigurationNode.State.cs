using KamiToolKit.Nodes;
using NativeMeters.Configuration;
using NativeMeters.Configuration.Persistence;
using NativeMeters.Nodes.Input;

namespace NativeMeters.Nodes.Configuration.PartyList;

internal sealed partial class PartyListMeterConfigurationNode
{
    private void ResetPartyListSettings()
    {
        config.ResetToDefaults();
        RefreshControls();
        SaveAndUpdate();
    }

    private void RefreshControls()
    {
        var defaults = new PartyListMeterSettings();

        isLoading = true;

        try
        {
            enabledToggle.IsChecked = config.Enabled;
            showSelfToggle.IsChecked = config.ShowSelf;
            showPartyToggle.IsChecked = config.ShowPartyMembers;
            hideWhenNoDataToggle.IsChecked = config.HideWhenNoCombatData;
            formatInput.Text = config.MemberFormat;
            highlightTopMemberToggle.IsChecked = config.HighlightTopMember;
            topMemberStatDropdown.SelectedOption = config.TopMemberStat;
            topMemberFormatInput.Text = config.TopMemberFormat;
            topMemberTextColorInput.CurrentColor = config.TopMemberTextColor;
            topMemberTextColorInput.DefaultColor = defaults.TopMemberTextColor;
            anchorDropdown.SelectedOption = config.Anchor;
            offsetXInput.Value = config.OffsetX;
            offsetYInput.Value = config.OffsetY;
            widthInput.Value = config.Width;
            heightInput.Value = config.Height;
            memberAlignmentDropdown.SelectedOption = config.MemberAlignment;

            showMemberBarsToggle.IsChecked = config.ShowMemberBars;
            barOffsetXInput.Value = config.BarOffsetX;
            barOffsetYInput.Value = config.BarOffsetY;
            barWidthInput.Value = config.BarWidth;
            barHeightInput.Value = config.BarHeight;
            barTypeDropdown.SelectedOption = config.BarType;
            barFillRightToLeftToggle.IsChecked = config.BarFillRightToLeft;
            barColorModeDropdown.SelectedOption = config.BarColorMode;
            barColorTreatmentDropdown.SelectedOption = config.BarColorTreatment;
            barColorInput.CurrentColor = config.BarColor;
            barColorInput.DefaultColor = defaults.BarColor;
            barBackgroundColorInput.CurrentColor = config.BarBackgroundColor;
            barBackgroundColorInput.DefaultColor = defaults.BarBackgroundColor;

            showRaidDpsToggle.IsChecked = config.ShowRaidDps;
            raidFormatInput.Text = config.RaidDpsFormat;
            raidOffsetXInput.Value = config.RaidOffsetX;
            raidOffsetYInput.Value = config.RaidOffsetY;
            raidWidthInput.Value = config.RaidWidth;
            raidHeightInput.Value = config.RaidHeight;
            raidAlignmentDropdown.SelectedOption = config.RaidAlignment;
            raidFontSizeInput.Value = (int)config.RaidFontSize;
            raidFontTypeDropdown.SelectedOption = config.RaidFontType;
            raidTextFlagsInput.Value = config.RaidTextFlags;
            raidTextColorInput.CurrentColor = config.RaidTextColor;
            raidTextColorInput.DefaultColor = defaults.RaidTextColor;
            raidOutlineColorInput.CurrentColor = config.RaidTextOutlineColor;
            raidOutlineColorInput.DefaultColor = defaults.RaidTextOutlineColor;

            fontSizeInput.Value = (int)config.FontSize;
            fontTypeDropdown.SelectedOption = config.FontType;
            textFlagsInput.Value = config.TextFlags;
            textColorInput.CurrentColor = config.TextColor;
            textColorInput.DefaultColor = defaults.TextColor;
            outlineColorInput.CurrentColor = config.TextOutlineColor;
            outlineColorInput.DefaultColor = defaults.TextOutlineColor;
        }
        finally
        {
            isLoading = false;
        }

        ApplyEnabledState();
        RecalculateLayout();
        UpdateFormatRowLayouts();
        UpdatePairedInputRowLayouts();
        RecalculateLayout();
    }

    private void ApplyEnabledState()
    {
        var isEnabled = config.Enabled;

        SetCheckboxEnabled(showSelfToggle, isEnabled);
        SetCheckboxEnabled(showPartyToggle, isEnabled);
        SetCheckboxEnabled(hideWhenNoDataToggle, isEnabled);
        SetCheckboxEnabled(highlightTopMemberToggle, isEnabled);
        SetCheckboxEnabled(showMemberBarsToggle, isEnabled);
        SetCheckboxEnabled(showRaidDpsToggle, isEnabled);

        var areMemberBarsEnabled = isEnabled && config.ShowMemberBars;
        var isRaidDpsEnabled = isEnabled && config.ShowRaidDps;
        var isTopMemberEnabled = isEnabled && config.HighlightTopMember;

        formatInput.IsEnabled = isEnabled;
        SetButtonEnabled(formatHelpButton, isEnabled);
        SetButtonEnabled(browseTagButton, isEnabled);
        topMemberStatDropdown.IsEnabled = isTopMemberEnabled;
        topMemberFormatInput.IsEnabled = isTopMemberEnabled;
        SetButtonEnabled(topMemberFormatHelpButton, isTopMemberEnabled);
        SetButtonEnabled(topMemberBrowseTagButton, isTopMemberEnabled);
        topMemberTextColorInput.IsEnabled = isTopMemberEnabled;
        anchorDropdown.IsEnabled = isEnabled;
        offsetXInput.IsEnabled = isEnabled;
        offsetYInput.IsEnabled = isEnabled;
        widthInput.IsEnabled = isEnabled;
        heightInput.IsEnabled = isEnabled;
        memberAlignmentDropdown.IsEnabled = isEnabled;
        barOffsetXInput.IsEnabled = areMemberBarsEnabled;
        barOffsetYInput.IsEnabled = areMemberBarsEnabled;
        barWidthInput.IsEnabled = areMemberBarsEnabled;
        barHeightInput.IsEnabled = areMemberBarsEnabled;
        barTypeDropdown.IsEnabled = areMemberBarsEnabled;
        SetCheckboxEnabled(barFillRightToLeftToggle, areMemberBarsEnabled);
        barColorModeDropdown.IsEnabled = areMemberBarsEnabled;
        barColorTreatmentDropdown.IsEnabled = areMemberBarsEnabled;
        barColorInput.IsEnabled = areMemberBarsEnabled && config.BarColorMode == ColorMode.Static;
        barBackgroundColorInput.IsEnabled = areMemberBarsEnabled;
        raidFormatInput.IsEnabled = isRaidDpsEnabled;
        SetButtonEnabled(raidFormatHelpButton, isRaidDpsEnabled);
        SetButtonEnabled(raidBrowseTagButton, isRaidDpsEnabled);
        raidOffsetXInput.IsEnabled = isRaidDpsEnabled;
        raidOffsetYInput.IsEnabled = isRaidDpsEnabled;
        raidWidthInput.IsEnabled = isRaidDpsEnabled;
        raidHeightInput.IsEnabled = isRaidDpsEnabled;
        raidAlignmentDropdown.IsEnabled = isRaidDpsEnabled;
        raidFontSizeInput.IsEnabled = isRaidDpsEnabled;
        raidFontTypeDropdown.IsEnabled = isRaidDpsEnabled;
        raidTextFlagsInput.IsEnabled = isRaidDpsEnabled;
        raidTextColorInput.IsEnabled = isRaidDpsEnabled;
        raidOutlineColorInput.IsEnabled = isRaidDpsEnabled;
        fontSizeInput.IsEnabled = isEnabled;
        fontTypeDropdown.IsEnabled = isEnabled;
        textFlagsInput.IsEnabled = isEnabled;
        textColorInput.IsEnabled = isEnabled;
        outlineColorInput.IsEnabled = isEnabled;
    }

    private static void SetCheckboxEnabled(CheckboxRowNode checkbox, bool isEnabled)
        => checkbox.IsEnabled = isEnabled;

    private static void SetButtonEnabled(CircleButtonNode button, bool isEnabled)
    {
        button.IsEnabled = isEnabled;
        button.Alpha = isEnabled ? 1.0f : 0.45f;
    }

    private void SaveAndUpdate()
    {
        if (isLoading)
            return;

        ConfigRepository.Save(System.Config);
        System.PartyListMeterManager?.UpdateSettings();
    }
}
