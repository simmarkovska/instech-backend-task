using Claims.Enums;
using Claims.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Claims.Tests
{
    public class ClaimModelTests
    {
        [Fact]
        public void DamageCostValidation()
        {
            var claimId = "1";
            var claim = new Claim
            {
                Id = claimId,
                CoverId = "1",
                Created = DateTime.Parse("2024-01-06T16:01:31.710Z"),
                Name = "TestName",
                Type = ClaimType.Collision
            };

            // valid value in the range (0, 100000)
            claim.DamageCost = 10;

            var validationContext = new ValidationContext(claim)
            {
                MemberName = nameof(Claim.DamageCost)
            };
            var validationResult = Validator.TryValidateProperty(claim.DamageCost, validationContext, null);

            Assert.True(validationResult, "Validation passed");

            // invalid value out of the range (0, 100000)
            claim.DamageCost =-1;

            validationResult = Validator.TryValidateProperty(claim.DamageCost, validationContext, null);

            Assert.False(validationResult, "Wrong DamageCost value");
        }
    }
}
