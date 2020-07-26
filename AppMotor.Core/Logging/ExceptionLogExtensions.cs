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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;

using AppMotor.Core.DataModel;
using AppMotor.Core.Globalization;
using AppMotor.Core.Utils;

using JetBrains.Annotations;

namespace AppMotor.Core.Logging
{
    /// <summary>
    /// Extension methods related to logging of exceptions.
    /// </summary>
    public static class ExceptionLogExtensions
    {
        [NotNull]
        private static readonly Dictionary<Type, LoggablePropertiesList> s_loggablePropertiesCache
            = new Dictionary<Type, LoggablePropertiesList>();

        /// <summary>
        /// Returns all (simple) loggable properties for the specified exception. The properties will be returned
        /// in alphabetical order. Only properties whose type matches <see cref="LoggableValues.IsSimpleLoggableType"/>
        /// will be returned.
        ///
        /// <para>Note: The list also includes all properties that have the type <c>object</c>. You'll need
        /// to filter them based on their actual value.</para>
        /// </summary>
        /// <seealso cref="GetLoggablePropertyValues"/>
        /// <seealso cref="GetLoggablePropertyValuesAsStrings"/>
        [PublicAPI, ItemNotNull]
        public static ImmutableArray<PropertyInfo> GetLoggableProperties([NotNull] this Exception exception)
        {
            Verify.Argument.IsNotNull(exception, nameof(exception));

            var exceptionType = exception.GetType();

            LoggablePropertiesList loggableProperties;

            lock (s_loggablePropertiesCache)
            {
                if (!s_loggablePropertiesCache.TryGetValue(exceptionType, out loggableProperties))
                {
                    loggableProperties = new LoggablePropertiesList(exceptionType);
                    s_loggablePropertiesCache[exceptionType] = loggableProperties;
                }
            }

            return loggableProperties.Value;
        }

        /// <summary>
        /// Returns all (simple) loggable values for the specified exception. The values will be returned
        /// sorted by property name.
        /// </summary>
        /// <param name="exception">This exception.</param>
        /// <param name="filter">The filter to use (optional).</param>
        /// <seealso cref="GetLoggableProperties"/>
        /// <seealso cref="GetLoggablePropertyValuesAsStrings"/>
        [PublicAPI, NotNull]
        public static IEnumerable<KeyValuePair<string, object>> GetLoggablePropertyValues(
                [NotNull] this Exception exception,
                [CanBeNull] ILoggableExceptionPropertyFilter filter = null
            )
        {
            var loggableProperties = exception.GetLoggableProperties();

            foreach (var loggableProperty in loggableProperties)
            {
                if (filter?.ExcludeProperty(loggableProperty) == true)
                {
                    continue;
                }

                object loggableValue;

                try
                {
                    loggableValue = loggableProperty.GetValue(exception);
                }
                catch (Exception ex)
                {
                    loggableValue = $"Error while retrieving value for property '{loggableProperty.Name}': {ex.Message}";
                }

                if (filter?.ExcludePropertyValue(loggableValue, loggableProperty) == true)
                {
                    continue;
                }

                if (loggableValue != null)
                {
                    if (loggableProperty.PropertyType == typeof(object))
                    {
                        // Check if the actual value is loggable.
                        if (!LoggableValues.IsSimpleLoggableType(loggableValue.GetType()))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        // Check if the actual value is sensitive.
                        if (loggableValue.IsSensitiveValue())
                        {
                            continue;
                        }
                    }
                }

                yield return new KeyValuePair<string, object>(loggableProperty.Name, loggableValue);
            }
        }

