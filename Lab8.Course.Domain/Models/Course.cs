namespace Lab8.Course.Domain.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MaxStudents { get; set; }
    public DateTime EnrollmentStartDate { get; set; }
    public DateTime EnrollmentEndDate { get; set; }
    public string? BranchId { get; set; }="default";
        public string SchemaName { get; set; } // Add schema name


    protected Course() { }

    public Course(string name, int maxStudents, DateTime enrollmentStartDate, DateTime enrollmentEndDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name cannot be empty", nameof(name));
            
        if (maxStudents <= 0)
            throw new ArgumentException("Max students must be greater than zero", nameof(maxStudents));
            
        if (enrollmentEndDate <= enrollmentStartDate)
            throw new ArgumentException("Enrollment end date must be after start date");

        Name = name;
        MaxStudents = maxStudents;
        EnrollmentStartDate = enrollmentStartDate;
        EnrollmentEndDate = enrollmentEndDate;
    }

    public void UpdateDetails(string name, int maxStudents, DateTime enrollmentStartDate, DateTime enrollmentEndDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Course name cannot be empty", nameof(name));
            
        if (maxStudents <= 0)
            throw new ArgumentException("Max students must be greater than zero", nameof(maxStudents));
            
        if (enrollmentEndDate <= enrollmentStartDate)
            throw new ArgumentException("Enrollment end date must be after start date");

        Name = name;
        MaxStudents = maxStudents;
        EnrollmentStartDate = enrollmentStartDate;
        EnrollmentEndDate = enrollmentEndDate;
    }
}