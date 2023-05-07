namespace NexumNovus.AppSettings.Common.Secure;
using System;
using Newtonsoft.Json.Serialization;

/// <summary>
/// Protects values using <see cref="ISecretProtector"/>.
/// </summary>
internal sealed class SecureValueProvider : IValueProvider
{
  private readonly ISecretProtector _secretProtector;
  private readonly IValueProvider _baseProvider;

  /// <summary>
  /// Initializes a new instance of the <see cref="SecureValueProvider"/> class.
  /// </summary>
  /// <param name="secretProtector"><see cref="ISecretProtector"/>.</param>
  /// <param name="provider"><see cref="IValueProvider"/>.</param>
  public SecureValueProvider(ISecretProtector secretProtector, IValueProvider? provider)
  {
    _secretProtector = secretProtector;
    _baseProvider = provider ?? throw new ArgumentNullException(nameof(provider));
  }

  /// <inheritdoc/>
  public object? GetValue(object target)
  {
    var value = _baseProvider.GetValue(target)?.ToString();
    return string.IsNullOrEmpty(value)
      ? value
      : _secretProtector.Protect(value);
  }

  /// <inheritdoc/>
  public void SetValue(object target, object? value) => _baseProvider.SetValue(target, value);
}
