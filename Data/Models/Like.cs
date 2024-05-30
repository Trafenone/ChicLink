namespace Data.Models;

public class Like
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public DateTime Timestamp { get; set; }
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!; 
}
