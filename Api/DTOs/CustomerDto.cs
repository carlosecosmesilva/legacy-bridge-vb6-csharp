namespace Api.DTOs
{
    public class CustomerDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Document { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
