using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using YourApp.Controllers;
using YourApp.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Tests.Controller.Tests
{
    [TestClass]
    public class AccountControllerTests
    {
        private Mock<UserManager<WebUser>> _mockUserManager;
        private Mock<SignInManager<WebUser>> _mockSignInManager;
        private AccountController _controller;

        [TestInitialize]
        public void SetUp()
        {
            _mockUserManager = new Mock<UserManager<WebUser>>(
                Mock.Of<IUserStore<WebUser>>(), null, null, null, null, null, null, null, null);
            _mockSignInManager = new Mock<SignInManager<WebUser>>(
                _mockUserManager.Object, null, null, null, null, null, null);

            _controller = new AccountController(_mockUserManager.Object, _mockSignInManager.Object, null, null);
        }

        [TestMethod]
        public async Task Register_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var result = await _controller.Register(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("UserPage", result.ActionName);
        }
    }
}