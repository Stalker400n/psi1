using Microsoft.AspNetCore.SignalR;
using back.Models;

namespace back.Hubs;

public class TeamHub : Hub
{
    public async Task JoinTeam(string teamId)
    {
        Console.WriteLine($"User {Context.ConnectionId} joining team {teamId}");
        await Groups.AddToGroupAsync(Context.ConnectionId, teamId);
    }

    public async Task LeaveTeam(string teamId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamId);
    }

    public async Task SendPlayState(string teamId, bool isPlaying, double playbackPosition)
    {
        await Clients.Group(teamId).SendAsync("ReceivePlayState", isPlaying, playbackPosition);
    }

    public async Task SendSeek(string teamId, double playbackPosition)
    {
        await Clients.Group(teamId).SendAsync("ReceiveSeek", playbackPosition);
    }
}