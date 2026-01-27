namespace NativeMeters.Configuration;

public class DtrSettings
{
    public bool Enabled { get; set; } = false;
    public string FormatString { get; set; } = "[dps:k.1] DPS";
    public bool ShowWhenDisconnected { get; set; } = false;
    public string DisconnectedText { get; set; } = "Disconnected";
    public bool ClickToOpenConfig { get; set; } = true;
}
