﻿#region License
// Copyright 2021 AppMotor Framework (https://github.com/skrysmanski/AppMotor)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;

using AppMotor.Core.IO;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppMotor.CliApp.CommandLine.Hosting
{
    /// <summary>
    /// <para>The default <see cref="IHostBuilderFactory"/> implementation. Lets you customize the host by
    /// setting the various properties in this class (or even by overriding <see cref="CreateHostBuilder"/>).</para>
    ///
    /// <para>By default, this factory creates hosts with the following features enabled:</para>
    ///
    /// <list type="bullet">
    ///     <item><description>Dependency injection (via <see cref="ServiceProviderConfigurationProvider"/>)</description></item>
    ///     <item><description>Configuration values loaded from "appsettings.json", "appsettings.{Env}.json" and the environment variables (via <see cref="AppConfigurationProvider"/>)</description></item>
    ///     <item><description>Logging to the Console (via <see cref="LoggingConfigurationProvider"/>)</description></item>
    ///     <item><description>Logging configuration via the "Logging" section (via <see cref="LoggingConfigurationSectionName"/>)</description></item>
    ///     <item><description>The content root is set to the current directory (via <see cref="ContentRoot"/>)</description></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// For more possibilities, see <see cref="Host.CreateDefaultBuilder(string[])"/>.
    /// </remarks>
    /// <seealso cref="MethodHostBuilderFactory"/>
    public class DefaultHostBuilderFactory : IHostBuilderFactory
    {
        /// <summary>
        /// An instance of this class.
        /// </summary>
        public static DefaultHostBuilderFactory Instance { get; } = new();

        /// <summary>
        /// The configures the <see cref="IServiceProviderFactory{TContainerBuilder}"/> (i.e. the dependency injection system) by
        /// calling one of the <c>UseServiceProviderFactory()</c> methods on the provided <see cref="IHostBuilder"/> instance.
        /// Defaults to <see cref="ApplyDefaultServiceProviderConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// <para>For more details, see: https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection </para>
        ///
        /// <para>This is an action (rather than a function that returns the service provider) because <see cref="IServiceProviderFactory{TContainerBuilder}"/>
        /// is generic and its type parameter may not always be <see cref="IServiceCollection"/> for all service providers - and we could
        /// not provide this flexibility with a function (because then we would need to hard code the type of <c>TContainerBuilder</c>).</para>
        /// </remarks>
        [PublicAPI]
        public Action<IHostBuilder> ServiceProviderConfigurationProvider { get; init; } = ApplyDefaultServiceProviderConfiguration;

        /// <summary>
        /// Configures the configuration providers (e.g. settings files) that provide configuration values for the application. Defaults to
        /// <see cref="ApplyDefaultAppConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// For more details, see: https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration
        /// </remarks>
        [PublicAPI]
        public Action<HostBuilderContext, IConfigurationBuilder>? AppConfigurationProvider { get; init; } = ApplyDefaultAppConfiguration;

        /// <summary>
        /// The name of the configuration section (<see cref="IConfiguration.GetSection"/>) used to configure log levels, etc. for
        /// all loggers that are enabled via <see cref="LoggingConfigurationProvider"/>. Defaults to "Logging" (the .NET default).
        /// Can be set to <c>null</c> to disable setting the section.
        /// </summary>
        /// <remarks>
        /// For more details, see: https://docs.microsoft.com/en-us/dotnet/core/extensions/logging#configure-logging
        /// </remarks>
        [PublicAPI]
        public string? LoggingConfigurationSectionName { get; init; } = "Logging";

        /// <summary>
        /// Configures the logging for the application. You can use the various <c>loggingBuilder.Add...()</c>
        /// methods to configure the desired logging. Defaults to <see cref="ApplyDefaultLoggingConfiguration"/>.
        /// Note that the configuration section for configuring the log levels etc. is specified via
        /// <see cref="LoggingConfigurationSectionName"/>.
        /// </summary>
        /// <remarks>
        /// For more details, see https://docs.microsoft.com/en-us/dotnet/core/extensions/logging-providers and
        /// https://docs.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter
        /// </remarks>
        [PublicAPI]
        public Action<HostBuilderContext, ILoggingBuilder>? LoggingConfigurationProvider { get; init; } = ApplyDefaultLoggingConfiguration;

        /// <summary>
        /// The content root to use. Defaults to <see cref="DirectoryPath.GetCurrentDirectory"/>. Can later be accessed
        /// via <see cref="IHostEnvironment.ContentRootFileProvider"/>. Can be <c>null</c> in which case no content root
        /// will be set (explicitly).
        /// </summary>
        /// <remarks>
        /// For more details on the content root, see: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/#content-root
        /// </remarks>
        /// <seealso cref="HostingHostBuilderExtensions.UseContentRoot"/>
        [PublicAPI]
        public DirectoryPath? ContentRoot { get; init; } = DirectoryPath.GetCurrentDirectory();

        /// <inheritdoc />
        public virtual IHostBuilder CreateHostBuilder()
        {
            var hostBuilder = new HostBuilder();

            var contentRoot = this.ContentRoot;
            if (contentRoot is not null)
            {
                hostBuilder.UseContentRoot(contentRoot.Value.Value);
            }

            this.ServiceProviderConfigurationProvider(hostBuilder);

            if (this.AppConfigurationProvider is not null)
            {
                hostBuilder.ConfigureAppConfiguration(this.AppConfigurationProvider);
            }

            if (this.LoggingConfigurationSectionName is not null)
            {
                hostBuilder.ConfigureLogging((context, loggingBuilder) =>
                {
                    // Load the logging configuration from the specified configuration section.
                    loggingBuilder.AddConfiguration(context.Configuration.GetSection(this.LoggingConfigurationSectionName));
                });
            }

            if (this.LoggingConfigurationProvider is not null)
            {
                hostBuilder.ConfigureLogging(this.LoggingConfigurationProvider);
            }

            return hostBuilder;
        }

        /// <summary>
        /// Creates a <see cref="DefaultServiceProviderFactory"/> with all scope validations enabled (see <see cref="ServiceProviderOptions.ValidateScopes"/>)
        /// and sets it as service provider.
        /// </summary>
        /// <seealso cref="ServiceProviderConfigurationProvider"/>
        [PublicAPI]
        public static void ApplyDefaultServiceProviderConfiguration(IHostBuilder hostBuilder)
        {
            var options = new ServiceProviderOptions()
            {
                // Enable all validations
                ValidateScopes = true,
                ValidateOnBuild = true,
            };

            hostBuilder.UseServiceProviderFactory(new DefaultServiceProviderFactory(options));
        }

        /// <summary>
        /// Enables the configuration files "appsettings.json" and "appsettings.{<see cref="HostBuilderContext.HostingEnvironment"/>}.json".
        /// Also enables loading configuration values from the environment variables (via <see cref="EnvironmentVariablesConfigurationSource"/>).
        /// </summary>
        /// <remarks>
        /// Whether the .json configuration files are reloaded when changed is configured via the "hostBuilder:reloadConfigOnChange" configuration
        /// value. The default is <c>true</c>.
        /// </remarks>
        /// <seealso cref="AppConfigurationProvider"/>
        [PublicAPI]
        public static void ApplyDefaultAppConfiguration(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            IHostEnvironment env = context.HostingEnvironment;

            bool reloadOnChange = context.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);

            configurationBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange);
            configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);

            configurationBuilder.Add(new EnvironmentVariablesConfigurationSource());
        }

        /// <summary>
        /// Enables Console logging.
        /// </summary>
        /// <seealso cref="LoggingConfigurationProvider"/>
        /// <seealso cref="LoggingConfigurationSectionName"/>
        [PublicAPI]
        public static void ApplyDefaultLoggingConfiguration(HostBuilderContext context, ILoggingBuilder loggingBuilder)
        {
            loggingBuilder.AddConsole();
        }
    }
}
