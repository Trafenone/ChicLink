using Data;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LikesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LikesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
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

    [HttpGet("{userId:guid}")]
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

    [HttpDelete("{senderId}/{receiverId}")]
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
