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

using AppMotor.Core.Utils;

using JetBrains.Annotations;

using Shouldly;

namespace AppMotor.Core.TestUtils
{
    [ShouldlyMethods]
    public static class DateTimeUtcShouldExtensions
    {
        [PublicAPI]
        public static void ShouldBe(this DateTimeUtc actual, DateTimeUtc expected, TimeSpan tolerance, string? customMessage = null)
        {
            actual.ToDateTime().ShouldBe(expected.ToDateTime(), tolerance, customMessage);
        }

        [PublicAPI]
        public static void ShouldNotBe(this DateTimeUtc actual, DateTimeUtc expected, TimeSpan tolerance, string? customMessage = null)
        {
            actual.ToDateTime().ShouldNotBe(expected.ToDateTime(), tolerance, customMessage);
        }
    }
}
