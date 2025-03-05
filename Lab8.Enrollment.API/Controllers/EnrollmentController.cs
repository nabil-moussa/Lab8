using Lab8.Enrollment.Common.Dtos;
using Lab8.Enrollment.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Lab8.Enrollment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IMemoryCache _cache;

    public EnrollmentController(IEnrollmentService enrollmentService, IMemoryCache cache)
    {
        _enrollmentService = enrollmentService;
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

    [HttpGet("protected")]
    public IActionResult Protected()
    {
        if (!_cache.TryGetValue("AuthToken", out string token))
        {
            return Unauthorized("Token not found.");
        }

        return Ok($"Token found: {token}");
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<EnrollmentDto>> CreateEnrollment(EnrollmentDto enrollmentDto)
    {
        string branchId = GetCurrentBranchId();
        var result = await _enrollmentService.CreateEnrollment(enrollmentDto, branchId);
        return CreatedAtAction(nameof(GetEnrollment), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "SameBranch")]
    public async Task<ActionResult<EnrollmentDto>> GetEnrollment(int id)
    {
        string branchId = GetCurrentBranchId();
        var enrollment = await _enrollmentService.GetEnrollment(id, branchId);
        
        if (enrollment == null)
            return NotFound();

        return enrollment;
    }

    [HttpGet("student/{studentId}")]
    [Authorize(Policy = "SameBranch")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsByStudent(int studentId)
    {
        string branchId = GetCurrentBranchId();
        return Ok(await _enrollmentService.GetEnrollmentsByStudent(studentId, branchId));
    }

    [HttpGet("course/{courseId}")]
    [Authorize(Policy = "SameBranch")]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetEnrollmentsByCourse(int courseId)
    {
        string branchId = GetCurrentBranchId();
        return Ok(await _enrollmentService.GetEnrollmentsByCourse(courseId, branchId));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult> DeleteEnrollment(int id)
    {
        string branchId = GetCurrentBranchId();
        var result = await _enrollmentService.DeleteEnrollment(id, branchId);
        
        if (!result)
            return NotFound();

        return NoContent();
    }
}