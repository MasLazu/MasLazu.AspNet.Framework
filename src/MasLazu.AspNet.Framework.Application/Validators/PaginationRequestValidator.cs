using FluentValidation;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Domain.Entities;
using System.Linq.Expressions;

namespace MasLazu.AspNet.Framework.Application.Validators;

public class PaginationRequestValidator<TEntity> : AbstractValidator<PaginationRequest<TEntity>>, IPaginationValidator<TEntity>
    where TEntity : BaseEntity
{
    public PaginationRequestValidator(IEntityPropertyMap<TEntity> propertyMap)
    {
        RuleFor(r => r.Page)
            .GreaterThan(0);

        RuleFor(r => r.PageSize)
            .InclusiveBetween(1, 200);

        RuleForEach(r => r.Filters)
            .Custom((filter, ctx) =>
            {
                if (string.IsNullOrWhiteSpace(filter.Field))
                {
                    ctx.AddFailure("Field", "Filter field is required");
                    return;
                }

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
            });

        RuleForEach(r => r.OrderBy)
            .Must(ob => !string.IsNullOrWhiteSpace(ob.Field))
            .WithMessage("OrderBy field is required");
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
