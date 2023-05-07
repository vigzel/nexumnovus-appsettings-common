namespace NexumNovus.AppSettings.Common.Utils;

using System.Text;
using Newtonsoft.Json;
using NexumNovus.AppSettings.Common.Secure;

/// <summary>
/// Application settings utility class.
/// </summary>
public static class AppSettingsParser
{
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
  /// <param name="name">Name of the settings object.</param>
  /// <param name="jsonSecretAction"><see cref="SecretAttributeAction" />.</param>
  /// <param name="secretProtector"><see cref="ISecretProtector" /> implementation to be used to protect properties with <see cref="SecretSettingAttribute" /> attribute. Default is <see cref="DefaultSecretProtector" />.</param>
  /// <returns>Key value pairs representing settings.</returns>
  public static IDictionary<string, string?> Flatten(object settings, string name, SecretAttributeAction jsonSecretAction, ISecretProtector? secretProtector = null)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new ArgumentNullException(nameof(name), "Name of a setting that is being flattened is required!");
    }

    secretProtector ??= DefaultSecretProtector.Instance;
    var jsonSerializerSettings = new JsonSerializerSettings()
    {
      ContractResolver = new JsonSecretContractResolver(jsonSecretAction, secretProtector),
      Formatting = Formatting.Indented,
    };

    var json = JsonConvert.SerializeObject(settings, jsonSerializerSettings);
    json = $"{{ \"{name}\": {json} }}";
    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
    return JsonConfigurationFileParser.Parse(stream);
  }

  /// <summary>
  /// Serializes the specified object to a JSON string
  ///
  /// Properties with JsonSecret attribute will be MarkedWithStar and cryptographically protected.
  /// </summary>
  /// <param name="settings">Object to be serialized.</param>
  /// <param name="secretProtector"><see cref="ISecretProtector" /> implementation to be used to protect properties with <see cref="SecretSettingAttribute" /> attribute. Default is <see cref="DefaultSecretProtector" />.</param>
  /// <returns>JSON string.</returns>
  public static string SerializeObject(object settings, ISecretProtector? secretProtector = null)
  {
    secretProtector ??= DefaultSecretProtector.Instance;
    var jsonSerializerSettings = new JsonSerializerSettings()
    {
      ContractResolver = new JsonSecretContractResolver(SecretAttributeAction.MarkWithStarAndProtect, secretProtector),
      Formatting = Formatting.Indented,
    };

    return JsonConvert.SerializeObject(settings, jsonSerializerSettings);
  }
}
