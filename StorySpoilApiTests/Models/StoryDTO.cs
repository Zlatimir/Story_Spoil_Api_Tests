using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorySpoilApiTests.Models
{
    internal class StoryDTO
    {
        [System.Text.Json.Serialization.JsonPropertyName("title")]
        public string Title { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string Description { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
