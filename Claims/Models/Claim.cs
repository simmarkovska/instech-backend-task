using Claims.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Claims.Models
{
    public class Claim
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "coverId")]
        public string CoverId { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "claimType")]
        public ClaimType Type { get; set; }

        [JsonProperty(PropertyName = "damageCost")]
        [Range(typeof(decimal), "0", "100000", ErrorMessage = "DamageCost cannot exceed 100.000")]
        public decimal DamageCost { get; set; }

    }
}
