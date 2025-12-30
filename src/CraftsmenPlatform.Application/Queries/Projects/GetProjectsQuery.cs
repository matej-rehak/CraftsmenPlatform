using CraftsmenPlatform.Application.Common.Specifications;
using CraftsmenPlatform.Application.DTOs.Project;
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Repositories;
using CraftsmenPlatform.Domain.ValueObjects;
using MediatR;
using CraftsmenPlatform.Domain.Enums;

namespace CraftsmenPlatform.Application.Queries.Projects;

/// <summary>
/// Query for getting filtered and paginated projects
/// </summary>
public record GetProjectsQuery : IRequest<Result<PagedResult<ProjectResponse>>>
{
    // Filtering
    public ProjectStatus? Status { get; init; }
    public Guid? CustomerId { get; init; }
    public string? SearchTerm { get; init; }
    public decimal? MinBudget { get; init; }
    public decimal? MaxBudget { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public DateTime? CreatedAfter { get; init; }
    public DateTime? CreatedBefore { get; init; }

    // Pagination
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    // Sorting
    public string? OrderBy { get; init; }
    public bool IsDescending { get; init; }
}

/// <summary>
/// Handler for GetProjectsQuery
/// </summary>
public class GetProjectsQueryHandler 
    : IRequestHandler<GetProjectsQuery, Result<PagedResult<ProjectResponse>>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<PagedResult<ProjectResponse>>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Create pagination params
            var pagination = new PaginationParams(request.PageNumber, request.PageSize);

            // Create sorting params
            var sorting = new SortingParams
            {
                OrderBy = request.OrderBy,
                IsDescending = request.IsDescending
            };

            // Create specification with all filters
            // Convert budget decimals to Money if provided
            Money? minBudgetMoney = null;
            if (request.MinBudget.HasValue)
            {
                var moneyResult = Money.Create(request.MinBudget.Value, "CZK");
                if (moneyResult.IsSuccess)
                    minBudgetMoney = moneyResult.Value;
            }

            Money? maxBudgetMoney = null;
            if (request.MaxBudget.HasValue)
            {
                var moneyResult = Money.Create(request.MaxBudget.Value, "CZK");
                if (moneyResult.IsSuccess)
                    maxBudgetMoney = moneyResult.Value;
            }

            var specification = new ProjectFilterSpecification(
                status: request.Status,
                customerId: request.CustomerId,
                searchTerm: request.SearchTerm,
                minBudget: minBudgetMoney,
                maxBudget: maxBudgetMoney,
                city: request.City,
                state: request.State,
                createdAfter: request.CreatedAfter,
                createdBefore: request.CreatedBefore,
                pagination: pagination,
                sorting: sorting
            );

            // Execute query
            var pagedProjects = await _projectRepository.GetPagedAsync(
                specification,
                cancellationToken);

            // Map to DTOs
            var pagedDtos = pagedProjects.Map(project => new ProjectResponse
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                BudgetMin = project.BudgetMin,
                BudgetMax = project.BudgetMax,
                PreferredStartDate = project.PreferredStartDate,
                Deadline = project.Deadline,
                Status = project.Status,
                PublishedAt = project.PublishedAt,
                CompletedAt = project.CompletedAt,
                Images = project.Images.ToList(),
                HasAcceptedOffer = project.Offers.Any(o => o.Status == Domain.Enums.OfferStatus.Accepted)
            });

            return Result<PagedResult<ProjectResponse>>.Success(pagedDtos);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ProjectResponse>>.Failure(
                $"Failed to retrieve projects: {ex.Message}");
        }
    }
}
