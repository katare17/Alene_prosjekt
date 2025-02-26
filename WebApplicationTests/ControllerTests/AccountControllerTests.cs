using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using Assert = Xunit.Assert;


namespace WebApplication1.Tests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<WebUser>> _userManagerMock;
        private readonly Mock<SignInManager<WebUser>> _signInManagerMock;
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ILogger<AccountController>> _loggerMock;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _userManagerMock = new Mock<UserManager<WebUser>>(
                Mock.Of<IUserStore<WebUser>>(), null, null, null, null, null, null, null, null);

            _signInManagerMock = new Mock<SignInManager<WebUser>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<WebUser>>(),
                null, null, null, null);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _loggerMock = new Mock<ILogger<AccountController>>();

            // Opprett controller med ekte DbContext
            _controller = new AccountController(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _dbContext,
                _loggerMock.Object);
        }

        [Fact]
        public async Task UserPage_ReturnsBadRequest_WhenGeoJsonOrDescriptionIsEmpty()
        {
            // Act
            var result = await _controller.UserPage("", "Description");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("GeoJson og beskrivelse må legges til", badRequestResult.Value);
        }

        [Fact]
        public async Task UserPage_ReturnsUnauthorized_WhenUserIdIsNotFound()
        {
            // Arrange
            var claims = new ClaimsPrincipal(new ClaimsIdentity()); // Ingen bruker-ID

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claims }
            };

            // Act
            var result = await _controller.UserPage("geojson", "description");

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Kunne ikke finne bruker", unauthorizedResult.Value);
        }

    }
}