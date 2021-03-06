﻿using Jasper;
using Jasper.Configuration;
using Jasper.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Samples
{
    public class Bootstrapping
    {
        public static void Go()
        {
            // SAMPLE: Bootstrapping-Basic
            using (var runtime = JasperRuntime.Basic())
            {
                // do stuff
            }
            // ENDSAMPLE

            // SAMPLE: Bootstrapping-Basic2
            using (var runtime = JasperRuntime.For(new JasperRegistry()))
            {
                // do stuff
            }
            // ENDSAMPLE

            // SAMPLE: Bootstrapping-Basic3
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Transports.ListenForMessagesFrom("tcp://localhost:2001");
            }))
            {
                // do stuff
            }

            // ENDSAMPLE
        }
    }

    // SAMPLE: Bootstrapping-CustomJasperExtension
    public class CustomJasperExtension : IJasperExtension
    {
        public int Threshold { get; set; } = 10;

        public void Configure(JasperRegistry registry)
        {
            // apply alterations
        }
    }
    // ENDSAMPLE

    // SAMPLE: AppWithExtensions
    public class AppWithExtensions : JasperRegistry
    {
        public AppWithExtensions()
        {
            // as is
            Include<CustomJasperExtension>();

            // or
            Include(new CustomJasperExtension());

            // or use the extension with customizations
            Include<CustomJasperExtension>(_ => { _.Threshold = 20; });
        }
    }
    // ENDSAMPLE


    public interface ISecurityService
    {
    }

    public class MySecurityService : ISecurityService
    {
    }

    // SAMPLE: Bootstrapping-ServiceRegistrations
    public class MyJasperApp : JasperRegistry
    {
        public MyJasperApp()
        {
            // Adding services to the underlying container via
            // the ASP.Net Core DI abstractions
            Services.AddTransient<ISecurityService, MySecurityService>();

            // or via idiomatic StructureMap
            Services.For<ISecurityService>().Use<MySecurityService>();
        }
    }

    // ENDSAMPLE
}

namespace Bootstrapping.Configuration
{
    public class MyJasperApp : JasperRegistry
    {
        public MyJasperApp()
        {
            // Set up the application configuration
            Configuration
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json");
        }
    }
}

namespace Bootstrapping.Configuration2
{
    // SAMPLE: CustomJasperRegistry
    public class MyJasperApp : JasperRegistry
    {
        public MyJasperApp()
        {
            ServiceName = "My Jasper App";

            Configuration.AddEnvironmentVariables();

            Transports.ListenForMessagesFrom("durable://localhost:2111/incoming");
        }
    }
    // ENDSAMPLE

    public static class Program
    {
        public static void Go()
        {
            // SAMPLE: Bootstrapping-with-custom-JasperRegistry
            using (var runtime = JasperRuntime.For<MyJasperApp>())
            {
                // do stuff
            }

            // or

            using (var runtime = JasperRuntime.For(new MyJasperApp()))
            {
                // do stuff
            }

            // ENDSAMPLE
        }
    }

    // SAMPLE: CustomServiceRegistry
    public class CustomServiceRegistry : JasperRegistry
    {
        public CustomServiceRegistry()
        {
            // The derived default would be "CustomService"
            ServiceName = "My Custom Service";
        }
    }
    // ENDSAMPLE

    // SAMPLE: EnvironmentNameRegistry
    public class EnvironmentNameRegistry : JasperRegistry
    {
        public EnvironmentNameRegistry()
        {
            // which is just a shortcut for:
            Hosting.UseEnvironment("Production");
        }
    }

    // ENDSAMPLE
}
