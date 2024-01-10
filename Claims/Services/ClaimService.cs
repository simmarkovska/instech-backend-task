using Claims.Models;
using Claims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace Claims.Services
{
    public class ClaimService : ControllerBase, IClaimService
    {
        private readonly Container _container;
        private readonly ICoverService _coverService;

        public ClaimService(CosmosClient dbClient,
            string databaseName, 
            string containerName,
            ICoverService coverService)
        {
            if (dbClient == null) 
                throw new ArgumentNullException(nameof(dbClient));
            _container = dbClient.GetContainer(databaseName, containerName);
            _coverService = coverService ??
                throw new ArgumentNullException(nameof(coverService)); ;
        }
        public async Task<IEnumerable<Claim>> GetClaimsAsync()
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

        public async Task<Claim> GetClaimAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Claim>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
        public Task AddItemAsync(Claim item)
        {
            return _container.CreateItemAsync(item, new PartitionKey(item.Id));
        }
        public Task DeleteItemAsync(string id)
        {
            return _container.DeleteItemAsync<Claim>(id, new PartitionKey(id));
        }

        public async Task<IActionResult> DateValidation(Claim item)
        {
            Cover cover = await _coverService.GetCoverAsync(item.CoverId);

                if (cover != null)
                { 
                    DateTime coverStartDate = new DateTime(cover.StartDate.Year, cover.StartDate.Month, cover.StartDate.Day);
                    DateTime coverEndDate = new DateTime(cover.EndDate.Year, cover.EndDate.Month, cover.EndDate.Day);

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
                    return BadRequest("Cover not found");
                }
        }
    }
}
