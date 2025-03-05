namespace Lab8.Enrollment.Domain.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string Grade { get; set; }
    public string? BranchId { get; set; } = "default";
    public string SchemaName { get; set; } 

    public DateTime EnrollmentDate { get; set; }
}