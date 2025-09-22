using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using psi1.Models;

namespace psi1.Controllers;
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult CreateTeam()
    {
        return View();
    }

    public IActionResult BrowseTeams()
    {
        return View();
    }

    public IActionResult JoinTeam()
    {
        return View();
    }
}

