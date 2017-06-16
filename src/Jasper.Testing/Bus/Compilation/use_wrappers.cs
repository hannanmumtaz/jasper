﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Model;
using Jasper.Codegen;
using Jasper.Codegen.Compilation;
using Jasper.Configuration;
using Jasper.Testing.Bus.Runtime;
using Jasper.Testing.Http;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Compilation
{
    public class use_wrappers : CompilationContext<TransactionalHandler>
    {
        private readonly Tracking theTracking = new Tracking();

        public use_wrappers()
        {
            services.For<Tracking>().Use(theTracking);
            services.ForSingletonOf<IFakeStore>().Use<FakeStore>();

            
        }


        [Fact]
        public async Task wrapper_executes()
        {
            var message = new Message1();

            await Execute(message);

            ShouldBeBooleanExtensions.ShouldBeTrue(theTracking.DisposedTheSession);
            ShouldBeBooleanExtensions.ShouldBeTrue(theTracking.OpenedSession);
            ShouldBeBooleanExtensions.ShouldBeTrue(theTracking.CalledSaveChanges);
        }

        [Fact]
        public async Task wrapper_applied_by_generic_attribute_executes()
        {
            var message = new Message2();

            await Execute(message);

            ShouldBeBooleanExtensions.ShouldBeTrue(theTracking.DisposedTheSession);
            ShouldBeBooleanExtensions.ShouldBeTrue(theTracking.OpenedSession);
            ShouldBeBooleanExtensions.ShouldBeTrue(theTracking.CalledSaveChanges);
        }
    }

    public class TransactionalHandler
    {
        [FakeTransaction]
        public void Handle(Message1 message)
        {
            
        }

        [GenericFakeTransaction]
        public void Handle(Message2 message)
        {
            
        }
    }

    public class Tracking
    {
        public bool DisposedTheSession;
        public bool OpenedSession;
        public bool CalledSaveChanges;
    }

    public class GenericFakeTransactionAttribute : ModifyChainAttribute
    {
        public override void Modify(IChain chain)
        {
            chain.Middleware.Add(new FakeTransaction());
        }
    }

    public class FakeTransactionAttribute : ModifyHandlerChainAttribute
    {
        public override void Modify(HandlerChain chain)
        {
            chain.Middleware.Add(new FakeTransaction());
        }
    }

    public class FakeTransaction : Frame
    {
        private Variable _store;
        private readonly Variable _session;

        public FakeTransaction() : base(false)
        {
            _session = new Variable(typeof(IFakeSession), "session", this);
        }

        protected override IEnumerable<Variable> resolveVariables(GeneratedMethod chain)
        {
            _store = chain.FindVariable(typeof(IFakeStore));
            yield return _store;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.Write($"BLOCK:using (var {_session.Usage} = {_store.Usage}.OpenSession())");
            Next?.GenerateCode(method, writer);
            writer.Write($"{_session.Usage}.{nameof(IFakeSession.SaveChanges)}();");
            writer.FinishBlock();
        }
    }


}