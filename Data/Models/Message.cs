﻿using System.Text.Json.Serialization;

namespace Data.Models;

public class Message
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string MessageContent { get; set; } = null!;
    public DateTime Timestamp { get; set; }

    [JsonIgnore]
    public User Sender { get; set; } = null!;

    [JsonIgnore]
    public User Receiver { get; set; } = null!;
}
