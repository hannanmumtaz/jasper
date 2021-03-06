using System;
using System.Threading.Tasks;
using Alba;
using Jasper.TestSupport.Alba;
using Xunit;

namespace Jasper.Testing.Http
{
    public class AlbaExample
    {
        // SAMPLE: AlbaScenarioUsage
        [Fact]
        public async Task sample_alba_spec()
        {
            using (var runtime = JasperRuntime.Basic())
            {
                await runtime.Scenario(x =>
                {
                    x.Get.Url("/salutations");
                    x.StatusCodeShouldBeOk();
                    x.ContentShouldBe("Greetings and salutations");
                    x.ContentTypeShouldBe("text/plain");
                });
            }
        }
        // ENDSAMPLE
    }

    // SAMPLE: GreetingsEndpoint
    public class GreetingsEndpoint
    {
        public string get_salutations()
        {
            return "Greetings and salutations";
        }
    }
    // ENDSAMPLE

    // SAMPLE: AlbaScenarioUsageShared
    public class ApplicationFixture : IDisposable
    {
        public ApplicationFixture()
        {
            // Your application setup here would probably
            // refer to a JasperRegistry for your application
            Runtime = JasperRuntime.Basic();
        }

        public JasperRuntime Runtime { get; private set; }

        public void Dispose()
        {
            Runtime?.Dispose();
        }
    }

    public class AlbaExampleWithSharedContext : IClassFixture<ApplicationFixture>
    {
        private JasperRuntime _runtime;

        public AlbaExampleWithSharedContext(ApplicationFixture fixture)
        {
            _runtime = fixture.Runtime;
        }


        [Fact]
        public Task sample_alba_spec()
        {
            return _runtime.Scenario(x =>
            {
                x.Get.Url("/salutations");
                x.StatusCodeShouldBeOk();
                x.ContentShouldBe("Greetings and salutations");
                x.ContentTypeShouldBe("text/plain");
            });
        }

    }
    // ENDSAMPLE
}
