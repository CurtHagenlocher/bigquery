/*
* Copyright (c) 2025 ADBC Drivers Contributors
*
* Licensed under the Apache License, Version 2.0 (the
* "License"); you may not use this file except in compliance
* with the License.  You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Text.Json.Serialization;

namespace AdbcDrivers.BigQuery
{
    [JsonSerializable(typeof(BigQueryTokenResponse))]
    [JsonSerializable(typeof(BigQueryStsTokenRequest))]
    [JsonSerializable(typeof(BigQueryStsTokenResponse))]
    internal partial class BigQueryJsonContext : JsonSerializerContext
    {
    }

    internal sealed class BigQueryStsTokenRequest
    {
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }

        [JsonPropertyName("subjectToken")]
        public string? SubjectToken { get; set; }

        [JsonPropertyName("audience")]
        public string? Audience { get; set; }

        [JsonPropertyName("grantType")]
        public string? GrantType { get; set; }

        [JsonPropertyName("subjectTokenType")]
        public string? SubjectTokenType { get; set; }

        [JsonPropertyName("requestedTokenType")]
        public string? RequestedTokenType { get; set; }
    }
}
