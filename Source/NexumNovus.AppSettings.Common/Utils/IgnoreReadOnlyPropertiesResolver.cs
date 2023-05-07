namespace NexumNovus.AppSettings.Common.Utils;

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Ignores read-only properties.
/// </summary>
internal class IgnoreReadOnlyPropertiesResolver : DefaultContractResolver
{
  /// <inheritdoc/>
  protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
  {
    var property = base.CreateProperty(member, memberSerialization);
    if (!property.Writable)
    {
      property.ShouldSerialize = _ => false;
    }

    return property;
  }
}
