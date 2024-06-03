using Api.Models.Users;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public UsersController(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    /// <returns>List of users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await _context.Users.ToListAsync());
    }

    /// <summary>
    /// Get user by ID.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <returns>User details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetUser(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .Include(u => u.LikesSent)
            .Include(u => u.LikesReceived)
            .Include(u => u.MessagesSent)
            .Include(u => u.MessagesReceived)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Update user details.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="request">User update request.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NoContent), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request)
    {
        if (id != request.UserId)
            return BadRequest();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        user.FirstName = request.Firstname;
        user.LastName = request.Lastname;
        user.Location = request.Location;

        _context.Update(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
