using Microsoft.AspNetCore.Mvc;
using back.Models;
using back.Data.Repositories;

namespace back.Controllers;

[ApiController]
[Route("users")]
public class GlobalUsersController : ControllerBase
{
  private readonly IGlobalUsersRepository _globalUsersRepository;
  private readonly ILogger<GlobalUsersController> _logger;

  public GlobalUsersController(
    IGlobalUsersRepository globalUsersRepository,
    ILogger<GlobalUsersController> logger)
  {
    _globalUsersRepository = globalUsersRepository ?? throw new ArgumentNullException(nameof(globalUsersRepository));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  [HttpPost("register-or-login")]
  public async Task<ActionResult<GlobalUserResponse>> RegisterOrLogin(
    [FromBody] LoginRequest request)
  {
    if (string.IsNullOrWhiteSpace(request.Name))
      return BadRequest(new { message = "Name is required" });

    if (string.IsNullOrWhiteSpace(request.DeviceFingerprint))
      return BadRequest(new { message = "Device fingerprint is required" });

    // Try to find existing user with this name
    var existingUser = await _globalUsersRepository.GetByNameAsync(request.Name);

    if (existingUser != null)
    {
      // Name exists - check if fingerprint matches
      if (existingUser.DeviceFingerprint == request.DeviceFingerprint)
      {
        // MATCH! User is authenticated from their device
        existingUser.LastSeenAt = DateTime.UtcNow;
        await _globalUsersRepository.UpdateAsync(existingUser.Id, existingUser);

        _logger.LogInformation(
          "User {Name} authenticated successfully from known device",
          request.Name);

        return Ok(new GlobalUserResponse
        {
          Id = existingUser.Id,
          Name = existingUser.Name,
          IsNew = false
        });
      }
      else
      {
        // NO MATCH! Name taken from different device
        _logger.LogWarning(
          "Authentication failed: Name {Name} attempted from different device",
          request.Name);

        return StatusCode(403, new
        {
          message = "This name is already taken from another device. Please choose a different name."
        });
      }
    }

    // Name doesn't exist - create new user
    var newUser = await _globalUsersRepository.CreateAsync(new GlobalUser
    {
      Name = request.Name,
      DeviceFingerprint = request.DeviceFingerprint,
      DeviceInfo = request.DeviceInfo ?? "Unknown Device"
    });

    _logger.LogInformation(
      "New user {Name} created with device fingerprint",
      request.Name);

    return Ok(new GlobalUserResponse
    {
      Id = newUser.Id,
      Name = newUser.Name,
      IsNew = true
    });
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<GlobalUser>> GetById(int id)
  {
    var user = await _globalUsersRepository.GetByIdAsync(id);
    if (user == null) return NotFound(new { message = "User not found" });
    return Ok(user);
  }
}
