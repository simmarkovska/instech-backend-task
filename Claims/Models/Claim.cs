using Claims.Enums;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Claims.Models
{
    ///<summary>
    ///Claim Model
    ///</summary>
    public class Claim
    {
        ///<summary>
        ///Claim Id property
        ///</summary>
        [JsonProperty(PropertyName = "id")]
        public required string Id { get; set; }

        ///<summary>
        ///Related Cover Id property
        ///</summary>
        [JsonProperty(PropertyName = "coverId")]
        public required string CoverId { get; set; }

        ///<summary>
        ///Date of creating the claim
        ///</summary>
        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }

        ///<summary>
        ///Claim name property
        ///</summary>
        [JsonProperty(PropertyName = "name")]
        public required string Name { get; set; }

        ///<summary>
        ///Type of the claim
        ///</summary>
        [JsonProperty(PropertyName = "claimType")]
        public ClaimType Type { get; set; }

        ///<summary>
        ///Damage Cost property
        ///</summary>
        [JsonProperty(PropertyName = "damageCost")]
        [Range(typeof(decimal), "0", "100000", ErrorMessage = "DamageCost cannot exceed 100.000")]
        public decimal DamageCost { get; set; }

    }
}
