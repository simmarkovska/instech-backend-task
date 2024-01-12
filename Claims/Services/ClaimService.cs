using Claims.Models;
using Claims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace Claims.Services
{
    ///<summary>
    ///ClaimSerice
    ///</summary>
    public class ClaimService : ControllerBase, IClaimService
    {
        private readonly Container _container;
        private string coverEndpoint;

        ///<summary>
        ///ClaimService constructor
        ///</summary>
        public ClaimService(CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            if (dbClient == null)
                throw new ArgumentNullException(nameof(dbClient));
            _container = dbClient.GetContainer(databaseName, containerName);
            coverEndpoint = GetCoverEndpoint();
        }

        ///<summary>
        ///Returns Cover Endpoint
        ///</summary>
        public string GetCoverEndpoint()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string? coverServiceEndpoint = configuration["CoverServiceEndpoint"];

            if (coverServiceEndpoint != null)
            {
                return coverServiceEndpoint;
            }
            else
            {
                throw new InvalidOperationException("CoverServiceEndpoint configuration value is missing.");
            }
        }

        ///<summary>
        ///Retrieve list of all claims from database
        ///</summary>
        public async Task<IEnumerable<Claim>> GetClaimsAsync()
        {
            try
            {
                var query = _container.GetItemQueryIterator<Claim>(new QueryDefinition("SELECT * FROM c"));
                var results = new List<Claim>();
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();

                    results.AddRange(response.ToList());
                }
                return results;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        ///<summary>
        ///Add new claim in database
        ///</summary>
        public async Task AddItemAsync(Claim item)
        {
            try
            {
                item.Id = Guid.NewGuid().ToString();
                await _container.CreateItemAsync(item, new PartitionKey(item.Id));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return;
        }

        ///<summary>
        ///Retrieve claim from database by unique Claim Id
        ///</summary>
        public async Task<Claim> GetClaimAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Claim>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"No claim with id {id} is found.");
            }
        }

        ///<summary>
        ///Delete claim from database by unique Claim Id
        ///</summary>
        public async Task DeleteItemAsync(string id)
        {
            try
            {
                await _container.DeleteItemAsync<Claim>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"No claim with id {id} is found.");
            }
            return;
        }

        ///<summary>
        ///Checks if date of creating the claim is in range of start and end date of the related cover
        ///</summary>
        public async Task<IActionResult> DateValidation(Claim item)
        {
            using (HttpClient client = new HttpClient())
            {

                string apiMethodEndpoint = $"{coverEndpoint}Covers/{item.CoverId}";

                HttpResponseMessage response = await client.GetAsync(apiMethodEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string cover = await response.Content.ReadAsStringAsync();
                    JsonDocument jsonDocument = JsonDocument.Parse(cover);
                    JsonElement root = jsonDocument.RootElement;
                    string? startDateTemp = root.GetProperty("startDate").GetString();
                    string? endDateTemp = root.GetProperty("endDate").GetString();

                    if (startDateTemp!=null && endDateTemp != null)
                    {
                        DateOnly startDate = DateOnly.Parse(startDateTemp);
                        DateOnly endDate = DateOnly.Parse(endDateTemp);
                        DateTime coverStartDate = new DateTime(startDate.Year, startDate.Month, startDate.Day);
                        DateTime coverEndDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);

                        if(item.Created.CompareTo(coverStartDate) < 0 || item.Created.CompareTo(coverEndDate) > 0)
                        {
                            return BadRequest("Created date must be within the period of the related Cover");
                        }
                        else
                        {
                            return Ok();
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("startDate or endDate property is missing.");

                    }
                }
                else
                {
                    return BadRequest("Cover not found");
                }
            }
        }
    }
}
