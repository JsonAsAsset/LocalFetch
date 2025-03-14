﻿using System.Collections.Generic;

namespace LocalFetch.Shared.Settings.Builds.Aes;

/// <summary>
/// This container can hold a primary key as well as a collection of dynamic keys.
/// </summary>
public class AesContainer
{
    public string Key { get; set; } = string.Empty;
    public List<DynamicKey> DynamicKeys { get; set; } = [];

    /// <summary>
    /// Indicates if there is at least one dynamic key.
    /// </summary>
    public bool HasDynamicKeys => DynamicKeys.Count > 0;

    /// <summary>
    /// It is valid if the Key is the expected length (66 characters) or if it contains any dynamic keys.
    /// </summary>
    public bool IsValid => Key.Length == 66 || HasDynamicKeys;
}
