﻿using Jasper.Persistence.SqlServer.Schema;
using Xunit;

namespace IntegrationTests.Persistence.SqlServer
{
    public class SchemaLoaderTests : SqlServerContext
    {
        [Fact]
        public void retrieve_creation_script()
        {
            var sql = SchemaLoader.ToCreationScript("foo");

            sql.ShouldContain("create table foo.jasper_outgoing_envelopes");
            sql.ShouldContain("create table foo.jasper_incoming_envelopes");
        }

        [Fact]
        public void retrieve_drop_script()
        {
            var sql = SchemaLoader.ToDropScript("foo");

            sql.ShouldContain("drop table foo.jasper_outgoing_envelopes");
            sql.ShouldContain("drop table foo.jasper_incoming_envelopes");
        }

        [Fact]
        public void drop_then_create()
        {
            var loader = new SchemaLoader(Servers.SqlServerConnectionString);
            loader.DropAll();

            loader.CreateAll();
        }

        [Fact]
        public void drop_then_create_different_schema()
        {
            var loader = new SchemaLoader(Servers.SqlServerConnectionString, "receiver");
            loader.DropAll();

            loader.CreateAll();
        }

        [Fact]
        public void recreate_all_tables()
        {
            var loader = new SchemaLoader(Servers.SqlServerConnectionString);
            loader.RecreateAll();
        }

        [Fact]
        public void recreate_all_tables_in_a_different_schema()
        {
            var loader = new SchemaLoader(Servers.SqlServerConnectionString, "sender");
            loader.RecreateAll();
        }
    }
}
