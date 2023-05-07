namespace NexumNovus.AppSettings.Common;

/// <summary>
/// Interface for settings repository.
/// </summary>
public interface ISettingsRepository
{
  /// <summary>
  /// Updates setting.
  /// </summary>
  /// <param name="name">Name of the setting to be updated.</param>
  /// <param name="settings">Settings object.</param>
  /// <returns>Task to be awaited.</returns>
  Task UpdateSettingsAsync(string name, object settings);
}
