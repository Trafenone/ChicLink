using System.ComponentModel.DataAnnotations;

namespace Api.Models.Messages;

public class CreateMessageRequest
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    [Required]
    public string MessageContent { get; set; } = null!;
}
