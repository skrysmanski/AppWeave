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
using System.Threading;

using AppMotor.CliApp.CommandLine;
using AppMotor.CliApp.CommandLine.Hosting;
using AppMotor.CliApp.TestUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Shouldly;

using Xunit;

namespace AppMotor.CliApp.Tests.CommandLine.Hosting
{
    public sealed class GenericHostCliCommandTests
    {
        [Fact]
        public void Test_DefaultGenericHost_NotStoppingOnItsOwn()
        {
            // The number of seconds to wait for the stopping event not to happen to "deduce"
            // that it would not fire on its own. (This test is not 100% reliable but it's better
            // than nothing.)
            const int MAX_WAIT_SECONDS_FOR_CONFIRMATION = 2;

            // Setup
            var command = new GenericHostTestCommand();
            var testApp = new TestApplicationWithCommand(command);

            using var startedEvent = new ManualResetEventSlim();
            using var stoppingEvent = new ManualResetEventSlim();
            using var stoppedEvent = new ManualResetEventSlim();

            // ReSharper disable once AccessToDisposedClosure
            command.LifetimeEvents.Started.RegisterEventHandler(() => startedEvent.Set()).ShouldNotBeNull();
            // ReSharper disable once AccessToDisposedClosure
            command.LifetimeEvents.Stopping.RegisterEventHandler(() => stoppingEvent.Set()).ShouldNotBeNull();
            // ReSharper disable once AccessToDisposedClosure
            command.LifetimeEvents.Stopped.RegisterEventHandler(() => stoppedEvent.Set()).ShouldNotBeNull();

            var appTask = testApp.RunAsync();

            try
            {
                command.LifetimeEvents.CancellationToken.IsCancellationRequested.ShouldBe(false);

                startedEvent.Wait(TimeSpan.FromSeconds(10)).ShouldBe(true);

                // Test
                stoppingEvent.Wait(TimeSpan.FromSeconds(MAX_WAIT_SECONDS_FOR_CONFIRMATION)).ShouldBe(false); // Stopping event was not triggered within 2 seconds
            }
            finally
            {
                // Cleanup
                command.Stop();

                // Verify
                stoppingEvent.IsSet.ShouldBe(true);
                command.LifetimeEvents.CancellationToken.IsCancellationRequested.ShouldBe(true);

                appTask.Wait(TimeSpan.FromSeconds(30)).ShouldBe(true);

                // Verify
                stoppedEvent.IsSet.ShouldBe(true);
            }
        }

        private sealed class GenericHostTestCommand : GenericHostCliCommand
        {
        }

        [Fact]
        public void Test_ServiceProvider()
        {
            // Setup
            var command = new GenericHostCommandWithServiceProvider();
            var testApp = new TestApplicationWithCommand(command);

            using var startedEvent = new ManualResetEventSlim();

            // ReSharper disable once AccessToDisposedClosure
            command.LifetimeEvents.Started.RegisterEventHandler(() => startedEvent.Set()).ShouldNotBeNull();

            var appTask = testApp.RunAsync();

            command.LifetimeEvents.CancellationToken.IsCancellationRequested.ShouldBe(false);

            startedEvent.Wait(TimeSpan.FromSeconds(10)).ShouldBe(true);

            // Test
            var testService = command.ServiceProvider.GetRequiredService<GenericHostCommandWithServiceProvider.ITestService>();
            testService.DoSomething(42).ShouldBe(42);

            var logger = command.ServiceProvider.GetRequiredService<ILogger<GenericHostCommandWithServiceProvider>>();
            logger.LogInformation("abc");

            command.ServiceProvider.GetRequiredService<IGenericHostCliCommandLifetimeEvents>().ShouldBeSameAs(command.LifetimeEvents);

            // Shutdown
            command.Stop();
            command.LifetimeEvents.CancellationToken.IsCancellationRequested.ShouldBe(true);
            appTask.Wait(TimeSpan.FromSeconds(30)).ShouldBe(true);
        }

