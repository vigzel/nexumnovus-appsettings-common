namespace NexumNovus.AppSettings.Common.Utils;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides lazy initialization routines.
///
/// Inspired by LazyInitializer.
/// </summary>
public static class LazyAction
{
  /// <summary>
  /// Executes action if the action has not already been executed.
  /// </summary>
  /// <param name="initialized">A reference to a location tracking whether the action has been executed.</param>
  /// <param name="syncLock">
  /// A reference to a location containing a mutual exclusive lock. If <paramref name="syncLock"/> is null,
  /// a new object will be instantiated.
  /// </param>
  /// <param name="init">Action to be executed.</param>
#pragma warning disable IDE0280 // Use 'nameof'
  public static void EnsureInitialized(ref bool initialized, [NotNullIfNotNull("syncLock")] ref object? syncLock, Action init)
#pragma warning restore IDE0280 // Use 'nameof'
  {
    // Fast path.
    if (Volatile.Read(ref initialized))
    {
      return;
    }

    // Lazily initialize the lock if necessary and then double check if initialization is still required.
    lock (EnsureLockInitialized(ref syncLock))
    {
      if (!Volatile.Read(ref initialized))
      {
        init();
        Volatile.Write(ref initialized, true);
      }
    }
  }

  private static object EnsureLockInitialized([NotNull] ref object? syncLock) =>
      syncLock ??
      Interlocked.CompareExchange(ref syncLock, new object(), null) ??
      syncLock;
}
