using System.Text.Json.Serialization;
using CUE4Parse.Encryption.Aes;

namespace LocalFetch.Shared.Settings.Builds.Containers.Aes;

/// <summary>
/// Represents a dynamic AES key.
/// </summary>
public class DynamicKey
{
    public string? Name { get; set; } = string.Empty;
    public string? Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this dynamic key is valid.
    /// </summary>
    [JsonIgnore] public bool IsInvalid =>
        string.IsNullOrEmpty(Key) && Key!.Length == 66;
    
    [JsonIgnore] public FAesKey EncryptionKey => new(Key!);
}