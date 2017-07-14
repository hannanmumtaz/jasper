﻿using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Util;

namespace Jasper.Bus
{
    public class ServiceBus : IServiceBus
    {
        private readonly IEnvelopeSender _sender;
        private readonly IReplyWatcher _watcher;
        private readonly IHandlerPipeline _pipeline;
        private readonly SerializationGraph _serialization;

        public ServiceBus(IEnvelopeSender sender, IReplyWatcher watcher, IHandlerPipeline pipeline, SerializationGraph serialization)
        {
            _sender = sender;
            _watcher = watcher;
            _pipeline = pipeline;
            _serialization = serialization;
        }

        public Task<TResponse> Request<TResponse>(object request, RequestOptions options = null)
        {
            options = options ?? new RequestOptions();

            var envelope = EnvelopeForRequestResponse<TResponse>(request);

            if (options.Destination != null)
            {
                envelope.Destination = options.Destination;
            }


            var task = _watcher.StartWatch<TResponse>(envelope.CorrelationId, options.Timeout);

            _sender.Send(envelope);

            return task;
        }

        public Envelope EnvelopeForRequestResponse<TResponse>(object request)
        {
            var messageType = typeof(TResponse).ToTypeAlias();
            var reader = _serialization.ReaderFor(messageType);

            var envelope = new Envelope
            {
                Message = request,
                ReplyRequested = messageType,
                AcceptedContentTypes = reader.ContentTypes

            };
            return envelope;
        }

        public Task Send<T>(T message)
        {
            return _sender.Send(new Envelope {Message = message});
        }

        public Task Send<T>(T message, Action<Envelope> customize)
        {
            var envelope = new Envelope {Message = message};
            customize(envelope);

            return _sender.Send(envelope);
        }

        public Task Send<T>(Uri destination, T message)
        {
            return _sender.Send(new Envelope { Message = message, Destination = destination});
        }

        public Task Consume<T>(T message)
        {
            return _pipeline.InvokeNow(message);
        }

        public void DelaySend<T>(T message, DateTime time)
        {
            _sender.Send(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime()
            });
        }

        public void DelaySend<T>(T message, TimeSpan delay)
        {
            DelaySend(message, DateTime.UtcNow.Add(delay));
        }

        public Task SendAndWait<T>(T message)
        {
            return GetSendAndWaitTask(message);
        }

        public Task SendAndWait<T>(Uri destination, T message)
        {
            return GetSendAndWaitTask(message, destination);
        }

        private Task GetSendAndWaitTask<T>(T message, Uri destination = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                AckRequested = true,
                Destination = destination
            };

            var task = _watcher.StartWatch<Acknowledgement>(envelope.CorrelationId, 10.Minutes());


            _sender.Send(envelope);

            return task;
        }

    }
}
