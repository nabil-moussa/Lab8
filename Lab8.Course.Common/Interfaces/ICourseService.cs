using Lab8.Course.Common.Dtos;

namespace Lab8.Course.Common.Interfaces;

public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllCoursesAsync(string branchId);
    Task<CourseDto> GetCourseByIdAsync(int id, string branchId);
    Task<CourseDto> CreateCourseAsync(CourseDto courseDto, string branchId);
    Task<CourseDto> UpdateCourseAsync(CourseDto courseDto, string branchId);
    Task DeleteCourseAsync(int id, string branchId);
}