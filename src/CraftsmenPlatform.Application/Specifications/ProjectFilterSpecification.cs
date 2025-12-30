
using CraftsmenPlatform.Domain.Common;
using CraftsmenPlatform.Domain.Entities;
using CraftsmenPlatform.Domain.Enums;
using System.Linq.Expressions;
using CraftsmenPlatform.Domain.ValueObjects;

namespace CraftsmenPlatform.Application.Common.Specifications;

/// <summary>
/// Specification for filtering projects with all possible criteria
/// </summary>
public class ProjectFilterSpecification : Specification<Project>
{
    public ProjectFilterSpecification(
        ProjectStatus? status = null,
        Guid? customerId = null,
        string? searchTerm = null,
        Money? minBudget = null,
        Money? maxBudget = null,
        string? city = null,
        string? state = null,
        DateTime? createdAfter = null,
        DateTime? createdBefore = null,
        PaginationParams? pagination = null,
        SortingParams? sorting = null)
    {
        // Build criteria using expression builder
        Expression<Func<Project, bool>>? criteria = null;

        if (status.HasValue)
            criteria = CombineWithAnd(criteria, p => p.Status == status.Value);

        if (customerId.HasValue)
            criteria = CombineWithAnd(criteria, p => p.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            criteria = CombineWithAnd(criteria, p =>
                p.Title.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search));
        }

        if (minBudget != null)
            criteria = CombineWithAnd(criteria,
                p => p.BudgetMax != null && p.BudgetMax.Amount >= minBudget.Amount);

        if (maxBudget != null)
            criteria = CombineWithAnd(criteria,
                p => p.BudgetMin != null && p.BudgetMin.Amount <= maxBudget.Amount);

        if (createdAfter.HasValue)
            criteria = CombineWithAnd(criteria, p => p.CreatedAt >= createdAfter.Value);

        if (createdBefore.HasValue)
            criteria = CombineWithAnd(criteria, p => p.CreatedAt <= createdBefore.Value);

        Criteria = criteria;

        // Includes for eager loading
        AddInclude(p => p.Offers);
        AddInclude(p => p.Images);

        // Sorting
        ApplySorting(sorting);

        // Pagination
        if (pagination != null)
        {
            ApplyPaging(pagination.Skip, pagination.PageSize);
        }
    }

    private void ApplySorting(SortingParams? sorting)
    {
        if (sorting?.OrderBy == null)
        {
            // Default sorting
            ApplyOrderByDescending(p => p.CreatedAt);
            return;
        }

        var orderBy = sorting.OrderBy.ToLower();
        var isDesc = sorting.IsDescending;

        // Map string to expression
        Expression<Func<Project, object>>? orderExpression = orderBy switch
        {
            "title" => p => p.Title,
            "createdat" => p => p.CreatedAt,
            "budgetmin" => p => p.BudgetMin != null ? p.BudgetMin.Amount : 0,
            "budgetmax" => p => p.BudgetMax != null ? p.BudgetMax.Amount : 0,
            "status" => p => p.Status,
            _ => p => p.CreatedAt // fallback
        };

        if (isDesc)
            ApplyOrderByDescending(orderExpression);
        else
            ApplyOrderBy(orderExpression);
    }

    private static Expression<Func<Project, bool>>? CombineWithAnd(
        Expression<Func<Project, bool>>? first,
        Expression<Func<Project, bool>> second)
    {
        if (first == null)
            return second;

        var parameter = Expression.Parameter(typeof(Project));

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<Project, bool>>(
            Expression.AndAlso(left!, right!), parameter);
    }
}

/// <summary>
/// Helper for combining expressions
/// </summary>
internal class ReplaceExpressionVisitor : ExpressionVisitor
{
    private readonly Expression _oldValue;
    private readonly Expression _newValue;

    public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    {
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override Expression? Visit(Expression? node)
    {
        return node == _oldValue ? _newValue : base.Visit(node);
    }
}