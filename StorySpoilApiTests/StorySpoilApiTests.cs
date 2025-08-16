using RestSharp;
using RestSharp.Authenticators;
using StorySpoilApiTests.Models;
using System.Net;
using System.Text.Json;

namespace StorySpoilApiTests
{
    public class StorySpoilApiTests
    {
        private const string UserName = "Tester007";
        private const string Password = "Tester007";
        private string jwtToken = string.Empty;

        private RestClient client;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        private static string lastStoryId;        
        private const string StoryImageUrl = "";

        [OneTimeSetUp]
        public void Setup()
        {
            jwtToken = GetJwtToken(UserName, Password);

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        [Test, Order(1)]
        public void CreateStory_WithRequiredFields_ShouldReturnCreated()
        {
            var newIdea = new StoryDTO
            {
                Title = "Test Story Title",
                Description = "This is a test story description.",
                Url = StoryImageUrl
            };            

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newIdea);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Expected status code 201 Created");

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"), "Expected success message");

            lastStoryId = responseData.StoryId;            
        }

        [Test, Order(2)]
        public void EditLastStory_SholdReturnOk()
        {
            if (string.IsNullOrEmpty(lastStoryId))
            {
                Assert.Fail("No story ID available to edit.");
            }

            var updatedStory = new StoryDTO
            {
                Title = "Updated Test Story Title",
                Description = "This is an updated test story description.",
                Url = StoryImageUrl
            };

            var request = new RestRequest($"/api/Story/Edit/{lastStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData.Msg, Is.EqualTo("Successfully edited"), "Expected success message");
        }

        [Test, Order(3)]
        public void GetAllStories_ShouldReturnOk()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");

            var responseData = JsonSerializer.Deserialize<List<StoryDTO>>(response.Content);

            Assert.That(responseData, Is.Not.Null, "Expected non-null response data");

            Assert.That(responseData.Count, Is.GreaterThan(0), "Expected at least one story in the response");
        }
        [Test, Order(4)]
        public void DeleteLastStory_ShouldReturnOk()
        {
            if (string.IsNullOrEmpty(lastStoryId))
            {
                Assert.Fail("No story ID available to delete.");
            }

            var request = new RestRequest($"/api/Story/Delete/{lastStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK");

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData.Msg, Is.EqualTo("Deleted successfully!"), "Expected success message");
        }

        [Test, Order(5)]
        public void CreateStory_WithMissingFields_ShouldReturnBadRequest()
        {
            var incompleteStory = new StoryDTO
            {
                Title = "Incomplete Story Title"
                // Missing Description
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(incompleteStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request");
        }

        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            var nonExistingStoryId = "non-existing-id";
            var updatedStory = new StoryDTO
            {
                Title = "Updated Non-Existing Story",
                Description = "This story does not exist.",
                Url = StoryImageUrl
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Expected status code 404 Not Found");

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData.Msg, Is.EqualTo("No spoilers..."), "Expected warning message");
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnNotBadRequest()
        {
            var nonExistingStoryId = "non-existing-id";

            var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request");

            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(responseData.Msg, Is.EqualTo("Unable to delete this story spoiler!"), "Expected warning message");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client.Dispose();
        }

        private string GetJwtToken(string userName, string password)
        {
            var tempClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });
            var response = tempClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Authentication failed: {response.Content}");
            }

            var responseData = JsonSerializer.Deserialize<JsonElement>(response.Content);
            var token = responseData.GetProperty("accessToken").GetString();

            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Authentication failed: No token received.");
            }

            return token;
        }
    }
}