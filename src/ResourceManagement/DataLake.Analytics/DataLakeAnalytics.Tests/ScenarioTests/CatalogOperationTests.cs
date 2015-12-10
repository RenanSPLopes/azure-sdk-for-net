﻿//
// Copyright (c) Microsoft.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Linq;
using System.Net;
using Microsoft.Azure.Management.DataLake.Analytics;
using Microsoft.Azure.Management.DataLake.Analytics.Models;
using Microsoft.Azure.Test;
using Xunit;
using Microsoft.Rest.ClientRuntime.Azure.TestFramework;
using Microsoft.Rest.Azure;

namespace DataLakeAnalytics.Tests
{
    public class CatalogOperationTests : TestBase, IDisposable
    {
        private CommonTestFixture commonData;

        public CatalogOperationTests()
        {
            commonData = new CommonTestFixture(this.GetType().FullName);

        }

        public void Dispose()
        {
            if (commonData != null)
            {
                commonData.Dispose();
            }
        }

        [Fact]
        public void GetCatalogItemsTest()
        {
            // this test currently tests for Database, table TVF, view, types and procedure
            using (var context = MockContext.Start(this.GetType().FullName))
            {
                using (var clientToUse = commonData.GetDataLakeAnalyticsCatalogManagementClient(context))
                {
                    var dbListResponse = clientToUse.Catalog.ListDatabases(
                        commonData.DataLakeAnalyticsAccountName);

                    Assert.True(dbListResponse.Count() >= 1);

                    // look for the DB we created
                    Assert.True(dbListResponse.Any(db => db.DatabaseName.Equals(commonData.DatabaseName)));

                    // Get the specific Database as well
                    var dbGetResponse = clientToUse.Catalog.GetDatabase(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName);

                    Assert.Equal(commonData.DatabaseName, dbGetResponse.DatabaseName);

                    // Get the table list
                    var tableListResponse = clientToUse.Catalog.ListTables(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo");

                    Assert.True(tableListResponse.Count() >= 1);

                    // look for the table we created
                    Assert.True(tableListResponse.Any(table => table.TableName.Equals(commonData.TableName)));

                    // Get the specific table as well
                    var tableGetResponse = clientToUse.Catalog.GetTable(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo", commonData.TableName);

                    Assert.Equal(commonData.TableName, tableGetResponse.TableName);

                    // Get the TVF list
                    var tvfListResponse = clientToUse.Catalog.ListTableValuedFunctions(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo");

                    Assert.True(tvfListResponse.Count() >= 1);

                    // look for the tvf we created
                    Assert.True(tvfListResponse.Any(tvf => tvf.TvfName.Equals(commonData.TvfName)));

                    // Get the specific TVF as well
                    var tvfGetResponse = clientToUse.Catalog.GetTableValuedFunction(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo", commonData.TvfName);

                    Assert.Equal(commonData.TvfName, tvfGetResponse.TvfName);

                    // Get the View list
                    var viewListResponse = clientToUse.Catalog.ListViews(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo");

                    Assert.True(viewListResponse.Count() >= 1);

                    // look for the view we created
                    Assert.True(viewListResponse.Any(view => view.ViewName.Equals(commonData.ViewName)));

                    // Get the specific view as well
                    var viewGetResponse = clientToUse.Catalog.GetView(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo", commonData.ViewName);

                    Assert.Equal(commonData.ViewName, viewGetResponse.ViewName);

                    // Get the Procedure list
                    var procListResponse = clientToUse.Catalog.ListProcedures(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo");

                    Assert.True(procListResponse.Count() >= 1);

                    // look for the procedure we created
                    Assert.True(procListResponse.Any(proc => proc.ProcName.Equals(commonData.ProcName)));

                    // Get the specific procedure as well
                    var procGetResponse = clientToUse.Catalog.GetProcedure(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo", commonData.ProcName);

                    Assert.Equal(commonData.ProcName, procGetResponse.ProcName);

                    // Get all the types
                    var typeGetResponse = clientToUse.Catalog.ListTypes(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo", null);


                    Assert.NotNull(typeGetResponse);
                    Assert.NotEmpty(typeGetResponse);

                    // Get all the types that are not complex
                    typeGetResponse = clientToUse.Catalog.ListTypes(
                        commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, "dbo", new Microsoft.Rest.Azure.OData.ODataQuery<USqlType>{Filter = "isComplexType eq false"});


                    Assert.NotNull(typeGetResponse);
                    Assert.NotEmpty(typeGetResponse);
                    Assert.False(typeGetResponse.Any(type => type.IsComplexType.Value));
                }
            }
        }

        [Fact]
        public void SecretAndCredentialCRUDTest()
        {
            using (var context = MockContext.Start(this.GetType().FullName))
            {
                using (var clientToUse = commonData.GetDataLakeAnalyticsCatalogManagementClient(context))
                {
                    using (var jobClient = commonData.GetDataLakeAnalyticsJobManagementClient(context))
                    {
                        // create the secret
                        var secretCreateResponse = clientToUse.Catalog.CreateSecret(
                            commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, commonData.SecretName,
                            new DataLakeAnalyticsCatalogSecretCreateOrUpdateParameters
                            {
                                Password = commonData.SecretPwd,
                                Uri = "https://adlasecrettest.contoso.com:443"
                            });

                        /*
                         * TODO: Enable once confirmed that we throw 409s when a secret already exists
                        // Attempt to create the secret again, which should throw
                        Assert.Throws<CloudException>(
                            () => clientToUse.Catalog.CreateSecret(
                                commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName,
                                new DataLakeAnalyticsCatalogSecretCreateOrUpdateParameters
                                {
                                    Password = commonData.SecretPwd,
                                    SecretName = commonData.SecretName,
                                    Uri = "https://adlasecrettestnewuri.contoso.com:443"
                                }));
                        */

                        // Get the secret and ensure the response contains a date.
                        var secretGetResponse = clientToUse.Catalog.GetSecret(
                            commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, commonData.SecretName);

                        Assert.NotNull(secretGetResponse);
                        Assert.NotNull(secretGetResponse.CreationTime);

                        // Create a credential with the secret
                        var credentialCreationScript =
                            string.Format(
                                @"USE {0}; CREATE CREDENTIAL {1} WITH USER_NAME = ""scope@rkm4grspxa"", IDENTITY = ""{2}"";",
                                commonData.DatabaseName, commonData.CredentialName, commonData.SecretName);
                        commonData.DataLakeAnalyticsManagementHelper.RunJobToCompletion(jobClient, 
                            commonData.DataLakeAnalyticsAccountName, TestUtilities.GenerateGuid(),
                            credentialCreationScript);

                        // Get the Credential list
                        var credListResponse = clientToUse.Catalog.ListCredentials(
                            commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName);
                        Assert.True(credListResponse.Count() >= 1);

                        // look for the credential we created
                        Assert.True(credListResponse.Any(cred => cred.CredentialName.Equals(commonData.CredentialName)));

                        // Get the specific credential as well
                        var credGetResponse = clientToUse.Catalog.GetCredential(
                            commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, commonData.CredentialName);
                        Assert.Equal(commonData.CredentialName, credGetResponse.CredentialName);

                        // Drop the credential (to enable secret deletion)
                        var credentialDropScript =
                            string.Format(
                                @"USE {0}; DROP CREDENTIAL {1};", commonData.DatabaseName, commonData.CredentialName);
                        commonData.DataLakeAnalyticsManagementHelper.RunJobToCompletion(jobClient, 
                            commonData.DataLakeAnalyticsAccountName, TestUtilities.GenerateGuid(),
                            credentialDropScript);

                        // Delete the secret
                        clientToUse.Catalog.DeleteSecret(
                            commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, commonData.SecretName);

                        // Try to get the secret which should throw
                        Assert.Throws<CloudException>(() => clientToUse.Catalog.GetSecret(
                            commonData.DataLakeAnalyticsAccountName, commonData.DatabaseName, commonData.SecretName));
                    }

                }
            }
        }
    }
}
