namespace NativeMeters.Configuration;

public class SystemConfiguration
{
    public const string FileName = "NativeMeters.json";

    private GeneralSettings _general = new();
    private ConnectionSettings _connection = new();

    public GeneralSettings General
    {
        get => _general;
        set => _general = value ?? new();
    }

    public ConnectionSettings ConnectionSettings
    {
        get => _connection;
        set => _connection = value ?? new();
    }

    /// <summary>
    /// Ensures all nested config objects are initialized. Call after deserialization.
    /// </summary>
    public void EnsureInitialized()
    {
        _general ??= new();
        _connection ??= new();
    }
}