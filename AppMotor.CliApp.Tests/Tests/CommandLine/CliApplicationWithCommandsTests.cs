﻿#region License
// Copyright 2020 AppMotor Framework (https://github.com/skrysmanski/AppMotor)
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

using AppMotor.CliApp.CommandLine;
using AppMotor.CliApp.TestUtils;
using AppMotor.Core.Exceptions;

using Shouldly;

using Xunit;

namespace AppMotor.CliApp.Tests.CommandLine
{
    public sealed class CliApplicationWithCommandsTests
    {
        [Fact]
        public void TestExceptionHandling_Regular()
        {
            var app = new ExceptionTestApplication(throwErrorMessageException: false);

            app.Run("error").ShouldBe(42, app.TerminalOutput);

            app.CaughtException.ShouldNotBeNull();
            app.CaughtException.ShouldBeOfType<InvalidOperationException>();
            app.CaughtException.Message.ShouldBe("This is a test");

            app.TerminalOutput.ShouldContain("This is a test");
            // Indicate the full exception report has been printed
            app.TerminalOutput.ShouldContain("Exception Type: ");
        }

        [Fact]
        public void TestExceptionHandling_ErrorMessageException()
        {
            var app = new ExceptionTestApplication(throwErrorMessageException: true);

            // "1" is used automatically for ErrorMessageException
            app.Run("error").ShouldBe(1, app.TerminalOutput);

            app.CaughtException.ShouldNotBeNull();
            app.CaughtException.ShouldBeOfType<ErrorMessageException>();
            app.CaughtException.Message.ShouldBe("This is an error message.");

            // This must be the only output.
            app.TerminalOutput.Trim().ShouldBe("This is an error message.");
        }

        private sealed class ExceptionTestApplication : TestApplicationWithCommands
        {
            /// <inheritdoc />
            protected override int ExitCodeOnException => 42;

            /// <inheritdoc />
            public ExceptionTestApplication(bool throwErrorMessageException)
                : base(new ErrorCommand(throwErrorMessageException))
            {
            }

            private sealed class ErrorCommand : CliCommand
            {
                private readonly bool m_throwErrorMessageException;

                /// <inheritdoc />
                public ErrorCommand(bool throwErrorMessageException) : base("error")
                {
                    this.m_throwErrorMessageException = throwErrorMessageException;
                }

                /// <inheritdoc />
                protected override CliCommandExecutor Executor => new(Execute);

                private void Execute()
                {
                    if (this.m_throwErrorMessageException)
                    {
                        throw new ErrorMessageException("This is an error message.");
                    }
                    else
                    {
                        throw new InvalidOperationException("This is a test");
                    }
                }
            }
        }
    }
}