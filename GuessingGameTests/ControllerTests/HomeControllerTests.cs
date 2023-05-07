using Microsoft.AspNetCore.Mvc;

namespace GuessingGameTests.ControllerTests
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _controller = new HomeController();
        }

        [Fact]
        public void Index_ShouldReturnViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void StartGame_ShouldRedirectToGameStartGameAction()
        {
            // Act
            var result = _controller.StartGame();

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StartGame", redirectToActionResult.ActionName);
            Assert.Equal("Game", redirectToActionResult.ControllerName);
        }
    }
}
