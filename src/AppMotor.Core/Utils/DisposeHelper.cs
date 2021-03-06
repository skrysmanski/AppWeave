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
using System.Threading.Tasks;

namespace AppMotor.Core.Utils
{
    /// <summary>
    /// Helper methods for working with <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/>.
    /// </summary>
    public static class DisposeHelper
    {
        /// <summary>
        /// Checks if the <paramref name="disposable"/> is also <see cref="IAsyncDisposable"/> and uses
        /// <see cref="IAsyncDisposable.DisposeAsync"/>, if it is - or <see cref="IDisposable.Dispose"/>
        /// if it's not.
        /// </summary>
        public static async Task DisposeWithAsyncSupport(IDisposable disposable)
        {
            if (disposable is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                disposable.Dispose();
            }
        }
    }
}
