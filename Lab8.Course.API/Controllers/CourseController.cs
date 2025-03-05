using Lab8.Course.Common.Dtos;
using Lab8.Course.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Lab8.Course.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    private readonly IMemoryCache _cache;

    public CoursesController(ICourseService courseService, IMemoryCache cache)
    {
        _courseService = courseService;
        _cache = cache;
    }
    
    private string GetCurrentBranchId()
    {
        var branchIdClaim = User.FindFirst("BranchId");
        
        if (branchIdClaim != null)
        {
            return branchIdClaim.Value;
        }
        
        return "default";
    }  
    [HttpGet("claims")]
    [Authorize]
    public IActionResult GetClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }
    
    [HttpGet]
    [Authorize(Policy = "SameBranch")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCourses()
    {
        string branchId = GetCurrentBranchId();
        var courses = await _courseService.GetAllCoursesAsync(branchId);
        return Ok(courses);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "SameBranch")]
    public async Task<ActionResult<CourseDto>> GetCourse(int id)
    {
        string branchId = GetCurrentBranchId();
        var course = await _courseService.GetCourseByIdAsync(id, branchId);
        
        if (course == null)
            return NotFound();
            
        return Ok(course);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CourseDto>> CreateCourse(CourseDto courseDto)
    {
        string branchId = GetCurrentBranchId();
        var createdCourse = await _courseService.CreateCourseAsync(courseDto, branchId);
        return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.Id }, createdCourse);
    }
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        if (!_cache.TryGetValue("AuthToken", out string token))
        {
            return Unauthorized("Token not found.");
        }

        return Ok($"Token found: {token}");
    }
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateCourse(int id, CourseDto courseDto)
    {
        if (id != courseDto.Id)
            return BadRequest();
            
        string branchId = GetCurrentBranchId();
        var updatedCourse = await _courseService.UpdateCourseAsync(courseDto, branchId);
        
        if (updatedCourse == null)
            return NotFound();
            
        return NoContent();
    }

    
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        string branchId = GetCurrentBranchId();
        await _courseService.DeleteCourseAsync(id, branchId);
        return NoContent();
    }
}