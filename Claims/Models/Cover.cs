using Claims.Enums;
using Newtonsoft.Json;

namespace Claims.Models;

///<summary>
///Cover Model
///</summary>
public class Cover
{
    ///<summary>
    ///Cover Id property
    ///</summary>
    [JsonProperty(PropertyName = "id")]
    public required string Id { get; set; }

    ///<summary>
    ///Start Date property
    ///</summary>
    [JsonProperty(PropertyName = "startDate")]
    public DateOnly StartDate { get; set; }

    ///<summary>
    ///End Date property
    ///</summary>
    [JsonProperty(PropertyName = "endDate")]
    public DateOnly EndDate { get; set; }

    ///<summary>
    ///Type of the cover
    ///</summary>
    [JsonProperty(PropertyName = "claimType")]
    public CoverType Type { get; set; }

    ///<summary>
    ///Premium property
    ///</summary>
    [JsonProperty(PropertyName = "premium")]
    public decimal Premium { get; set; }
}