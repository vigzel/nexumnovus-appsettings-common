namespace NexumNovus.AppSettings.Common.Secure;

/// <summary>
/// Property with this attribute should be encrypted or stored in a secure location.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class SecretSettingAttribute : Attribute
{
  /// <summary>
  /// Initializes a new instance of the <see cref="SecretSettingAttribute"/> class.
  /// </summary>
  public SecretSettingAttribute()
  {
  }
}
