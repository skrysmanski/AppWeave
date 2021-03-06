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

using AppMotor.Core.Utils;

using Shouldly;

using Xunit;

namespace AppMotor.Core.Tests.Utils
{
    public sealed class EnumUtilsTests
    {
        [Fact]
        public void TestGetValues()
        {
            // test
            MyTestEnum[] enumValues = EnumUtils.GetValues<MyTestEnum>();

            // verify
            enumValues.ShouldContain(MyTestEnum.Value1);
            enumValues.ShouldContain(MyTestEnum.Value2);
            enumValues.ShouldContain(MyTestEnum.Value3);
            enumValues.Length.ShouldBe(3);
        }

        private enum MyTestEnum
        {
            Value1,
            Value2,
            Value3,
        }
    }
}
