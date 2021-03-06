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

using AppMotor.Core.DataModel;

namespace AppMotor.Core.Logging
{
    /// <summary>
    /// Represents a simple loggable value in the sense that it's a (more or less) value
    /// that ideally fits onto one line in a log file. You may implement this interface
    /// for a type whose values should be included in <see cref="ExceptionLogExtensions.GetLoggableProperties"/>
    /// and therefor in <see cref="ExtendedExceptionStringExtensions.ToStringExtended"/>.
    /// </summary>
    /// <seealso cref="SimpleLoggableValueMarker"/>
    public interface ISimpleLoggableValue
    {
    }

    /// <summary>
    /// A <see cref="TypeMarker"/> alternative to <see cref="ISimpleLoggableValue"/>.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class SimpleLoggableValueMarker : TypeMarker
    {
    }
}
