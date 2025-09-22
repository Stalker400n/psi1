using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using psi1.Models;

namespace psi1.Controllers;
public class HomeController : Controller
{
    private static List<TeamViewModel> _Teams = new List<TeamViewModel>();
    private static int _nextTeamId = 1; 

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult CreateTeam()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateTeam(psi1.Models.TeamViewModel model)
    {
        if (ModelState.IsValid)
        {
            model.Id = _nextTeamId++;
            _Teams.Add(model);
            return RedirectToAction(nameof(BrowseTeams));
        }
        return View(model);
    }


    public IActionResult BrowseTeams()
    {
        return View(_Teams);
    }

    public IActionResult JoinTeam()
    {
        return View();
    }

}

