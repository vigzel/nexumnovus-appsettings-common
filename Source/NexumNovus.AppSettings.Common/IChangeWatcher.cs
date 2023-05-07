namespace NexumNovus.AppSettings.Common;

using Microsoft.Extensions.Primitives;

/// <summary>
/// Interface for change watcher.
/// </summary>
public interface IChangeWatcher
{
  /// <summary>
  /// Creates a <see cref="IChangeToken" /> that is notified when change occurs.
  /// </summary>
  /// <returns><see cref="IChangeToken" />.</returns>
  IChangeToken Watch();

  /// <summary>
  /// Call to manualy trigger change on <see cref="IChangeToken" />.
  /// </summary>
  /// <param name="newState">New state.</param>
  void TriggerChange(string newState);
}
