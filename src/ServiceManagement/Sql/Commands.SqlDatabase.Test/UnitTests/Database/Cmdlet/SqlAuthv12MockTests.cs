﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.SqlDatabase.Services.Server;
using Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.TSql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.Commands.SqlDatabase.Test.UnitTests.Database.Cmdlet
{
    [RecordMockDataResults("./")]
    [TestClass]
    public class SqlAuthv12MockTests
    {
        public static string username = "testlogin";
        public static string password = "MyS3curePa$$w0rd";
        public static string manageUrl = "https://mysvr2.adamkr-vm04.onebox.xdb.mscds.com";

        [TestInitialize]
        public void Setup()
        {
            var mockConn = new MockSqlConnection();
            TSqlConnectionContext.MockSqlConnection = mockConn;
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Do any test clean up here.
        }

        [TestMethod]
        public void NewAzureSqlDatabaseWithSqlAuthv12()
        {

            using (System.Management.Automation.PowerShell powershell =
                System.Management.Automation.PowerShell.Create())
            {

                // Create a context
                NewAzureSqlDatabaseServerContextTests.CreateServerContextSqlAuthV2(
                    powershell,
                    manageUrl,
                    username,
                    password,
                    "$context");

                Collection<PSObject> database1, database2, database3, database4;

                database1 = powershell.InvokeBatchScript(
                    @"$testdb1 = New-AzureSqlDatabase " +
                    @"-Context $context " +
                    @"-DatabaseName testdb1 " +
                    @"-Force",
                    @"$testdb1");
                database2 = powershell.InvokeBatchScript(
                    @"$testdb2 = New-AzureSqlDatabase " +
                    @"-Context $context " +
                    @"-DatabaseName testdb2 " +
                    @"-Collation Japanese_CI_AS " +
                    @"-Edition Basic " +
                    @"-MaxSizeGB 2 " +
                    @"-Force",
                    @"$testdb2");
                database3 = powershell.InvokeBatchScript(
                    @"$testdb3 = New-AzureSqlDatabase " +
                    @"-Context $context " +
                    @"-DatabaseName testdb3 " +
                    @"-MaxSizeBytes 107374182400 " +
                    @"-Force",
                    @"$testdb3");
                var slo = powershell.InvokeBatchScript(
                    @"$so = Get-AzureSqlDatabaseServiceObjective " +
                    @"-Context $context " +
                    @"-ServiceObjectiveName S2 ",
                    @"$so");
                database4 = powershell.InvokeBatchScript(
                    @"$testdb4 = New-AzureSqlDatabase " +
                    @"-Context $context " +
                    @"-DatabaseName testdb4 " +
                    @"-Edition Standard " +
                    @"-ServiceObjective $so " +
                    @"-Force",
                    @"$testdb4");

                Assert.AreEqual(0, powershell.Streams.Error.Count, "Errors during run!");
                Assert.AreEqual(0, powershell.Streams.Warning.Count, "Warnings during run!");
                powershell.Streams.ClearStreams();

                Services.Server.Database database = database1.Single().BaseObject as Services.Server.Database;
                Assert.IsTrue(database != null, "Expecting a Database object");
                ValidateDatabaseProperties(database, "testdb1", "Standard", 250, 268435456000L, "SQL_Latin1_General_CP1_CI_AS", false, DatabaseTestHelper.StandardS0SloGuid);

                database = database2.Single().BaseObject as Services.Server.Database;
                Assert.IsTrue(database != null, "Expecting a Database object");
                ValidateDatabaseProperties(database, "testdb2", "Basic", 2, 2147483648L, "Japanese_CI_AS", false, DatabaseTestHelper.BasicSloGuid);

                database = database3.Single().BaseObject as Services.Server.Database;
                Assert.IsTrue(database != null, "Expecting a Database object");
                ValidateDatabaseProperties(database, "testdb3", "Standard", 100, 107374182400L, "SQL_Latin1_General_CP1_CI_AS", false, DatabaseTestHelper.StandardS0SloGuid);

                database = database4.Single().BaseObject as Services.Server.Database;
                Assert.IsTrue(database != null, "Expecting a Database object");
                ValidateDatabaseProperties(database, "testdb4", "Standard", 250, 268435456000L, "SQL_Latin1_General_CP1_CI_AS", false, DatabaseTestHelper.StandardS2SloGuid);
            }
        }


        /// <summary>
        /// Validate the properties of a database against the expected values supplied as input.
        /// </summary>
        /// <param name="database">The database object to validate</param>
        /// <param name="name">The expected name of the database</param>
        /// <param name="edition">The expected edition of the database</param>
        /// <param name="maxSizeGb">The expected max size of the database in GB</param>
        /// <param name="collation">The expected Collation of the database</param>
        /// <param name="isSystem">Whether or not the database is expected to be a system object.</param>
        internal static void ValidateDatabaseProperties(
            Services.Server.Database database,
            string name,
            string edition,
            int maxSizeGb,
            long maxSizeBytes,
            string collation,
            bool isSystem,
            Guid slo)
        {
            Assert.AreEqual(name, database.Name);
            Assert.AreEqual(edition, database.Edition);
            Assert.AreEqual(maxSizeGb, database.MaxSizeGB);
            Assert.AreEqual(maxSizeBytes, database.MaxSizeBytes);
            Assert.AreEqual(collation, database.CollationName);
            Assert.AreEqual(isSystem, database.IsSystemObject);
            // Assert.AreEqual(slo, database.ServiceObjectiveId);
        }
    }
}
