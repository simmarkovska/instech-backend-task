using Covers.Controllers;
using Covers.Enums;
using Covers.Models;
using Covers.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Covers.Tests
{
    public class CoversControllerTests
    {
        private readonly CoversController _coversController;
        private readonly Mock<ILogger<CoversController>> logger;
        private readonly Mock<ICoverService> coverServiceMock;
        private readonly Cover cover;
        private readonly string coverId;

        public CoversControllerTests()
        {
            logger = new Mock<ILogger<CoversController>>();
            coverServiceMock = new Mock<ICoverService>();
            _coversController = new CoversController(logger.Object,
                coverServiceMock.Object);
            coverId = "1";
            cover = new Cover
            {
                Id = coverId,
                StartDate = new DateOnly(2023, 1, 1),
                EndDate = new DateOnly(2024, 4, 5),
                Type = CoverType.Yacht,
                Premium = 1000
            };
        }



        [Fact]
        public async Task GetAsync_Covers()
        {
            coverServiceMock.Setup(service => service.GetCoversAsync()).ReturnsAsync(new List<Cover>());

            var result = await _coversController.GetAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsAssignableFrom<IEnumerable<Cover>>(okResult.Value);
        }

        [Fact]
        public async Task Get_Cover()
        {
            coverServiceMock.Setup(service => service.GetCoverAsync(coverId)).ReturnsAsync(cover);

            var result = await _coversController.GetAsync(coverId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var coverResult = Assert.IsType<Cover>(okResult.Value);
            Assert.Equal(cover, coverResult);
        }

        [Fact]
        public async Task CreateAsync_Cover()
        {
            coverServiceMock.Setup(service => service.DateValidation(cover)).Returns(new OkResult());
            coverServiceMock.Setup(service => service.ComputePremium(cover.StartDate, cover.EndDate, cover.Type)).Returns(1500m);

            var result = await _coversController.CreateAsync(cover);

            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var returnedCover = Assert.IsType<Cover>(okResult.Value);
            Assert.Equal(cover.Id, returnedCover.Id);
            Assert.Equal(1500m, returnedCover.Premium);

            // Verify that ComputePremium and AddItemAsync are called with the correct arguments
            coverServiceMock.Verify(service => service.ComputePremium(cover.StartDate, cover.EndDate, cover.Type), Times.Once);
            coverServiceMock.Verify(service => service.AddItemAsync(cover), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Cover_ValidationErrror()
        {
            var validationErrorMessage = "StartDate cannot be in the past.";
            coverServiceMock.Setup(service => service.DateValidation(cover)).Returns(new BadRequestObjectResult(validationErrorMessage));

            var result = await _coversController.CreateAsync(cover);

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);

            var badRequestResult = result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);

            Assert.Equal(validationErrorMessage, badRequestResult.Value);

            // Verify that ComputePremium and AddItemAsync are not called in case of invalid input
            coverServiceMock.Verify(service => service.ComputePremium(It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CoverType>()), Times.Never);
            coverServiceMock.Verify(service => service.AddItemAsync(It.IsAny<Cover>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Cover()
        {
            await _coversController.DeleteAsync(coverId);

            // Verify that DeleteItemAsync is called with the correct argument
            coverServiceMock.Verify(service => service.DeleteItemAsync(coverId), Times.Once);
        }      
    }
}
