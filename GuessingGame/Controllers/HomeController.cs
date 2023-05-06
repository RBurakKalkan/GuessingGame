using GuessingGame.Models.ViewModels;
using GuessingGame.Services;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult StartGame()
    {
        return RedirectToAction("StartGame", "Game");
    }
}
