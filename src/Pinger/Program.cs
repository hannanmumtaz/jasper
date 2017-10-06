﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Jasper.Bus;
using Jasper.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using TestMessages;

namespace Pinger
{
    class Program
    {
        static int Main(string[] args)
        {
            return JasperAgent.Run(args, _ =>
            {
                _.Logging.UseConsoleLogging = true;

                _.Transports.Lightweight.ListenOnPort(2600);

                // Using static routing rules to start
                _.Publish.Message<PingMessage>().To("tcp://localhost:2601");

                _.Services.AddSingleton<IHostedService, PingSender>();
            });
        }
    }

    public class PongHandler
    {
        public void Handle(PongMessage message)
        {
            ConsoleWriter.Write(ConsoleColor.Cyan, "Got a pong back with name: " + message.Name);

        }
    }

    public class PingSender : BackgroundService
    {
        private readonly IServiceBus _bus;

        public PingSender(IServiceBus bus)
        {
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                    ConsoleWriter.Write(ConsoleColor.Magenta, "Look at me!!!!!");
                }
            }, stoppingToken);
        }
    }
}