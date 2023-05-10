namespace NexumNovus.AppSettings.Common.Validators;
using Microsoft.Extensions.Options;

/// <summary>
/// Interface used to validate options.
/// </summary>
/// <typeparam name="TOptions">The options type to validate.</typeparam>
public interface ILateValidateOptions<TOptions> : IValidateOptions<TOptions>
  where TOptions : class
{
}
