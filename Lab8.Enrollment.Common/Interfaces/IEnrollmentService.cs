using Lab8.Enrollment.Common.Dtos;

namespace Lab8.Enrollment.Common.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto> CreateEnrollment(EnrollmentDto enrollmentDto, string branchId);
    Task<EnrollmentDto> GetEnrollment(int id, string branchId);
    Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudent(int studentId, string branchId);
    Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourse(int courseId, string branchId);
    Task<bool> DeleteEnrollment(int id, string branchId);
    Task ProcessStudentEnrolledEvent(StudentEnrolledEvent enrollmentEvent);
}