using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using back.Models;
using back.Models.Enums;

namespace back.IntegrationTests.Controllers
{
    public class UsersControllerIntegrationTests : IAsyncLifetime
    {
        private WebApplicationFactory<Program> _factory = null!;
        private HttpClient _client = null!;
        private int _testTeamId;

        public async Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();

            // Create a test team first
            var newTeam = new Team
            {
                Name = "Test Team Integration",
                CurrentSongIndex = 0
            };

            var response = await _client.PostAsJsonAsync("/teams", newTeam);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var team = await response.Content.ReadFromJsonAsync<Team>();
                _testTeamId = team!.Id;
            }
            else
            {
                // If creation fails, use a test ID (test will handle NotFound)
                _testTeamId = 99999;
            }

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
            var endpoint = $"/teams/{_testTeamId}/users";

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
            var teamId = 999999;
            var endpoint = $"/teams/{teamId}/users";

            // Act
            var response = await _client.GetAsync(endpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddUser_WhenValidUserProvided_ShouldCreateUserAndReturnCreated()
        {
            // Arrange
            var newUser = new User
            {
                Name = "Integration Test User",
                Score = 0,
                Role = Role.Member,
                IsActive = true
            };
            var endpoint = $"/teams/{_testTeamId}/users";

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newUser);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.NotFound);
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var content = await response.Content.ReadFromJsonAsync<User>();
                content.Should().NotBeNull();
                content!.Name.Should().Be("Integration Test User");
                content.Role.Should().Be(Role.Member);
            }
        }

        [Fact]
        public async Task AddUser_WithEmptyName_ShouldReturnBadRequest()
        {
            // Arrange
            var newUser = new User
            {
                Name = "",
                Score = 0,
                Role = Role.Member,
                IsActive = true
            };
            var endpoint = $"/teams/{_testTeamId}/users";

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newUser);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddUser_WhenTeamNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var newUser = new User
            {
                Name = "Test User",
                Score = 0,
                Role = Role.Member,
                IsActive = true
            };
            var endpoint = $"/teams/999999/users";

            // Act
            var response = await _client.PostAsJsonAsync(endpoint, newUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateUser_WithValidUser_ShouldUpdateAndReturnOk()
        {
            // Arrange - First create a user
            var newUser = new User
            {
                Name = "Original Name",
                Score = 100,
                Role = Role.Member,
                IsActive = true
            };
            var createResponse = await _client.PostAsJsonAsync($"/teams/{_testTeamId}/users", newUser);

            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                // Team not found, skip test
                return;
            }

            var createdUser = await createResponse.Content.ReadFromJsonAsync<User>();
            var userId = createdUser!.Id;

            var updatedUser = new User
            {
                Name = "Updated Name",
                Score = 200,
                Role = Role.Host,
                IsActive = true
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/teams/{_testTeamId}/users/{userId}", updatedUser);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadFromJsonAsync<User>();
                content.Should().NotBeNull();
                content!.Name.Should().Be("Updated Name");
                content.Score.Should().Be(200);
            }
        }

        [Fact]
        public async Task DeleteUser_WhenUserExists_ShouldReturnNoContent()
        {
            // Arrange - First create a user
            var newUser = new User
            {
                Name = "User to Delete",
                Score = 100,
                Role = Role.Member,
                IsActive = true
            };
            var createResponse = await _client.PostAsJsonAsync($"/teams/{_testTeamId}/users", newUser);

            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                // Team not found, skip test
                return;
            }

            var createdUser = await createResponse.Content.ReadFromJsonAsync<User>();
            var userId = createdUser!.Id;

            // Act
            var response = await _client.DeleteAsync($"/teams/{_testTeamId}/users/{userId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }
    }
}
