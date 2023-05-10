namespace NexumNovus.AppSettings.Common.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Extension methods for registering options with service collection.
/// </summary>
public static class ConfigurationExtensions
{
  /// <summary>
  /// Registers and binds options as POCO class.
  /// This enables you to inject your class T directly insetead of using IOptions&lt;T&gt;.
  /// </summary>
  /// <typeparam name="T">The options type to be configured.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="configuration">Configration source.</param>
  /// <returns>The <see cref="IServiceCollection" />.</returns>
  public static IServiceCollection AddOptionsAsPOCO<T>(this IServiceCollection services, IConfiguration configuration)
    where T : class, new()
  {
    var config = new T();
    configuration.Bind(config);
    services.AddSingleton(config);
    return services;
  }

  /// <summary>
  /// Registers and binds options and options validator IValidateOptions&lt;TOptions&gt;.
  /// Options are expected to implement IValidateOptions&lt;TOptions&gt;.
  /// Options validation is called on service startup.
  /// </summary>
  /// <typeparam name="TOptions">The options type to be configured.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="configSectionName">Configration section name.</param>
  /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that configure calls can be chained in it.</returns>
  public static OptionsBuilder<TOptions> AddOptionsWithValidator<TOptions>(this IServiceCollection services, string configSectionName)
    where TOptions : class, IValidateOptions<TOptions>
    => services.AddOptionsWithValidator<TOptions, TOptions>(configSectionName);

  /// <summary>
  /// Registers and binds options and options validator IValidateOptions&lt;TOptions&gt;.
  /// Options validation is called on service startup.
  /// </summary>
  /// <typeparam name="TOptions">The options type to be configured.</typeparam>
  /// <typeparam name="TOptionsValidator">The options validator type.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="configSectionName">Configration section name.</param>
  /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that configure calls can be chained in it.</returns>
  public static OptionsBuilder<TOptions> AddOptionsWithValidator<TOptions, TOptionsValidator>(this IServiceCollection services, string configSectionName)
    where TOptions : class
    where TOptionsValidator : class, IValidateOptions<TOptions>
  {
    services.AddSingleton<IValidateOptions<TOptions>, TOptionsValidator>();

    return services
      .AddOptions<TOptions>()
      .BindConfiguration(configSectionName)
      .ValidateOnStart();
  }

  /// <summary>
  /// Registers and binds options and options validator IValidateOptionsCustom&lt;TOptions&gt;.
  /// Options are expected to implement IValidateOptionsCustom&lt;TOptions&gt;.
  /// Options validation can be started using IOptions&lt;ValidatorOptionsCustom&gt;.
  /// </summary>
  /// <typeparam name="TOptions">The options type to be configured.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="configSectionName">Configration section name.</param>
  /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that configure calls can be chained in it.</returns>
  public static OptionsBuilder<TOptions> AddOptionsWithLateValidator<TOptions>(this IServiceCollection services, string configSectionName)
    where TOptions : class, ILateValidateOptions<TOptions>
    => services.AddOptionsWithLateValidator<TOptions, TOptions>(configSectionName);

  /// <summary>
  /// Registers and binds options and options validator IValidateOptionsCustom&lt;TOptions&gt;.
  /// Options validation can be started using IOptions&lt;ValidatorOptionsCustom&gt;.
  /// </summary>
  /// <typeparam name="TOptions">The options type to be configured.</typeparam>
  /// <typeparam name="TOptionsValidator">The options validator type.</typeparam>
  /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
  /// <param name="configSectionName">Configration section name.</param>
  /// <returns>The <see cref="OptionsBuilder{TOptions}"/> so that configure calls can be chained in it.</returns>
  public static OptionsBuilder<TOptions> AddOptionsWithLateValidator<TOptions, TOptionsValidator>(this IServiceCollection services, string configSectionName)
    where TOptions : class
    where TOptionsValidator : class, ILateValidateOptions<TOptions>
  {
    services.AddSingleton<ILateValidateOptions<TOptions>, TOptionsValidator>();

    var optionsBuilder = services
      .AddOptions<TOptions>()
      .BindConfiguration(configSectionName);

    // following code is equivalent to ValidateOnStart, but this way it gives me more control on when to call validation
    // this enables me to inject IOptions<ValidatorOptionsCustom> that will contain all registered validators
    services.AddOptions<LateValidatorOptions>()
      .Configure<ILateValidateOptions<TOptions>, IOptionsMonitor<TOptions>>((vo, validator, options) =>
      {
        // configure is called when IOptions<ValidatorOptionsCustom>.Value is callled.
        // configure will just fill ValidatorOptionsCustom.Validators dictionary with all option validators
        // calling method is then expected to call "validate" on each member of that dictionary
        vo.Validators[(typeof(TOptions), optionsBuilder.Name)] = () =>
        {
          var result = validator.Validate(optionsBuilder.Name, options.Get(optionsBuilder.Name));
          if (result is not null && result.Failed)
          {
            throw new OptionsValidationException(optionsBuilder.Name, typeof(TOptions), result.Failures);
          }
        };
      });

    return optionsBuilder;
  }
}