        private sealed class GenericHostCommandWithServiceProvider : GenericHostCliCommand
        {
            public IServiceProvider ServiceProvider => this.Services;

            /// <inheritdoc />
            protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
            {
                base.ConfigureServices(context, services);

                services.AddSingleton<ITestService, TestService>();
            }

            public interface ITestService
            {
                int DoSomething(int value);
            }

            private sealed class TestService : ITestService
            {
                /// <inheritdoc />
                public int DoSomething(int value)
                {
                    return value;
                }
            }
        }

        [Fact]
        public void Test_ExplicitExecutor()
        {
            const int WAIT_SECONDS_INSIDE_COMMAND = 1;

            // Setup
            var command = new GenericHostCommandWithExplicitExecutor(waitInsideExecute: TimeSpan.FromSeconds(WAIT_SECONDS_INSIDE_COMMAND));
            var testApp = new TestApplicationWithCommand(command);

            using var startedEvent = new ManualResetEventSlim();
            using var stoppingEvent = new ManualResetEventSlim();
            using var stoppedEvent = new ManualResetEventSlim();

            // ReSharper disable once AccessToDisposedClosure
            command.LifetimeEvents.Started.RegisterEventHandler(() => startedEvent.Set()).ShouldNotBeNull();
            // ReSharper disable once AccessToDisposedClosure
            command.LifetimeEvents.Stopping.RegisterEventHandler(() =>
                {
                    command.LifetimeEvents.CancellationToken.IsCancellationRequested.ShouldBe(true);
                    stoppingEvent.Set();
                }
            ).ShouldNotBeNull();
            // ReSharper disable once AccessToDisposedClosure
            command.LifetimeEvents.Stopped.RegisterEventHandler(() => stoppedEvent.Set()).ShouldNotBeNull();

            command.LifetimeEvents.CancellationToken.IsCancellationRequested.ShouldBe(false);

            var appTask = testApp.RunAsync();

            startedEvent.Wait(TimeSpan.FromSeconds(10)).ShouldBe(true);

            // Test
            appTask.Wait(TimeSpan.FromSeconds(WAIT_SECONDS_INSIDE_COMMAND * 3)).ShouldBe(true);

            // Verify
            stoppingEvent.IsSet.ShouldBe(true);
            stoppedEvent.IsSet.ShouldBe(true);
        }

        private sealed class GenericHostCommandWithExplicitExecutor : GenericHostCliCommand
        {
            /// <inheritdoc />
            protected override CliCommandExecutor ExplicitExecutor => new(Execute);

            private readonly TimeSpan _waitInsideExecute;

            /// <inheritdoc />
            public GenericHostCommandWithExplicitExecutor(TimeSpan waitInsideExecute)
            {
                this._waitInsideExecute = waitInsideExecute;
            }

            private void Execute()
            {
                Thread.Sleep(this._waitInsideExecute);
            }
        }

        [Fact]
        public void TestCustomHostBuilderFactory()
        {
            // Setup
            var command = new CommandWithCustomHostBuilderFactory();
            var testApp = new TestApplicationWithCommand(command);

            command.LifetimeEvents.Started.RegisterEventHandler(() => command.Stop()).ShouldNotBeNull();

            // Test
            testApp.Run();

            // Verify
            command.CustomHostBuilderFactoryCalled.ShouldBe(true);
        }

        private sealed class CommandWithCustomHostBuilderFactory : GenericHostCliCommand
        {
            public bool CustomHostBuilderFactoryCalled { get; private set; }

            /// <inheritdoc />
            protected override IHostBuilderFactory HostBuilderFactory { get; }

            /// <inheritdoc />
            public CommandWithCustomHostBuilderFactory()
            {
                this.HostBuilderFactory = new MethodHostBuilderFactory(
                    () =>
                    {
                        this.CustomHostBuilderFactoryCalled = true;
                        return new HostBuilder();
                    }
                );
            }
        }
    }
}