        /// <summary>
        /// Returns all (simple) loggable values for the specified exception as strings. The values
        /// will be returned sorted by property name. Value to text conversion is done via
        /// <see cref="LoggableValues.GetLoggableText"/>. See there for more details.
        /// </summary>
        /// <param name="exception">This exception.</param>
        /// <param name="valueFormatter">The formatter to use for converting the property values into
        /// strings.</param>
        /// <param name="filter">The filter to use (optional).</param>
        /// <seealso cref="GetLoggableProperties"/>
        /// <seealso cref="GetLoggablePropertyValues"/>
        [PublicAPI, NotNull]
        public static IEnumerable<KeyValuePair<string, string>> GetLoggablePropertyValuesAsStrings(
                [NotNull] this Exception exception,
                [CanBeNull] IValueFormatter valueFormatter = null,
                [CanBeNull] ILoggableExceptionPropertyFilter filter = null
            )
        {
            var loggableValues = exception.GetLoggablePropertyValues(filter);

            foreach (var (propertyName, loggableValue) in loggableValues)
            {
                string loggableText;

                if (propertyName == nameof(Exception.HResult) && loggableValue is int hResult)
                {
                    loggableText = HResultInfo.FormatHResult(hResult, includeName: true);
                }
                else
                {
                    try
                    {
                        loggableText = LoggableValues.GetLoggableText(loggableValue, valueFormatter);
                    }
                    catch (Exception ex)
                    {
                        loggableText = $"Error while convert value of property '{propertyName}' to text: {ex.Message}";
                    }
                }

                yield return new KeyValuePair<string, string>(propertyName, loggableText);
            }
        }

        private sealed class LoggablePropertiesList
        {
            [NotNull]
            private readonly PropertyInfo[] m_allPropertiesOrderedByName;

            [NotNull, ItemNotNull]
            private readonly HashSet<Type> m_allPropertyTypes = new HashSet<Type>();

            [ItemNotNull]
            public ImmutableArray<PropertyInfo> Value => this.m_valueLazy.Value;

            [NotNull]
            private Lazy<ImmutableArray<PropertyInfo>> m_valueLazy;

            public LoggablePropertiesList([NotNull] Type exceptionType)
            {
                // NOTE: This list will only contain properties that are "visible" for the
                //   exception type. This especially excludes properties that hide properties
                //   from the base class with the same name. But that's ok. We don't need
                //   to return two properties with the same name.
                this.m_allPropertiesOrderedByName = exceptionType.GetProperties(BindingFlags.Public|BindingFlags.Instance)
                                                                 .OrderBy(prop => prop.Name)
                                                                 .ToArray();

                this.m_valueLazy = CreateValueLazy();

                foreach (var propertyInfo in this.m_allPropertiesOrderedByName)
                {
                    this.m_allPropertyTypes.Add(propertyInfo.PropertyType);
                }

                LoggableValues.LoggabilityChanged += OnLoggabilityChanged;
            }

            private void OnLoggabilityChanged(object sender, [NotNull] LoggabilityChangedEventArgs e)
            {
                // If any of our types changed loggability, re-create the list of loggable properties.
                if (this.m_allPropertyTypes.Contains(e.Type))
                {
                    this.m_valueLazy = CreateValueLazy();
                }
            }

            [NotNull]
            private Lazy<ImmutableArray<PropertyInfo>> CreateValueLazy()
            {
                return new Lazy<ImmutableArray<PropertyInfo>>(
                    CollectProperties,
                    LazyThreadSafetyMode.ExecutionAndPublication
                );
            }

            [ItemNotNull]
            private ImmutableArray<PropertyInfo> CollectProperties()
            {
                var loggablePropertiesBuilder = ImmutableArray.CreateBuilder<PropertyInfo>();

                foreach (var propertyInfo in this.m_allPropertiesOrderedByName)
                {
                    if (!propertyInfo.CanRead)
                    {
                        continue;
                    }

                    // NOTE: Since "object" may be anything, we include it here. We'll then check
                    //   the actual value when having a concrete exception (see above).
                    if (   LoggableValues.IsSimpleLoggableType(propertyInfo.PropertyType)
                        || propertyInfo.PropertyType == typeof(object))
                    {
                        loggablePropertiesBuilder.Add(propertyInfo);
                    }
                }

                return loggablePropertiesBuilder.ToImmutable();
            }
        }
    }
}