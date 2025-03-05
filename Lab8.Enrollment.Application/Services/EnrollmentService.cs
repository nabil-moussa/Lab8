using Lab8.Enrollment.Common.Dtos;
using Lab8.Enrollment.Common.Interfaces;
using Lab8.Enrollment.Common.RabbitMQ;
using Lab8.Enrollment.Infrastructure.RabbitMQ;
using Lab8.Enrollment.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab8.Enrollment.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly EnrollmentDbContext _context;
    private readonly RabbitMQPublisher _publisher;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        EnrollmentDbContext context, 
        RabbitMQPublisher publisher,
        ILogger<EnrollmentService> logger)
    {
        _context = context;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<EnrollmentDto> CreateEnrollment(EnrollmentDto enrollmentDto, string branchId)
    {
        _context.SetBranchId(branchId);

        var enrollment = new Domain.Models.Enrollment
        {
            StudentId = enrollmentDto.StudentId,
            CourseId = enrollmentDto.CourseId,
            Grade = enrollmentDto.Grade ?? "N/A",
            BranchId = branchId,
            EnrollmentDate = enrollmentDto.EnrollmentDate
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        _publisher.PublishEnrollmentCreated(new EnrollmentCreatedEvent
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            Grade = enrollment.Grade,
            BranchId = branchId,
            EnrollmentDate = enrollment.EnrollmentDate,
            OriginService = "Lab8.Enrollment"
        });

        return new EnrollmentDto
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            Grade = enrollment.Grade,
            BranchId = branchId,
            EnrollmentDate = enrollment.EnrollmentDate
        };
    }

    public async Task<EnrollmentDto> GetEnrollment(int id, string branchId)
    {
        _context.SetBranchId(branchId);
        
        var enrollment = await _context.Enrollments.FindAsync(id);
        
        if (enrollment == null)
        {
            enrollment = await _context.Enrollments
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        if (enrollment == null)
            return null;

        return new EnrollmentDto
        {
            Id = enrollment.Id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            Grade = enrollment.Grade,
            BranchId = enrollment.BranchId,
            EnrollmentDate = enrollment.EnrollmentDate
        };
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByStudent(int studentId, string branchId)
    {
        _context.SetBranchId(branchId);

        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                Grade = e.Grade,
                BranchId = e.BranchId,
                EnrollmentDate = e.EnrollmentDate
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<EnrollmentDto>> GetEnrollmentsByCourse(int courseId, string branchId)
    {
        _context.SetBranchId(branchId);

        return await _context.Enrollments
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                Grade = e.Grade,
                BranchId = e.BranchId,
                EnrollmentDate = e.EnrollmentDate
            })
            .ToListAsync();
    }

    public async Task<bool> DeleteEnrollment(int id, string branchId)
    {
        _context.SetBranchId(branchId);
        
        var enrollment = await _context.Enrollments.FindAsync(id);
        
        if (enrollment == null)
            return false;

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();

        // Optionally publish an enrollment deleted event
        _publisher.PublishEnrollmentDeleted(new EnrollmentDeletedEvent
        {
            Id = id,
            StudentId = enrollment.StudentId,
            CourseId = enrollment.CourseId,
            BranchId = branchId,
            OriginService = "Lab8.Enrollment"
        });

        return true;
    }

    public async Task ProcessStudentEnrolledEvent(StudentEnrolledEvent enrollmentEvent)
    {
        _logger.LogInformation($"Processing student enrollment event: Student {enrollmentEvent.StudentId} enrolling in Course {enrollmentEvent.CourseId}");
        
        _context.SetBranchId(enrollmentEvent.BranchId);

        var enrollment = new Domain.Models.Enrollment
        {
            StudentId = enrollmentEvent.StudentId,
            CourseId = enrollmentEvent.CourseId,
            BranchId = enrollmentEvent.BranchId,
            EnrollmentDate = enrollmentEvent.EnrollmentDate,
            Grade = "N/A" // Default grade for new enrollments
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Successfully enrolled student {enrollmentEvent.StudentId} in course {enrollmentEvent.CourseId}");
    }
}