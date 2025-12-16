using Microsoft.AspNetCore.SignalR;
using back.Data.Repositories;
using back.Models;

namespace back.Hubs;

public class TeamHub : Hub
{
    private readonly ITeamsRepository _teamsRepository;
    private readonly ILogger<TeamHub> _logger;

    public TeamHub(
        ITeamsRepository teamsRepository,
        ILogger<TeamHub> logger)
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
    Console.WriteLine($"========== PLAY CALLED ==========");
    Console.WriteLine($"Team ID: {teamId}");
    Console.WriteLine($"Connection: {Context.ConnectionId}");
    
    var team = await _teamsRepository.GetByIdAsync(int.Parse(teamId));
    Console.WriteLine($"Team current state - IsPlaying: {team.IsPlaying}");
    
    // TEMPORARILY REMOVE THIS CHECK
    // if(team.IsPlaying)
    //     return; 
    
    team.IsPlaying = true;
    team.StartedAtUtc = DateTime.UtcNow;
    
    await _teamsRepository.UpdateAsync(int.Parse(teamId), team);
    Console.WriteLine($"Team updated - IsPlaying: {team.IsPlaying}, StartedAt: {team.StartedAtUtc}");
    
    var broadcastState = new {
        team.CurrentSongIndex,
        team.IsPlaying,
        team.StartedAtUtc,
        team.ElapsedSeconds
    };
    
    Console.WriteLine($"Broadcasting: {System.Text.Json.JsonSerializer.Serialize(broadcastState)}");
    
    await Clients.Group(teamId)
        .SendAsync("PlaybackState", broadcastState);
        
    Console.WriteLine($"========== PLAY COMPLETED ==========");
}

public async Task Pause(string teamId)
{
    Console.WriteLine($"========== PAUSE CALLED ==========");
    Console.WriteLine($"Team ID: {teamId}");
    Console.WriteLine($"Connection: {Context.ConnectionId}");
    
    var team = await _teamsRepository.GetByIdAsync(int.Parse(teamId));
    Console.WriteLine($"Team current state - IsPlaying: {team.IsPlaying}, StartedAt: {team.StartedAtUtc}");
    
    // TEMPORARILY REMOVE THIS CHECK
    // if(!team.IsPlaying || team.StartedAtUtc == null)
    //     return;
    
    if (team.StartedAtUtc != null)
    {
        team.ElapsedSeconds += (DateTime.UtcNow - team.StartedAtUtc.Value).TotalSeconds;
    }
    team.StartedAtUtc = null;
    team.IsPlaying = false;
    
    await _teamsRepository.UpdateAsync(int.Parse(teamId), team);
    Console.WriteLine($"Team updated - IsPlaying: {team.IsPlaying}, ElapsedSeconds: {team.ElapsedSeconds}");
    
    var broadcastState = new {
        team.CurrentSongIndex,
        team.IsPlaying,
        team.StartedAtUtc,
        team.ElapsedSeconds
    };
    
    Console.WriteLine($"Broadcasting: {System.Text.Json.JsonSerializer.Serialize(broadcastState)}");
    
    await Clients.Group(teamId)
        .SendAsync("PlaybackState", broadcastState);
        
    Console.WriteLine($"========== PAUSE COMPLETED ==========");
}
}