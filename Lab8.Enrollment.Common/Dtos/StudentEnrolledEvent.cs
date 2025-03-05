namespace Lab8.Enrollment.Common.Dtos;

public class StudentEnrolledEvent
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public string BranchId { get; set; }
    public DateTime EnrollmentDate { get; set; }
}