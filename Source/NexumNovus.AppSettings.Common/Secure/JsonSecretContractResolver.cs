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
  private readonly ISecretProtector? _secretProtector;

  /// <summary>
  /// Initializes a new instance of the <see cref="JsonSecretContractResolver"/> class.
  /// </summary>
  /// <param name="secretAttributeAction">Action to take on properties with <see cref="SecretAttributeAction"/> attribute.</param>
  /// <param name="secretProtector"><see cref="ISecretProtector"/>.</param>
  public JsonSecretContractResolver(SecretAttributeAction secretAttributeAction = SecretAttributeAction.MarkWithStar, ISecretProtector? secretProtector = null)
  {
    if (secretAttributeAction == SecretAttributeAction.MarkWithStarAndProtect && secretProtector == null)
    {
      throw new ArgumentNullException(nameof(secretProtector));
    }

    _secretAttributeAction = secretAttributeAction;
    _secretProtector = secretProtector;
  }

  /// <inheritdoc/>
  protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
  {
    var property = base.CreateProperty(member, memberSerialization);
    if (member.GetCustomAttribute<SecretSettingAttribute>() != null)
    {
      if (_secretAttributeAction is SecretAttributeAction.MarkWithStar or SecretAttributeAction.MarkWithStarAndProtect)
      {
        property.PropertyName += "*";
        if (_secretAttributeAction == SecretAttributeAction.MarkWithStarAndProtect)
        {
          property.ValueProvider = new SecureValueProvider(_secretProtector!, property.ValueProvider);
        }
      }
    }

    return property;
  }
}
