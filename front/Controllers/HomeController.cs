using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using psi1.Models;
using psi1.Services;

namespace psi1.Controllers;
public class HomeController : Controller
{
    private readonly TeamService teamService;

    public HomeController(TeamService teamService)
    {
        this.teamService = teamService;
    }
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult CreateTeam()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateTeam(TeamViewModel model)
    {
        if (ModelState.IsValid)
        {
            teamService.AddTeam(model);
            return RedirectToAction(nameof(BrowseTeams));
        }
        return View(model);
    }


    public IActionResult BrowseTeams()
    {
        return View(teamService.GetTeams());
    }

    public IActionResult JoinTeam()
    {
        return View();
    }

}

