using CraftsmenPlatform.Application.DTOs.Project;
using CraftsmenPlatform.Application.Queries.Projects;
using CraftsmenPlatform.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CraftsmenPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("per-user")]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get projects with filtering, pagination and sorting
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/projects?status=Published&amp;city=Prague&amp;pageNumber=1&amp;pageSize=20&amp;orderBy=createdAt:desc
    /// 
    /// Available filters:
    /// - status: Draft, Published, InProgress, Completed, Cancelled
    /// - searchTerm: Search in title and description
    /// - minBudget, maxBudget: Budget range
    /// - city, state: Location filters
    /// - createdAfter, createdBefore: Date range
    /// 
    /// Sorting:
    /// - orderBy: title, createdAt, budgetMin, budgetMax, status
    /// - Format: "fieldName:asc" or "fieldName:desc"
    /// - Default: createdAt:desc
    /// </remarks>
    [HttpGet]
    [DisableRateLimiting] // Public read - only global rate limit
    [ProducesResponseType(typeof(PagedResponseDto<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProjects(
        [FromQuery] GetProjectsRequest request)
    {
        // Parse sorting from query string
        var sorting = SortingParams.Parse(request.Sort);

        var query = new GetProjectsQuery
        {
            Status = request.Status,
            CustomerId = request.CustomerId,
            SearchTerm = request.Search,
            MinBudget = request.MinBudget,
            MaxBudget = request.MaxBudget,
            City = request.City,
            State = request.State,
            CreatedAfter = request.CreatedAfter,
            CreatedBefore = request.CreatedBefore,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            OrderBy = sorting.OrderBy,
            IsDescending = sorting.IsDescending
        };

        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }

        // Map to API response format
        var response = new PagedResponseDto<ProjectResponse>
        {
            Data = result.Value.Items,
            Pagination = new PaginationMetadataDto
            {
                CurrentPage = result.Value.PageNumber,
                PageSize = result.Value.PageSize,
                TotalCount = result.Value.TotalCount,
                TotalPages = result.Value.TotalPages,
                HasPreviousPage = result.Value.HasPreviousPage,
                HasNextPage = result.Value.HasNextPage
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Get my projects (authenticated user)
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponseDto<ProjectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProjects(
        [FromQuery] GetProjectsRequest request)
    {
        var userId = User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        // Create new request with CustomerId set
        var myProjectsRequest = new GetProjectsRequest
        {
            Status = request.Status,
            CustomerId = userGuid,
            Search = request.Search,
            MinBudget = request.MinBudget,
            MaxBudget = request.MaxBudget,
            City = request.City,
            State = request.State,
            CreatedAfter = request.CreatedAfter,
            CreatedBefore = request.CreatedBefore,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Sort = request.Sort
        };
        
        return await GetProjects(myProjectsRequest);
    }
}
