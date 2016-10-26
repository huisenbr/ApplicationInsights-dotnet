﻿namespace Microsoft.ApplicationInsights.WindowsServer
{
    using System;
    using System.Globalization;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;

    using Microsoft.ApplicationInsights.WindowsServer.Azure;
    using Microsoft.ApplicationInsights.WindowsServer.Azure.Emulation;
    using Microsoft.ApplicationInsights.WindowsServer.Implementation;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Assert = Xunit.Assert;
    
    [TestClass]
    public class AzureWebAppRoleEnvironmentTelemetryInitializerTest
    {
        [TestMethod]
        public void AzureWebAppRoleEnvironmentTelemetryInitializerSetsRoleName()
        {
            var telemetryItem = new EventTelemetry();

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "TestRoleName");
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", "TestRoleInstanceName");

            var initializer = new AzureWebAppRoleEnvironmentTelemetryInitializer();
            initializer.Initialize(telemetryItem);

            Assert.Equal("TestRoleName", telemetryItem.Context.Cloud.RoleName);
            Assert.Equal("TestRoleInstanceName", telemetryItem.Context.Cloud.RoleInstance);
            Assert.Equal("TestRoleInstanceName", telemetryItem.Context.GetInternalContext().NodeName);

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", null);
        }

        [TestMethod]
        public void AzureWebAppRoleEnvironmentTelemetryInitializerDoesNotOverrideRoleName()
        {
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "TestRoleName");
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", "TestRoleInstanceName");

            var telemetryItem = new EventTelemetry();
            telemetryItem.Context.Cloud.RoleName = "Test";

            var initializer = new AzureWebAppRoleEnvironmentTelemetryInitializer();
            initializer.Initialize(telemetryItem);

            Assert.Equal("Test", telemetryItem.Context.Cloud.RoleName);
            Assert.Equal("TestRoleInstanceName", telemetryItem.Context.Cloud.RoleInstance);
            Assert.Equal("TestRoleInstanceName", telemetryItem.Context.GetInternalContext().NodeName);

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", null);
        }

        [TestMethod]
        public void AzureWebAppRoleEnvironmentTelemetryInitializerDoesNotOverrideRoleInstance()
        {
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "TestRoleName");
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", "TestRoleInstanceName");

            var telemetryItem = new EventTelemetry();
            telemetryItem.Context.Cloud.RoleInstance = "Test";

            var initializer = new AzureWebAppRoleEnvironmentTelemetryInitializer();
            initializer.Initialize(telemetryItem);

            Assert.Equal("TestRoleName", telemetryItem.Context.Cloud.RoleName);
            Assert.Equal("Test", telemetryItem.Context.Cloud.RoleInstance);
            Assert.Equal("TestRoleInstanceName", telemetryItem.Context.GetInternalContext().NodeName);

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", null);
        }

        [TestMethod]
        public void AzureWebAppRoleEnvironmentTelemetryInitializerDoesNotOverrideNodeName()
        {
            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", "TestRoleName");
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", "TestRoleInstanceName");

            var telemetryItem = new EventTelemetry();
            telemetryItem.Context.GetInternalContext().NodeName = "Test";

            var initializer = new AzureWebAppRoleEnvironmentTelemetryInitializer();
            initializer.Initialize(telemetryItem);

            Assert.Equal("TestRoleName", telemetryItem.Context.Cloud.RoleName);
            Assert.Equal("TestRoleInstanceName", telemetryItem.Context.Cloud.RoleInstance);
            Assert.Equal("Test", telemetryItem.Context.GetInternalContext().NodeName);

            Environment.SetEnvironmentVariable("WEBSITE_SITE_NAME", null);
            Environment.SetEnvironmentVariable("WEBSITE_HOSTNAME", null);
        }
    }
}
