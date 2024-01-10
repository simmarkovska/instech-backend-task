using Claims.Auditing;
using Claims.Auditing.Interfaces;
using Claims.Enums;
using Claims.Models;
using Claims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace Claims.Services
{
    public class CoverService : ControllerBase, ICoverService
    {
        private readonly Container _container;
        private readonly Auditer _auditer;
        private readonly IAuditContext _auditContext;

        public CoverService(CosmosClient dbClient,
            string databaseName,
            string containerName,
            IAuditContext auditContext)
        {
            if (dbClient == null)
                throw new ArgumentNullException(nameof(dbClient));

            _container = dbClient?.GetContainer("ClaimDb", "Cover")
                ?? throw new ArgumentNullException(nameof(dbClient));
            _auditContext = auditContext;
            _auditer = new Auditer(_auditContext);
        }

        public decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
        {

            decimal basePremiumRate = 1250;

            decimal multiplier = GetMultiplierValue(coverType);

            var premiumPerDay = basePremiumRate * multiplier;
            var insuranceLength = Math.Min(365, endDate.DayNumber - startDate.DayNumber);
            var totalPremium = 0m;

            decimal discountFactor = 0.05m;

            // Days for each discount period
            int daysNoDiscount = Math.Min(30, insuranceLength);
            int daysFirstDiscount = Math.Min(180 - daysNoDiscount, insuranceLength - daysNoDiscount);
            int daysSecondDiscount = Math.Min(365 - daysNoDiscount - daysFirstDiscount, insuranceLength - daysNoDiscount - daysFirstDiscount);

            // Calculate total premium without discount for the first 30 days
            totalPremium += premiumPerDay * daysNoDiscount;

            // Calculate discount for the next 150 days
            totalPremium += premiumPerDay * daysFirstDiscount * (1 - (coverType == CoverType.Yacht ? discountFactor : 0.02m));

            // Calculate discount for the remaining days
            totalPremium += premiumPerDay * daysSecondDiscount * (1 - (coverType == CoverType.Yacht ? discountFactor : 0.03m));

            return totalPremium;
        }

        private decimal GetMultiplierValue(CoverType coverType)
        {
            switch (coverType)
            {
                case CoverType.Yacht:
                    return 1.1m;
                case CoverType.PassengerShip:
                    return 1.2m;
                case CoverType.Tanker:
                    return 1.5m;
                default:
                    return 1.3m;
            }
        }


        public async Task<IEnumerable<Cover>> GetCoversAsync()
        {
            var query = _container.GetItemQueryIterator<Cover>(new QueryDefinition("SELECT * FROM c"));
            var results = new List<Cover>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<Cover> GetCoverAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Cover>(id, new(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task AddItemAsync(Cover item)
        {
            await _container.CreateItemAsync(item, new PartitionKey(item.Id));

            await _auditer.AuditCover(item.Id, "POST");
        }
        public async Task DeleteItemAsync(string id)
        {
            await _auditer.AuditCover(id, "DELETE");

            await _container.DeleteItemAsync<Cover>(id, new PartitionKey(id));
        }

        public IActionResult DateValidation(Cover item)
        {
            DateTime coverStartDate = new DateTime(item.StartDate.Year, item.StartDate.Month, item.StartDate.Day);

            if (coverStartDate.Date < DateTime.Now.Date)
            {
                return BadRequest("StartDate cannot be in the past.");
            }
            else
            {
                return Ok();
            }
        }
    }
}
