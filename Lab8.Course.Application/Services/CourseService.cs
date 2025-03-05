using Lab8.Course.Common.Dtos;
using Lab8.Course.Common.Interfaces;
using Lab8.Course.Common.RabbitMQ;
using Lab8.Course.Infrastructure.RabbitMQ;
using Lab8.Course.Persistence.Context;
using Microsoft.EntityFrameworkCore;


namespace Lab8.Course.Application.Services;

public class CourseService : ICourseService
{
    private readonly CourseDbContext _context;
    private readonly RabbitMQPublisher _publisher;

    public CourseService(CourseDbContext context, RabbitMQPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync(string branchId)
    {
        _context.SetBranchId(branchId);
        
        var courses = await _context.Courses.ToListAsync();
        return courses.Select(c => new CourseDto
        {
            Id = c.Id,
            Name = c.Name,
            MaxStudents = c.MaxStudents,
            EnrollmentStartDate = c.EnrollmentStartDate,
            EnrollmentEndDate = c.EnrollmentEndDate
        });
    }

    public async Task<CourseDto> GetCourseByIdAsync(int id, string branchId)
    {
        _context.SetBranchId("default");
        
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return null;
        

        if (course == null)
        {
            course = await _context.Courses
                .AsNoTracking() 
                .IgnoreQueryFilters() 
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            MaxStudents = course.MaxStudents,
            EnrollmentStartDate = course.EnrollmentStartDate,
            EnrollmentEndDate = course.EnrollmentEndDate
        };
    }

    public async Task<CourseDto> CreateCourseAsync(CourseDto courseDto, string branchId)
    {
        _context.SetBranchId(branchId);

        var course = new Domain.Models.Course(courseDto.Name, courseDto.MaxStudents, courseDto.EnrollmentStartDate,
            courseDto.EnrollmentEndDate);

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        _publisher.PublishCourseCreated(new CourseCreatedEvent
        {
            Id = course.Id,
            Name = course.Name,
            MaxStudents = course.MaxStudents,
            EnrollmentStartDate = course.EnrollmentStartDate,
            EnrollmentEndDate = course.EnrollmentEndDate,
            BranchId = branchId,
            OriginService = "Lab8.Course" 

        });

        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            MaxStudents = course.MaxStudents,
            EnrollmentStartDate = course.EnrollmentStartDate,
            EnrollmentEndDate = course.EnrollmentEndDate
        };
    }

    public async Task<CourseDto> UpdateCourseAsync(CourseDto courseDto, string branchId)
    {
        _context.SetBranchId(branchId);
        
        var course = await _context.Courses.FindAsync(courseDto.Id);
        if (course == null)
            return null;

        course.Name = courseDto.Name;
        course.MaxStudents = courseDto.MaxStudents;
        course.EnrollmentStartDate = courseDto.EnrollmentStartDate;
        course.EnrollmentEndDate = courseDto.EnrollmentEndDate;

        await _context.SaveChangesAsync();

        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            MaxStudents = course.MaxStudents,
            EnrollmentStartDate = course.EnrollmentStartDate,
            EnrollmentEndDate = course.EnrollmentEndDate
        };
    }

    public async Task DeleteCourseAsync(int id, string branchId)
    {
        _context.SetBranchId(branchId);
        
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }
    }
}


