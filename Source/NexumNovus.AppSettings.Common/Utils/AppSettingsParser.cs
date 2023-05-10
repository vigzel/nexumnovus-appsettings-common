namespace NexumNovus.AppSettings.Common.Utils;

using System.IO;
using System.Text;
using Newtonsoft.Json;
using NexumNovus.AppSettings.Common.Secure;

/// <summary>
/// Application settings utility class.
/// </summary>
public static class AppSettingsParser
{
  /// <summary>
  /// Converts json file to a flat key-value settings dictionary.
  /// </summary>
  /// <param name="path">The path to the file.</param>
  /// <returns>Key value pairs representing settings.</returns>
  public static IDictionary<string, string?> Parse(string path)
  {
    using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize: 1, FileOptions.SequentialScan))
    {
      return Parse(stream);
    }
  }

  /// <summary>
  /// Converts json file stream to a flat key-value settings dictionary.
  /// </summary>
  /// <param name="stream">The stream to read.</param>
  /// <returns>Key value pairs representing settings.</returns>
  public static IDictionary<string, string?> Parse(Stream stream) => JsonConfigurationFileParser.Parse(stream);

  /// <summary>
  /// Converts object to a flat key-value dictionary.
  ///
  /// Read only properties are ignored.
  /// Properties with JsonSecret attribute can be Ignored or MarkedWithStar.
  /// </summary>
  /// <param name="settings">Object to be flattened.</param>
  /// <param name="section">Name of the settings object.</param>
  /// <param name="jsonSecretAction"><see cref="SecretAttributeAction" />.</param>
  /// <param name="secretProtector"><see cref="ISecretProtector" /> implementation to be used to protect properties with <see cref="SecretSettingAttribute" /> attribute. Default is <see cref="DefaultSecretProtector" />.</param>
  /// <returns>Key value pairs representing settings.</returns>
  public static IDictionary<string, string?> Flatten(object settings, string section, SecretAttributeAction jsonSecretAction, ISecretProtector? secretProtector = null)
  {
    if (string.IsNullOrWhiteSpace(section))
    {
      throw new ArgumentNullException(nameof(section), "Section of a setting that is being flattened is required!");
    }

    secretProtector ??= DefaultSecretProtector.Instance;
    var jsonSerializerSettings = new JsonSerializerSettings()
    {
      ContractResolver = new JsonSecretContractResolver(jsonSecretAction, secretProtector),
      Formatting = Formatting.Indented,
    };

    var json = JsonConvert.SerializeObject(settings, jsonSerializerSettings);
    json = $"{{ \"{section}\": {json} }}";
    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
    {
      return JsonConfigurationFileParser.Parse(stream);
    }
  }

  /// <summary>
  /// Converts settings dictionary to json string.
  /// </summary>
  /// <param name="settings">Settings dictionary where keys are delimited with ":".</param>
  /// <returns>Json string.</returns>
  public static string ConvertSettingsDictionaryToJson(IDictionary<string, string?> settings)
  {
    var jsonProperties = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase); // dictionary of dictionaries

    foreach (var (key, value) in settings)
    {
      var parts = key.Split(':');
      AddJsonProperty(jsonProperties, parts, value);
    }

    // Find and transform dictionaries with sequential numbered keys to arrays
    foreach (var prop in jsonProperties.Keys)
    {
      if (jsonProperties[prop] is Dictionary<string, object?> dict)
      {
        FindArrays(dict, jsonProperties, prop);
      }
    }

    return JsonConvert.SerializeObject(jsonProperties, new JsonSerializerSettings() { Formatting = Formatting.Indented });
  }

  private static void AddJsonProperty(Dictionary<string, object?> parent, string[] parts, string? value)
  {
    var propertyName = parts[0];
    var remainingParts = parts.Skip(1).ToArray();

    if (!remainingParts.Any())
    {
      parent[propertyName] = value;
      return;
    }

    if (!parent.TryGetValue(propertyName, out var propertyValue))
    {
      propertyValue = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
      parent[propertyName] = propertyValue;
    }

    var nestedProperty = (Dictionary<string, object?>)propertyValue!;
    AddJsonProperty(nestedProperty, remainingParts, value);
  }

  // Transforms dictionaries with sequential numbered keys to arrays
  private static void FindArrays(Dictionary<string, object?> properties, Dictionary<string, object?> parent, string parentName)
  {
    if (properties.ContainsKey("0") && IsSequentialListOfNumbers(properties.Keys))
    {
      parent[parentName] = properties.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
    }

    foreach (var prop in properties.Keys)
    {
      if (properties[prop] is Dictionary<string, object?> dict)
      {
        FindArrays(dict, properties, prop);
      }
    }
  }

  private static bool IsSequentialListOfNumbers(IEnumerable<string> data)
  {
    var numbers = new bool[data.Count()];

    foreach (var item in data)
    {
      if (!int.TryParse(item, out var num) || num < 0 || num >= numbers.Length || numbers[num])
      {
        return false;
      }

      numbers[num] = true;
    }

    return true;
  }
}
