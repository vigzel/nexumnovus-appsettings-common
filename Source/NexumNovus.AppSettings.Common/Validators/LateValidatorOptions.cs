namespace NexumNovus.AppSettings.Common.Validators;
using System;
using System.Collections.Generic;

/// <summary>
/// Holds a map of each pair of a) options type and b) options name to a method that forces its evaluation.
/// </summary>
public class LateValidatorOptions
{
  /// <summary>
  /// Gets map that contains each pair of a) options type and b) options name to a method that forces its evaluation, e.g. IOptionsMonitor&lt;TOptions&gt;.Get(name).
  /// </summary>
  public IDictionary<(Type OptionsType, string OptionsName), Action> Validators { get; } = new Dictionary<(Type, string), Action>();

  /// <summary>
  /// Validate all registered options of type T.
  /// </summary>
  /// <typeparam name="T">Type of options to validate.</typeparam>
  /// <param name="name">Optional name of options to validate.</param>
  public void Validate<T>(string? name = null)
  {
    foreach (var (option, validate) in Validators)
    {
      if (option.OptionsType == typeof(T) && (string.IsNullOrEmpty(name) || option.OptionsName == name))
      {
        validate();
      }
    }
  }

  /// <summary>
  /// Validate all registered options.
  /// </summary>
  public void ValidateAll()
  {
    foreach (var validate in Validators.Values)
    {
      validate();
    }
  }
}
