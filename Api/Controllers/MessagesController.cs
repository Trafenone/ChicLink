using Api.Models.Messages;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{userId:guid}/sentMessages")]
    public async Task<ActionResult> GetSentMessages(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.MessagesSent)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound();

        var sentMessages = user.MessagesSent;

        return Ok(sentMessages);
    }

    [HttpGet("{userId:guid}/receivedMessages")]
    public async Task<ActionResult> GetReceivedMessages(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.MessagesReceived)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return NotFound();

        var receivedMessages = user.MessagesReceived;

        return Ok(receivedMessages);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] CreateMessageRequest request)
    {
        var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.SenderId);
        var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.ReceiverId);

        if (sender == null || receiver == null)
            return BadRequest("Invalid sender or receiver.");

        var message = new Message
        {
            SenderId = request.SenderId,
            ReceiverId = request.ReceiverId,
            MessageContent = request.MessageContent,
            Timestamp = DateTime.UtcNow
        };

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();

        return Ok(message);
    }
}
