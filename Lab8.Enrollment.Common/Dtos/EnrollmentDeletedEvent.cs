namespace Lab8.Enrollment.Common.Dtos;

public class EnrollmentDeletedEvent
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string BranchId { get; set; }
    public string OriginService { get; set; }
}