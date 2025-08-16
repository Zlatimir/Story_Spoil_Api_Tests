using System.Text.Json.Serialization;

namespace StorySpoilApiTests.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }
    }
}
