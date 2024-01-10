using Claims.Controllers;
using Claims.Enums;
using Claims.Models;
using Claims.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests
    {
        private readonly ClaimsController _claimsController;
        private readonly Mock<ILogger<ClaimsController>> logger;
        private readonly Mock<IClaimService> claimServiceMock;
        private Mock<IMessageService> messageServiceMock;
        private readonly Claim claim;
        private readonly string claimId;
        public ClaimsControllerTests()
        {
            claimServiceMock = new Mock<IClaimService>();
            logger = new Mock<ILogger<ClaimsController>>();
            messageServiceMock = new Mock<IMessageService>();
            _claimsController = new ClaimsController(logger.Object, 
                claimServiceMock.Object, messageServiceMock.Object){ };
            claimId = "1";
            claim = new Claim
            {
                Id = claimId,
                CoverId = "1",
                Created = DateTime.Parse("2024-01-06T16:01:31.710Z"),
                Name = "TestName",
                Type = ClaimType.Collision,
                DamageCost = 1000
            };

        }

        [Fact]
        public async Task Get_Claims()
        {
            var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(_ =>
            {});

            var client = application.CreateClient();

            var response = await client.GetAsync("/Claims");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var claims = JsonConvert.DeserializeObject<IEnumerable<Claim>>(content);

            //TODO: Apart from ensuring 200 OK being returned, what else can be asserted?

            Assert.NotNull(response);
            Assert.IsAssignableFrom<IEnumerable<Claim>>(claims);
        }

        [Fact]
        public async Task Create_Claim()
        {
            claimServiceMock.Setup(service => service.DateValidation(claim)).ReturnsAsync(new OkResult());

            var result = await _claimsController.CreateAsync(claim);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var returnedClaim = okResult.Value as Claim;
            Assert.NotNull(returnedClaim);

        }

        [Fact]
        public async Task Create_Claim_ValidationError()
        {
            var validationErrorMessage = "Created date must be within the period of the related Cover";
            claimServiceMock.Setup(service => service.DateValidation(claim)).ReturnsAsync(new BadRequestObjectResult(validationErrorMessage));

            var result = await _claimsController.CreateAsync(claim);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);

            Assert.Equal(validationErrorMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task Get_Claim()
        {
            claimServiceMock.Setup(service => service.GetClaimAsync(claimId)).ReturnsAsync(claim);

            var result = await _claimsController.GetAsync(claimId);

            Assert.NotNull(result);
            Assert.IsType<Claim>(result);
            Assert.Equal(claim, result);

        }

        [Fact]
        public async Task Delete_Claim()
        {
            await _claimsController.DeleteAsync(claimId);

            // Verify that SendMessage method was called
            messageServiceMock.Verify(
                service => service.SendMessage(It.IsAny<object>()), Times.Once);

            // Verify that DeleteItemAsync method was called with the expected parameter
            claimServiceMock.Verify(service => service.DeleteItemAsync(claimId), Times.Once);

        }

    }
}
