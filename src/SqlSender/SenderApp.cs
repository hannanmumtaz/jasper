﻿using Baseline.Dates;
using Jasper;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Persistence.SqlServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace SqlSender
{
    public class SenderApp : JasperRegistry
    {
        public SenderApp()
        {
            Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();

            Hosting.UseUrls("http://*:5060").UseKestrel();

            Hosting.ConfigureLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Error);
                x.AddConsole();
            });

            Settings.Alter<MessagingSettings>(x => x.Retries.NodeReassignmentPollingTime = 5.Seconds());

            Settings.PersistMessagesWithSqlServer((context, settings) =>
            {
                settings.ConnectionString = context.Configuration["mssql"];
                settings.SchemaName = "sender";
            });


            Settings.Configure(c =>
            {
                Transports.ListenForMessagesFrom(c.Configuration["listener"]);
                Publish.AllMessagesTo(c.Configuration["receiver"]);
            });

        }
    }
}
