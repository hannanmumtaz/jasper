﻿using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Conneg;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class message_forwarding
    {

        [Fact]
        public async Task send_message_via_forwarding()
        {
            var tracker = new MessageTracker();
            var channel = "tcp://localhost:2345/incoming".ToUri();


            var runtime = await JasperRuntime.ForAsync(_ =>
            {
                _.Handlers.DisableConventionalDiscovery(true);
                _.Handlers.IncludeType<NewMessageHandler>();

                _.Services.AddSingleton(tracker);


                _.Publish.Message<OriginalMessage>().To(channel);
                _.Transports.ListenForMessagesFrom(channel);
            });

            try
            {
                var waiter = tracker.WaitFor<NewMessage>();

                await runtime.Messaging.Send(new OriginalMessage {FirstName = "James", LastName = "Worthy"}, e =>
                {
                    e.Destination = channel;
                    e.ContentType = "application/json";
                });

                waiter.Wait(5.Seconds());

                waiter.Result.Message.As<NewMessage>().FullName.ShouldBe("James Worthy");
            }
            finally
            {
                await runtime.Shutdown();
            }

        }
    }

    [MessageIdentity("versioned-message", Version = 1)]
    public class OriginalMessage : IForwardsTo<NewMessage>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public NewMessage Transform()
        {
            return new NewMessage{FullName = $"{FirstName} {LastName}"};
        }
    }

    [MessageIdentity("versioned-message", Version = 2)]
    public class NewMessage
    {
        public string FullName { get; set; }
    }

    public class NewMessageHandler
    {
        public void Handle(NewMessage message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Record(message, envelope);
        }
    }
}
