namespace NexumNovus.AppSettings.Common;

/// <summary>
/// Contains information about a load exception.
/// </summary>
public class SettingsLoadExceptionContext
{
  /// <summary>
  /// Gets or sets the exception that occurred in Load.
  /// </summary>
  public Exception Exception { get; set; } = null!;

  /// <summary>
  /// Gets or sets a value indicating whether the exception will not be rethrown.
  /// </summary>
  public bool Ignore { get; set; }
}
