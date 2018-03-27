using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AcronymDataManager.Models
{
    public class WordDefinition
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        public string Acronym { get; set; }

        public IEnumerable<string> Definitions { get; set; }
    }
}
