using FluentValidation;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Domain.Entities;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MasLazu.AspNet.Framework.Application.Validators;

public class CursorPaginationRequestValidator<TEntity> : AbstractValidator<CursorPaginationRequest>, ICursorPaginationValidator<TEntity>
    where TEntity : BaseEntity
{
    private readonly IConfiguration _configuration;

    public CursorPaginationRequestValidator(IEntityPropertyMap<TEntity> propertyMap, IConfiguration configuration)
    {
        _configuration = configuration;

        RuleFor(r => r.Limit)
            .InclusiveBetween(1, int.TryParse(_configuration["Pagination:MaxPageSize"], out int max) ? max : 200);

        RuleFor(r => r.Cursor)
            .Must(cursor => cursor == null || cursor != Guid.Empty)
            .WithMessage("Cursor must be null or a non-empty GUID");

        RuleForEach(r => r.Filters)
            .Custom((filter, ctx) =>
            {
                if (string.IsNullOrWhiteSpace(filter.Field))
                {
                    ctx.AddFailure("Field", "Filter field is required");
                    return;
                }

                try
                {
                    Expression<Func<TEntity, object>> expr = propertyMap.Get(filter.Field);
                    Expression body = expr.Body;
                    if (body.NodeType == ExpressionType.Convert)
                    {
                        body = ((UnaryExpression)body).Operand;
                    }

                    Type propertyType = Nullable.GetUnderlyingType(body.Type) ?? body.Type;

                    if (!IsOperatorValidForType(filter.Operator, propertyType))
                    {
                        ctx.AddFailure($"Operator '{filter.Operator}' is not valid for field '{filter.Field}' of type {propertyType.Name}");
                    }
                }
                catch (KeyNotFoundException)
                {
                    ctx.AddFailure($"Field '{filter.Field}' is not available for filtering on entity {typeof(TEntity).Name}");
                }
            });

        RuleForEach(r => r.OrderBy)
            .Custom((orderBy, ctx) =>
            {
                if (string.IsNullOrWhiteSpace(orderBy.Field))
                {
                    ctx.AddFailure("Field", "OrderBy field is required");
                    return;
                }

                try
                {
                    Expression<Func<TEntity, object>> expr = propertyMap.Get(orderBy.Field);
                }
                catch (KeyNotFoundException)
                {
                    ctx.AddFailure($"Field '{orderBy.Field}' is not available for ordering on entity {typeof(TEntity).Name}");
                }
            });
    }

    private bool IsOperatorValidForType(string? op, Type type)
    {
        if (string.IsNullOrWhiteSpace(op))
        {
            return false;
        }

        op = op.Trim().ToLowerInvariant();

        if (type == typeof(string))
        {
            return op is "=" or "!=" or "contains" or "startswith" or "endswith";
        }

        if (type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
            type == typeof(int) || type == typeof(long) ||
            type == typeof(decimal) || type == typeof(double) ||
            type == typeof(float))
        {
            return op is "=" or "!=" or ">" or "<" or ">=" or "<=";
        }

        if (type == typeof(bool))
        {
            return op is "=" or "!=";
        }

        if (type.IsEnum || type == typeof(Guid))
        {
            return op is "=" or "!=";
        }

        return false;
    }
}
