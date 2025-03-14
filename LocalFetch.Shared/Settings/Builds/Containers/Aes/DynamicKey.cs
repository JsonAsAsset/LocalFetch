namespace LocalFetch.Shared.Settings.Builds.Aes;

/// <summary>
/// Represents a dynamic AES key.
/// </summary>
public class DynamicKey
{
    public string? Name { get; set; } = string.Empty;
    public string? Guid { get; set; } = string.Empty;
    public string? Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this dynamic key is valid.
    /// </summary>
    public bool IsValid =>
        !string.IsNullOrEmpty(Guid) && Guid.Length == 32 &&
        !string.IsNullOrEmpty(Key) && Key.Length == 66;
}