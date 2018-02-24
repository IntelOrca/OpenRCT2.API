using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ApiIntegrationTests.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;
using OpenRCT2.API;
using OpenRCT2.API.Abstractions;
using OpenRCT2.API.Controllers;
using OpenRCT2.API.Implementations;
using OpenRCT2.DB.Abstractions;
using Xunit;

namespace ApiIntegrationTests
{
    public class TestAuthentication
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public TestAuthentication()
        {
            var userApi = new Mock<OpenRCT2.API.OpenRCT2org.IUserApi>();
            userApi.Setup(x => x.AuthenticateUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                   .ReturnsAsync(new OpenRCT2.API.OpenRCT2org.JUser());

            IWebHostBuilder webHostBuilder = new WebHostBuilder()
                .UseEnvironment(Environments.Testing)
                .ConfigureServices(services =>
                {
                    services.AddSingleton(Mock.Of<IDBService>());
                    services.AddSingleton(Mock.Of<IUserRepository>());
                    services.AddSingleton(userApi.Object);
                })
                .UseStartup<Startup>();

            _server = new TestServer(webHostBuilder);
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task CheckUnauthenticatedAsync()
        {
            var response = await _client.GetAsync("/users");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CheckAuthenticatedAsync()
        {
            string token;
            {
                var body = new UserController.JLoginRequest()
                {
                    user = "mockuser",
                    password = "mockpassword"
                };
                var response = await _server.CreateRequest("/user/login")
                                            .UseJsonBody(body)
                                            .PostAsJsonAsync<UserController.JLoginResponse>();
                Assert.Equal(response.status, JStatus.OK);
                token = response.token;
            }
            {
                var response = await _server.CreateRequest("/users")
                                            .AddHeader(HeaderNames.Authorization, $"Bearer {token}")
                                            .GetAsJsonAsync<JResponse>();
                Assert.Equal(response.status, JStatus.OK);
            }
        }
    }
}
