namespace NexumNovus.AppSettings.Common;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NexumNovus.AppSettings.Common.Secure;

/// <summary>
/// Represents a base class for database based <see cref="IConfigurationSource"/>.
/// </summary>
public abstract class NexumDbConfigurationSource : IConfigurationSource
{
  // removes any characters that are not alphanumeric or underscore.
  private readonly Regex _sanitazationRegex = new("[^a-zA-Z0-9_]");

  private string _tableName = "__AppSettings";

  /// <summary>
  /// Gets or Sets the name of the database table that will hold application settings.
  /// </summary>
  public string TableName
  {
    get => _tableName;
    set => _tableName = _sanitazationRegex.Replace(value, string.Empty);
  }

  /// <summary>
  /// Gets or sets the database connection string.
  /// </summary>
  public string ConnectionString { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets a value indicating whether the settings will be reloaded if they are updated in database.
  /// If <see cref="CheckForChangesPeriod"/> is set to TimeSpan.Zero, settings will be reloaded only on updates through <see cref="ISettingsRepository"/>.
  /// </summary>
  public bool ReloadOnChange { get; set; }

  /// <summary>
  /// Gets or sets the change watcher
  /// Default implementation pools database for changes every <see cref="CheckForChangesPeriod"/>.
  /// </summary>
  public IChangeWatcher ChangeWatcher { get; set; } = null!;

  /// <summary>
  /// Gets or sets period.
  /// If set to TimeSpan.Zero (default) database will not be pooled for changes.
  /// </summary>
  public TimeSpan CheckForChangesPeriod { get; set; } = TimeSpan.Zero;

  /// <summary>
  /// Gets or sets protector that encrypts properties with attribute [SecretSetting]
  /// Default implementation uses <see cref="DataProtectionProvider"/>.
  /// </summary>
  public ISecretProtector Protector { get; set; } = null!;

  /// <summary>
  /// Gets or sets will be called if an uncaught exception occurs in ConfigurationProvider.Load.
  /// </summary>
  public Action<SettingsLoadExceptionContext>? OnLoadException { get; set; }

  /// <summary>
  /// Gets or sets logger used to log internal messages.
  /// </summary>
  public ILogger? Logger { get; set; }

  /// <summary>
  /// Builds the <see cref="IConfigurationProvider"/> for this source.
  /// </summary>
  /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
  /// <returns>An <see cref="IConfigurationProvider"/>.</returns>
  public IConfigurationProvider Build(IConfigurationBuilder builder)
  {
    EnsureDefaults();
    return CreateProvider(builder);
  }

  /// <summary>
  /// Creates the <see cref="IConfigurationProvider"/> for this source.
  /// </summary>
  /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
  /// <returns>An <see cref="IConfigurationProvider"/>.</returns>
  protected abstract IConfigurationProvider CreateProvider(IConfigurationBuilder builder);

  /// <summary>
  /// Ensure default values are set.
  /// </summary>
  protected virtual void EnsureDefaults() => Protector ??= DefaultSecretProtector.Instance;
}
