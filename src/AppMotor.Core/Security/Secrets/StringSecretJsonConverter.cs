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
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppMotor.Core.Security.Secrets
{
    internal sealed class StringSecretJsonConverter : JsonConverter<DecryptedStringSecret>
    {
        private readonly List<DecryptedStringSecret> _createdSecrets = new();

        public IReadOnlyList<DecryptedStringSecret> CreatedSecrets => this._createdSecrets;

        /// <inheritdoc />
        public override DecryptedStringSecret? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.HasValueSequence)
            {
                throw new NotSupportedException("ValueSequence is not supported.");
            }

            ReadOnlySpan<byte> span = reader.ValueSpan;
            using var decryptedSecret = DecryptedSecret.FromInMemorySource(span);

            var stringSecret = decryptedSecret.ToStringSecret(DecryptedStringSecret.SupportedEncodings.Utf8);
            this._createdSecrets.Add(stringSecret);

            return stringSecret;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, DecryptedStringSecret value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }
}
