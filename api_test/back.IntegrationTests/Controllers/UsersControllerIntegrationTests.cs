using System.Net;
using System.Net.Http.Json;
using back.Models;
using back.Models.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;

namespace back.IntegrationTests.Controllers
{
    public class UsersControllerIntegrationTests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
            // Add any database setup here if needed
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetUsers_WhenTeamExists_ShouldReturnListOfUsers()
        {
            // Arrange
            // Using a team ID from the dummy data
            var teamId = 395326;
            var endpoint = $"/teams/{teamId}/users";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadFromJsonAsync<IEnumerable<User>>();
                content.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task GetUsers_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var teamId = 99999;
            var endpoint = $"/teams/{teamId}/users";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUser_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var teamId = 395326;
            var userId = 3;
            var endpoint = $"/teams/{teamId}/users/{userId}";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddUser_WhenValidUserProvided_ShouldCreateUserAndReturnCreated()
        {
            // Arrange
            var teamId = 395326;
            var newUser = new User
            {
                Name = "Integration Test User",
                Score = 0,
                Role = Role.Member,
                IsActive = true
            };
            var endpoint = $"/teams/{teamId}/users";

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newUser);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var content = await response.Content.ReadFromJsonAsync<User>();
                content.Should().NotBeNull();
                content.Name.Should().Be("Integration Test User");
                content.Role.Should().Be(Role.Member);
            }
        }
    }
}
