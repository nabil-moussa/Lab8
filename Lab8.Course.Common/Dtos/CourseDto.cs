namespace Lab8.Course.Common.Dtos;

public class CourseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MaxStudents { get; set; }
    public DateTime EnrollmentStartDate { get; set; }
    public DateTime EnrollmentEndDate { get; set; }
}