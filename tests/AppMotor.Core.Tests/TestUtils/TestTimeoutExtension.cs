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

using JetBrains.Annotations;

namespace AppMotor.Core.TestUtils
{
    public static class TestTimeoutExtension
    {
        [PublicAPI]
        public static async Task ShouldFinishWithin(this Task task, TimeSpan timeout)
        {
            await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);

            if (!task.IsCompleted)
            {
                throw new TimeoutException();
            }

            await task;
        }

        [PublicAPI]
        public static async Task<T> ShouldFinishWithin<T>(this Task<T> task, TimeSpan timeout)
        {
            await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false);

            if (!task.IsCompleted)
            {
                throw new TimeoutException();
            }

            return await task;
        }
    }
}
