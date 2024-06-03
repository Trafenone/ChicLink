using Api.Models.Messages;
using Data;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// Controller for managing messages.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MessagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the messages sent by a user.
    /// </summary>
    /// <remarks>
    /// Returns a list of messages sent by the specified user.
    /// </remarks>
    /// <param name="userId">ID of the user.</param>
    /// <returns>Action result with the list of sent messages.</returns>
    [HttpGet("{userId:guid}/sentMessages")]
    [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Gets the messages received by a user.
    /// </summary>
    /// <remarks>
    /// Returns a list of messages received by the specified user.
    /// </remarks>
    /// <param name="userId">ID of the user.</param>
    /// <returns>Action result with the list of received messages.</returns>
    [HttpGet("{userId:guid}/receivedMessages")]
    [ProducesResponseType(typeof(IEnumerable<Message>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Sends a message from one user to another.
    /// </summary>
    /// <remarks>
    /// Creates a message record and saves it to the database.
    /// </remarks>
    /// <param name="request">Message request containing sender, receiver, and message content.</param>
    /// <returns>Action result with the sent message information.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Message), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
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
