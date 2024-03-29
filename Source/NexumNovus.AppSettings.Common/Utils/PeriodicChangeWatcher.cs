[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("NexumNovus.AppSettings.Common.Test")]

namespace NexumNovus.AppSettings.Common.Utils;

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Periodicaly checks for changes by calling getNewState every refreshInterval, and comparing the new state with the previous one.
/// </summary>
internal sealed class PeriodicChangeWatcher : IChangeWatcher, IDisposable
{
  private string? _state;
  private readonly Func<string?> _getNewState;
  private readonly TimeSpan _refreshInterval;
  private readonly ILogger? _logger;

  private IDisposable? _timerSubscription;
  private bool _timerInitialized;
  private object _timerLock = new();
  private readonly Func<IDisposable?> _timerFactory;

  private ChangeTokenInfo _changeTokenInfo;

  /// <summary>
  /// Initializes a new instance of the <see cref="PeriodicChangeWatcher"/> class.
  /// </summary>
  /// <param name="getNewState">Callback method to call to get new state.</param>
  /// <param name="initialState">Initial state. Optional.</param>
  /// <param name="refreshInterval">Refresh interval. Default is 60 seconds.</param>
  /// <param name="logger">Logger. Optional.</param>
  /// <param name="scheduler">Scheduler. Optional, this parameter is used in unit tests.</param>
  public PeriodicChangeWatcher(Func<string?> getNewState, string? initialState = null, TimeSpan? refreshInterval = null, ILogger? logger = null, IScheduler? scheduler = null)
  {
    _getNewState = getNewState;
    _state = initialState;
    _refreshInterval = refreshInterval ?? TimeSpan.FromSeconds(60);
    _logger = logger;

    _changeTokenInfo = new ChangeTokenInfo();
    _timerFactory = () =>
    {
      if (_refreshInterval == TimeSpan.Zero)
      {
        return null;
      }

      var timerScheduler = scheduler ?? Scheduler.Default;

      return Observable
        .Interval(_refreshInterval, timerScheduler)
        .Subscribe(CheckHasChanged);
    };
  }

  /// <inheritdoc/>
  public IChangeToken Watch()
  {
    LazyInitializer.EnsureInitialized(ref _timerSubscription, ref _timerInitialized, ref _timerLock, _timerFactory);
    return _changeTokenInfo.ChangeToken;
  }

  /// <inheritdoc/>
  public void TriggerChange(string? newState)
  {
    _logger?.LogDebug($"[PeriodicChangeWatcher] TriggerChange for new state {newState}");
    UpdateState(newState);
  }

  private void CheckHasChanged(long value)
  {
    _logger?.LogTrace("[PeriodicChangeWatcher] Checking for changes...");

    try
    {
      var newState = _getNewState();
      if (newState != _state)
      {
        UpdateState(newState);
      }
    }
    catch (Exception ex)
    {
      _logger?.LogError(ex, $"[PeriodicChangeWatcher] Exception raised. {ex.Message}");
    }
  }

  private void UpdateState(string? newState)
  {
    _logger?.LogTrace($"[PeriodicChangeWatcher] Updating state. PreviousState = {_state}, NewState = {newState}.");

    var changeTokenInfo = _changeTokenInfo;

    // create new token (because we are canceling source of current one)
    // important to do this before triggering change on the old token!!!
    // ako bi tu samo updateao state onda bi mi se ChangeToken.OnChange zavrtio u beskonacnoj petlji jer mu je cancelation token cancelan
    _changeTokenInfo = new ChangeTokenInfo();

    _state = newState;
    changeTokenInfo.TriggerChange();
  }

  private readonly struct ChangeTokenInfo : IDisposable
  {
    public ChangeTokenInfo()
    {
      TokenSource = new CancellationTokenSource();
      ChangeToken = new CancellationChangeToken(TokenSource.Token);
    }

    public CancellationTokenSource TokenSource { get; }

    public CancellationChangeToken ChangeToken { get; }

    public void TriggerChange()
    {
      TokenSource?.Cancel();
      TokenSource?.Dispose();
    }

    public void Dispose() => TokenSource?.Dispose();
  }

  #region Disposable

  private bool _disposed;

  /// <summary>
  /// Dispose the object.
  /// </summary>
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Dispose the object.
  /// </summary>
  /// <param name="disposing"><c>true</c> if invoked from <see cref="IDisposable.Dispose"/>.</param>
  private void Dispose(bool disposing)
  {
    if (_disposed)
    {
      return;
    }

    if (disposing)
    {
      _timerSubscription?.Dispose();
      _changeTokenInfo.Dispose();
    }

    _disposed = true;
  }

  #endregion
}
