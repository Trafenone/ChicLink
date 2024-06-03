using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// Controller for managing likes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LikesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LikesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a like from one user to another.
    /// </summary>
    /// <remarks>
    /// Creates a like record if it does not already exist
    /// </remarks>
    /// <param name="senderId">ID of the user sending the like.</param>
    /// <param name="receiverId">ID of the user receiving the like.</param>
    /// <returns>Action result with the like information.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Like), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLike(Guid senderId, Guid receiverId)
    {
        if (senderId == receiverId)
            return BadRequest("You cannot like yourself.");

        var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);
        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == receiverId);

        if (sender == null || receiver == null)
            return NotFound("User not found.");

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.SenderId == senderId && l.ReceiverId == receiverId);
        if (existingLike != null)
            return BadRequest("You already liked this user.");

        var like = new Like
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Timestamp = DateTime.UtcNow,
        };

        await _context.Likes.AddAsync(like);
        await _context.SaveChangesAsync();

        return Ok(like);
    }

    /// <summary>
    /// Gets the likes received by a user.
    /// </summary>
    /// <remarks>
    /// Returns a list of likes received by the specified user.
    /// </remarks>
    /// <param name="userId">ID of the user.</param>
    /// <returns>Action result with the list of likes received by the user.</returns>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<Like>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserLikes(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.LikesReceived)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound("User not found.");

        var likes = user.LikesReceived;

        return Ok(likes);
    }

    /// <summary>
    /// Deletes a like.
    /// </summary>
    /// <remarks>
    /// Removes a like record if it exists.
    /// </remarks>
    /// <param name="senderId">ID of the user who sent the like.</param>
    /// <param name="receiverId">ID of the user who received the like.</param>
    /// <returns>Action result indicating the outcome of the operation.</returns>
    [HttpDelete("{senderId}/{receiverId}")]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLike(Guid senderId, Guid receiverId)
    {
        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.SenderId == senderId && l.ReceiverId == receiverId);
        if (like == null)
            return NotFound("Like not found.");

        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
