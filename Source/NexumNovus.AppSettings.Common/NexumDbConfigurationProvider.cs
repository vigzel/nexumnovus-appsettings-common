namespace NexumNovus.AppSettings.Common;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NexumNovus.AppSettings.Common.Secure;
using NexumNovus.AppSettings.Common.Utils;

/// <summary>
/// Represents a base class for database based <see cref="ConfigurationProvider"/>.
/// </summary>
/// <typeparam name="T">Type of configuration source.</typeparam>
public abstract class NexumDbConfigurationProvider<T> : ConfigurationProvider, IDisposable
  where T : NexumDbConfigurationSource
{
  private readonly IDisposable? _changeTokenRegistration;

  /// <summary>
  /// Initializes a new instance of the <see cref="NexumDbConfigurationProvider{T}"/> class.
  /// </summary>
  /// <param name="source">Configration source.</param>
  public NexumDbConfigurationProvider(T source)
  {
    Source = source;
    if (source.ReloadOnChange)
    {
      Source.ChangeWatcher = Source.ChangeWatcher ?? new PeriodicChangeWatcher(GetLastUpdateDt, refreshInterval: Source.CheckForChangesPeriod, logger: Source.Logger);

      _changeTokenRegistration = ChangeToken.OnChange(
          () =>
          {
            source.Logger?.LogTrace("[NexumDbConfigurationProvider] Calling ChangeWatcher.Watch.");
            return source.ChangeWatcher.Watch();
          },
          () =>
          {
            source.Logger?.LogTrace("[NexumDbConfigurationProvider] Change detected. Loading settings from DB.");
            Load();
            source.Logger?.LogTrace("[NexumDbConfigurationProvider] Settings reloaded.");
            OnReload(); // without this, settings won't be updated and IOptionsMonitor won't trigger OnChange event (note that OnChange is Disposable and must be unsubscribed from)
          });
    }
  }

  /// <summary>
  /// Gets the source settings for this provider.
  /// </summary>
  public T Source { get; }

  /// <inheritdoc/>
  public override void Load()
  {
    try
    {
      var dbSettings = GetSettingsFromDb();

      var settings = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
      foreach (var sett in dbSettings)
      {
        var key = sett.Key;
        var value = sett.Value;

        if (key.EndsWith('*'))
        {
          key = key[..^1];
          value = UnprotectSafe(key, value);
        }

        settings.Add(key, value);
      }

      Data = settings;
    }
    catch (CryptographicException)
    {
      throw; // CryptographicException was handled inside UnprotectSafe, if we see it here just rethrow it
    }
    catch (Exception e)
    {
      HandleException(ExceptionDispatchInfo.Capture(e));
    }
  }

  /// <summary>
  /// Loads the configuration from database.
  /// </summary>
  /// <returns>The configuration key value pairs.</returns>
  protected abstract Dictionary<string, string?> GetSettingsFromDb();

  /// <summary>
  /// Gets last update date for the configuration.
  /// </summary>
  /// <returns>Configurations last update date.</returns>
  protected abstract string? GetLastUpdateDt();

  private string? UnprotectSafe(string settingName, string? settingValue)
  {
    if (string.IsNullOrWhiteSpace(settingValue))
    {
      return settingValue;
    }

    try
    {
      var secretProtector = Source.Protector ?? DefaultSecretProtector.Instance;
      return secretProtector.Unprotect(settingValue);
    }
    catch (CryptographicException ex)
    {
      HandleException(ExceptionDispatchInfo.Capture(new CryptographicException($"Failed to decrypt value \"{settingValue}\" for \"{settingName}\".", ex)));
      return null; // if we get here, HandleException ignored the error
    }
  }

  private void HandleException(ExceptionDispatchInfo info)
  {
    var ignoreException = false;
    if (Source.OnLoadException != null)
    {
      var exceptionContext = new SettingsLoadExceptionContext
      {
        Exception = info.SourceException,
      };
      Source.OnLoadException.Invoke(exceptionContext);
      ignoreException = exceptionContext.Ignore;
    }

    if (!ignoreException)
    {
      info.Throw();
    }
  }

  #region Dispose

  /// <summary>
  /// Dispose the provider.
  /// </summary>
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Dispose the provider.
  /// </summary>
  /// <param name="disposing"><c>true</c> if invoked from <see cref="IDisposable.Dispose"/>.</param>
  protected virtual void Dispose(bool disposing) => _changeTokenRegistration?.Dispose();

  #endregion
}
