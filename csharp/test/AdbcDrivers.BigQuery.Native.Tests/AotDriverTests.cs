/*
 * Copyright (c) 2026 ADBC Drivers Contributors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AdbcDrivers.BigQuery.MockServer;
using Apache.Arrow;
using Apache.Arrow.Adbc;
using Apache.Arrow.Adbc.C;
using Apache.Arrow.Ipc;
using Apache.Arrow.Types;
using Xunit;

namespace AdbcDrivers.BigQuery.Native.Tests
{
    [Trait("Category", "NativeAot")]
    public class AotDriverTests
    {
        private const string DriverPathEnvironmentVariable = "ADBC_BIGQUERY_NATIVE_PATH";

        private static string RequireDriverPath()
        {
            string? path = Environment.GetEnvironmentVariable(DriverPathEnvironmentVariable);
            Skip.IfNot(
                !string.IsNullOrEmpty(path) && File.Exists(path),
                $"Set {DriverPathEnvironmentVariable} to the published BigQuery NativeAOT driver.");
            return path!;
        }

        private static Dictionary<string, string> CreateParameters(BigQueryMockServer server) =>
            new Dictionary<string, string>
            {
                { "adbc.bigquery.project_id", "mock-project" },
                { "adbc.bigquery.auth_type", "mock" },
                { "adbc.bigquery.test.rest_endpoint", server.RestEndpoint },
                { "adbc.bigquery.test.storage_endpoint", server.GrpcEndpoint },
            };

        [SkippableFact]
        public void DriverLoadsFromNativeLibrary()
        {
            using AdbcDriver driver = CAdbcDriverImporter.Load(RequireDriverPath());
            Assert.Equal(AdbcVersion.Version_1_1_0, driver.DriverVersion);
        }

        [SkippableFact]
        public async Task SelectRoundTripsThroughNativeDriver()
        {
            using var server = new BigQueryMockServer();
            var schema = new Schema(new[]
            {
                new Field("value", Int64Type.Default, nullable: true),
            }, null);
            using var batch = new RecordBatch(
                schema,
                new IArrowArray[] { new Int64Array.Builder().Append(42).Build() },
                1);

            server.ReadService.DefaultArrowSchema = ArrowSerializationHelpers.SerializeSchema(schema);
            server.ReadService.DefaultArrowBatch = ArrowSerializationHelpers.SerializeRecordBatch(batch);
            server.ReadService.DefaultRowCount = 1;

            using AdbcDriver driver = CAdbcDriverImporter.Load(RequireDriverPath());
            using AdbcDatabase database = driver.Open(CreateParameters(server));
            using AdbcConnection connection = database.Connect(new Dictionary<string, string>());
            using AdbcStatement statement = connection.CreateStatement();
            statement.SqlQuery = "SELECT 42 AS value";

            QueryResult result = statement.ExecuteQuery();
            using IArrowArrayStream stream = result.Stream!;
            using RecordBatch? resultBatch = await stream.ReadNextRecordBatchAsync();

            Assert.NotNull(resultBatch);
            var values = Assert.IsType<Int64Array>(resultBatch!.Column(0));
            Assert.Equal(42L, values.GetValue(0));
            Assert.Null(await stream.ReadNextRecordBatchAsync());
        }
    }
}
