using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using OpenRCT2.API;
using Xunit;

namespace ApiIntegrationTests
{
    public class TestLocalisation
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public TestLocalisation()
        {
            var webHostBuilder = new WebHostBuilder()
                .UseEnvironment(Environments.Testing)
                .UseStartup<Startup>();

            _server = new TestServer(webHostBuilder);
            _client = _server.CreateClient();
        }

        [Theory]
        [InlineData("unk")]
        [InlineData("en-GB")]
        [InlineData("nl-NL")]
        public async Task TestBadgeAsync(string langId)
        {
            var response = await _client.GetAsync($"/localisation/status/badges/{langId}");
            response.EnsureSuccessStatusCode();

            Assert.Equal("image/svg+xml", response.Content.Headers.ContentType.MediaType);
            Assert.StartsWith("<svg ", await response.Content.ReadAsStringAsync());
        }
    }
}
