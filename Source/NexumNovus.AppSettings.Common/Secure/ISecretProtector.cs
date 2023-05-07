namespace NexumNovus.AppSettings.Common.Secure;

/// <summary>
/// Provides a mechanism for cryptographically protecting/unprotecting plaintext data.
/// </summary>
public interface ISecretProtector
{
  /// <summary>
  /// Cryptographically protects a piece of plaintext data.
  /// </summary>
  /// <param name="plaintext">The plaintext data to protect.</param>
  /// <returns>The protected form of the plaintext data.</returns>
  string Protect(string plaintext);

  /// <summary>
  /// Cryptographically unprotects a piece of protected data.
  /// </summary>
  /// <param name="protectedData">The protected data to unprotect.</param>
  /// <returns>The plaintext form of the protected data.</returns>
  /// <exception cref="System.Security.Cryptography.CryptographicException">
  /// Thrown if the protected data is invalid or malformed.
  /// </exception>
  string Unprotect(string protectedData);
}
