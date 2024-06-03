namespace Data.Models
{
    public class Photo
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public Guid ProfileId { get; set; }
        public Profile Profile { get; set; } = null!;
    }
}
