namespace Lab8.Enrollment.Common.Dtos;

public class CourseCreatedEvent
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MaxStudents { get; set; }
    public DateTime EnrollmentStartDate { get; set; }
    public DateTime EnrollmentEndDate { get; set; }
    public string BranchId { get; set; }
    public string OriginService { get; set; }

}