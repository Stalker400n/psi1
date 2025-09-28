using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using psi1.Models;

namespace psi1.Services;
public class TeamService
{
    private static List<TeamViewModel> _teams = new List<TeamViewModel>();
    private static int _nextTeamId = 1;

    public void AddTeam(psi1.Models.TeamViewModel model)
    {
        model.Id = _nextTeamId++;
        _teams.Add(model);
    }

    public List<TeamViewModel> GetTeams()
    {
        return _teams;
    }
}