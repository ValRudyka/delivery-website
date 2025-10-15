namespace delivery_website.Models.Entities
{
    public class SystemLog
    {
        public Guid LogId { get; set; }
        public string LogLevel { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string? StackTrace { get; set; }
        public string? UserId { get; set; }
        public string? RequestPath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
