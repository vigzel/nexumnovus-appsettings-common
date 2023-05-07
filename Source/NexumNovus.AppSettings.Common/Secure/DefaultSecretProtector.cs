namespace NexumNovus.AppSettings.Common.Secure;

using System.Reflection;
using Microsoft.AspNetCore.DataProtection;

/// <summary>
/// Used in non-DI aware scenarios for Data Protection.
/// https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/non-di-scenarios?view=aspnetcore-7.0.
/// </summary>
public class DefaultSecretProtector : ISecretProtector
{
  private static DefaultSecretProtector? _instance;
  private readonly IDataProtector _dataProtector;

  /// <summary>
  /// Gets the instance of <see cref="DefaultSecretProtector"/>.
  /// </summary>
  public static DefaultSecretProtector Instance
  {
    get
    {
      _instance ??= new DefaultSecretProtector();
      return _instance;
    }
  }

  private DefaultSecretProtector()
  {
    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
      ?? throw new NotSupportedException("Failed to determine LocalApplicationData folder! It's necessary for data protection of settings.");

    var appName = Assembly.GetEntryAssembly()?.GetName()?.Name ?? "NexumNovus";
    var destFolder = Path.Combine(localAppData, $"{appName}-keys");

    // instantiate the data protection system at this folder
    var dataProtectionProvider = DataProtectionProvider.Create(
          new DirectoryInfo(destFolder),
          configuration => configuration.SetApplicationName(appName));

    _dataProtector = dataProtectionProvider.CreateProtector($"{appName}.AppSettings");
  }

  /// <summary>
  /// Cryptographically protects a piece of plaintext data.
  /// </summary>
  /// <param name="plaintext">The plaintext data to protect.</param>
  /// <returns>The protected form of the plaintext data.</returns>
  public string Protect(string plaintext)
  {
    if (string.IsNullOrWhiteSpace(plaintext))
    {
      return plaintext;
    }

    return _dataProtector.Protect(plaintext);
  }

  /// <summary>
  /// Cryptographically unprotects a piece of protected data.
  /// </summary>
  /// <param name="protectedData">The protected data to unprotect.</param>
  /// <returns>The plaintext form of the protected data.</returns>
  /// <exception cref="System.Security.Cryptography.CryptographicException">
  /// Thrown if <paramref name="protectedData"/> is invalid or malformed.
  /// </exception>
  public string Unprotect(string protectedData)
  {
    if (string.IsNullOrWhiteSpace(protectedData))
    {
      return protectedData;
    }

    return _dataProtector.Unprotect(protectedData);
  }
}
