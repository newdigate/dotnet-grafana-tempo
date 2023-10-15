using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using dotnet_tempo_2.Models;

namespace dotnet_tempo_2.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private static ActivitySource indexSource = new ActivitySource("TraceActivityName");

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        using (var activity = indexSource.CreateActivity("Index", ActivityKind.Internal))
        activity?.SetTag("tenant","goofy-goobers");
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
