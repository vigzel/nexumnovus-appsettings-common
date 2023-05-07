namespace NexumNovus.AppSettings.Common.Secure;

/// <summary>
/// Enum action specifying what action to take on properties with <see cref="SecretSettingAttribute" /> attribute.
/// </summary>
public enum SecretAttributeAction
{
  /// <summary>
  /// Do nothing.
  /// </summary>
  Ignore,

  /// <summary>
  /// Add sufix *.
  /// </summary>
  MarkWithStar,
}
