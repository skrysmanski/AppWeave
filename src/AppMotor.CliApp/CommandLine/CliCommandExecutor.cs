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
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace AppMotor.CliApp.CommandLine
{
    /// <summary>
    /// Represents the main/execute method of a <see cref="CliCommand"/> (via <see cref="CliCommand.Executor"/>).
    /// The main purpose of this class is to give the user the freedom to choose between a synchronous or <c>async</c>
    /// method and to choose between <c>void</c>, <c>bool</c>, and <c>int</c> as return type.
    /// </summary>
    public class CliCommandExecutor
    {
        // START MARKER: Generated code

        //
        // NOTE: The code of this class has been generated with the 'ExecutorGenerator' tool. Do
        //   not make manual changes to this class or they may get lost (by accident) when the code
        //   for this class is generated the next time!!!
        //

        private readonly Func<CancellationToken, Task<int>> _action;

        /// <summary>
        /// Creates an executor for a method that: is synchronous, and returns no exit code (<c>void</c>).
        ///
        /// <para>The exit code is always 0.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Action action)
        {
            this._action = _ =>
            {
                action();
                return Task.FromResult(0);
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is synchronous, takes in the application cancellation token, and returns no exit code (<c>void</c>).
        ///
        /// <para>The exit code is always 0.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Action<CancellationToken> action)
        {
            this._action = cancellationToken =>
            {
                action(cancellationToken);
                return Task.FromResult(0);
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is synchronous, and returns an exit code.
        ///
        /// <para>The return value is directly taken as exit code.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<int> action)
        {
            this._action = _ =>
            {
                int retVal = action();
                return Task.FromResult(retVal);
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is synchronous, takes in the application cancellation token, and returns an exit code.
        ///
        /// <para>The return value is directly taken as exit code.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<CancellationToken, int> action)
        {
            this._action = cancellationToken =>
            {
                int retVal = action(cancellationToken);
                return Task.FromResult(retVal);
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is synchronous, and returns a success <c>bool</c>.
        ///
        /// <para>The return value of <c>true</c> is translated into the exit code 0; <c>false</c> is translated into 1.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<bool> action)
        {
            this._action = _ =>
            {
                bool retVal = action();
                return Task.FromResult(retVal ? 0 : 1);
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is synchronous, takes in the application cancellation token, and returns a success <c>bool</c>.
        ///
        /// <para>The return value of <c>true</c> is translated into the exit code 0; <c>false</c> is translated into 1.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<CancellationToken, bool> action)
        {
            this._action = cancellationToken =>
            {
                bool retVal = action(cancellationToken);
                return Task.FromResult(retVal ? 0 : 1);
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is asynchronous, and returns no exit code (<c>Task</c>/<c>void</c>).
        ///
        /// <para>The exit code is always 0.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<Task> action)
        {
            this._action = async _ =>
            {
                await action().ConfigureAwait(continueOnCapturedContext: false);
                return 0;
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is asynchronous, takes in the application cancellation token, and returns no exit code (<c>Task</c>/<c>void</c>).
        ///
        /// <para>The exit code is always 0.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<CancellationToken, Task> action)
        {
            this._action = async cancellationToken =>
            {
                await action(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                return 0;
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is asynchronous, and returns an exit code.
        ///
        /// <para>The return value is directly taken as exit code.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<Task<int>> action)
        {
            this._action = async _ =>
            {
                int retVal = await action().ConfigureAwait(continueOnCapturedContext: false);
                return retVal;
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is asynchronous, takes in the application cancellation token, and returns an exit code.
        ///
        /// <para>The return value is directly taken as exit code.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<CancellationToken, Task<int>> action)
        {
            this._action = action;
        }

        /// <summary>
        /// Creates an executor for a method that: is asynchronous, and returns a success <c>bool</c>.
        ///
        /// <para>The return value of <c>true</c> is translated into the exit code 0; <c>false</c> is translated into 1.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<Task<bool>> action)
        {
            this._action = async _ =>
            {
                bool retVal = await action().ConfigureAwait(continueOnCapturedContext: false);
                return retVal ? 0 : 1;
            };
        }

        /// <summary>
        /// Creates an executor for a method that: is asynchronous, takes in the application cancellation token, and returns a success <c>bool</c>.
        ///
        /// <para>The return value of <c>true</c> is translated into the exit code 0; <c>false</c> is translated into 1.</para>
        /// </summary>
        [PublicAPI]
        public CliCommandExecutor(Func<CancellationToken, Task<bool>> action)
        {
            this._action = async cancellationToken =>
            {
                bool retVal = await action(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                return retVal ? 0 : 1;
            };
        }

        /// <summary>
        /// Executes this executor and returns the exit code.
        /// </summary>
        public async Task<int> Execute(CancellationToken cancellationToken)
        {
            return await this._action(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }

        // END MARKER: Generated code
    }
}
