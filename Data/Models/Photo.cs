namespace Data.Models
{
    public class Photo
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
