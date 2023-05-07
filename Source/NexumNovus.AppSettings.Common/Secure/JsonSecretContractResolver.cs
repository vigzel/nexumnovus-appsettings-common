namespace NexumNovus.AppSettings.Common.Secure;

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NexumNovus.AppSettings.Common.Utils;

/// <summary>
/// Ignores read-only properties.
/// Optional processing for properties with [JsonSecret] attribute, depending on SecretAttributeAction value
///   - adds '*' to property name ('*' at the end of property name will later signal to deserializer *ConfigurationProvider that this property needs to be decrypted).
/// </summary>
internal sealed class JsonSecretContractResolver : IgnoreReadOnlyPropertiesResolver
{
  private readonly SecretAttributeAction _secretAttributeAction;

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonSecretContractResolver"/> class.
  /// </summary>
  /// <param name="secretAttributeAction">Action to take on properties with <see cref="SecretAttributeAction"/> sttribute.</param>
  public JsonSecretContractResolver(SecretAttributeAction secretAttributeAction = SecretAttributeAction.MarkWithStar) => _secretAttributeAction = secretAttributeAction;

  /// <inheritdoc/>
  protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
  {
    var property = base.CreateProperty(member, memberSerialization);
    if (member.GetCustomAttribute<SecretSettingAttribute>() != null)
    {
      if (_secretAttributeAction == SecretAttributeAction.MarkWithStar)
      {
        property.PropertyName += "*";
      }
    }

    return property;
  }
}
