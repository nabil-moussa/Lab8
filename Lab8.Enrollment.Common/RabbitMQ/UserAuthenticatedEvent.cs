namespace Lab8.Enrollment.Common.RabbitMQ;

public class UserAuthenticatedEvent
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string BranchId { get; set; }
    public string JwtToken { get; set; }
    public DateTime TokenExpiration { get; set; }
}