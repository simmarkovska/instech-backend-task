using Covers.Auditing;
using Covers.Auditing.Interfaces;
using Covers.Enums;
using Covers.Models;
using Covers.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;

namespace Covers.Services
{
    ///<summary>
    ///CoverService
    ///</summary>
    public class CoverService : ControllerBase, ICoverService
    {
        private readonly Container _container;
        private readonly IAuditer _auditer;
        private readonly IAuditContext _auditContext;

        ///<summary>
        ///CoverService constructor
        ///</summary>
        public CoverService(CosmosClient dbClient,
            string databaseName,
            string containerName,
            IAuditContext auditContext)
        {
            if (dbClient == null)
                throw new ArgumentNullException(nameof(dbClient));

            if (dbClient == null)
                throw new ArgumentNullException(nameof(dbClient));
            _container = dbClient.GetContainer(databaseName, containerName);
            _auditContext = auditContext;
            _auditer = new Auditer(_auditContext);
        }

        ///<summary>
        ///Retrieve list of all covers from database
        ///</summary>
        public async Task<IEnumerable<Cover>> GetCoversAsync()
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        ///<summary>
        ///Add new cover in database
        ///</summary>
        public async Task AddItemAsync(Cover item)
        {
            try
            {
                item.Id = Guid.NewGuid().ToString();
                await _container.CreateItemAsync(item, new PartitionKey(item.Id));
                await _auditer.AuditCover(item.Id, "POST");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        ///<summary>
        ///Retrieve cover from database by unique Cover Id
        ///</summary>
        public async Task<Cover> GetCoverAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Cover>(id, new(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"No cover with id {id} is found.");
            }
        }


        ///<summary>
        ///Delete cover from database by unique Cover Id
        ///</summary>
        public async Task DeleteItemAsync(string id)
        {
            try
            {
                await _auditer.AuditCover(id, "DELETE");
                await _container.DeleteItemAsync<Cover>(id, new PartitionKey(id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"No cover with id {id} is found.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        ///<summary>
        ///Checks if the Cover Start Date is greater or equal to today's date
        ///</summary>
        public IActionResult DateValidation(Cover item)
        {
            DateTime coverStartDate = new DateTime(item.StartDate.Year, item.StartDate.Month, item.StartDate.Day);
            DateTime coverEndDate = new DateTime(item.EndDate.Year, item.EndDate.Month, item.EndDate.Day);

            if (coverStartDate.Date < DateTime.Now.Date)
            {
                return BadRequest("StartDate cannot be in the past.");
            }
            else if(coverStartDate.Date > coverEndDate.Date)
            {
                return BadRequest("StartDate cannot be greater than EndDate.");
            }
            else
            {
                return Ok();
            }
        }

        ///<summary>
        ///Calculates premium
        ///</summary>
        public decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
        {

            decimal basePremiumRate = 1250;

            decimal multiplier = GetMultiplierValue(coverType);

            var premiumPerDay = basePremiumRate * multiplier;
            var insuranceLength = endDate.DayNumber - startDate.DayNumber;
            if(insuranceLength > 365)
            {
                throw new InvalidOperationException("Total insurance period cannot exceed 1 year.");
            }
            var totalPremium = 0m;

            decimal yachtDiscount = 0.05m;

            // Days for each discount period
            int daysNoDiscount = Math.Min(30, insuranceLength);
            int daysFirstDiscount = Math.Min(180 - daysNoDiscount, insuranceLength - daysNoDiscount);
            int daysSecondDiscount = Math.Min(365 - daysNoDiscount - daysFirstDiscount, insuranceLength - daysNoDiscount - daysFirstDiscount);

            // Calculate total premium without discount for the first 30 days
            totalPremium += premiumPerDay * daysNoDiscount;

            // Calculate discount for the next 150 days
            totalPremium += premiumPerDay * daysFirstDiscount * (1 - (coverType == CoverType.Yacht ? yachtDiscount : 0.02m));

            // Calculate discount for the remaining days
            totalPremium += premiumPerDay * daysSecondDiscount * (1 - (coverType == CoverType.Yacht ? yachtDiscount : 0.03m));

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
    }
}
