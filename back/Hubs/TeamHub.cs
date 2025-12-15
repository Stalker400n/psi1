using Microsoft.AspNetCore.SignalR;
using back.Repositories;
using back.Models;

namespace back.Hubs;

public class TeamHub : Hub
{

    private readonly ITeamsRepository _teamsRepository;
    private readonly ILogger<TeamsController> _logger;


    public TeamHub(
      ITeamsRepository teamsRepository,
      ILogger<TeamsController> logger)
    {
      if (teamsRepository == null) throw new ArgumentNullException(nameof(teamsRepository));
      if (logger == null) throw new ArgumentNullException(nameof(logger));
      
      _teamsRepository = teamsRepository;
      _logger = logger;
    }




    public async Task JoinTeam(string teamId)
    {
        Console.WriteLine($"User {Context.ConnectionId} joining team {teamId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, teamId);

        var team = await _teamsRepository.GetByIdAsync(int.Parse(teamId));

        await Clients.Caller.SendAsync("PlaybackState", new {
            team.CurrentSongIndex,
            team.IsPlaying,
            team.StartedAtUtc,
            team.ElapsedSeconds
        });
    }

    public async Task LeaveTeam(string teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamId);
    }


    public async Task Play(string teamId)
    {
        var team = await _teamsRepository.GetByIdAsync(int.Parse(teamId));

        if(state.isPlaying)
            return; 


        team.isPlaying = true;
        team.StartedAtUtc = DateTime.UtcNow;

        await _teamsRepository.updateAsync(int.Parse(teamId), team);

        await Clients.Group(teamId)
            .SendAsync("PlaybackState", team)
    }

    public async Task Pause(string teamId)
    {
        var team = await _teamsRepository(int.Parse(teamId));

        if(!team.isPlaying || team.StartedAtUtc == null)
            return;

        team.ElapsedSeconds +=
            (DateTime.UtcNow - team.StartedAtUtc.Value).TotalSeconds;

        team.StartedAtUtc = null;
        team.IsPlaying = false;

        await _teamsRepository.updateAsync(int.Parse(teamId), team);

        await Clients.Group(teamId)
            .SendAsync("PlaybackState", team);
    }
}