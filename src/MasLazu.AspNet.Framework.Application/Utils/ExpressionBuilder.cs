using System.Globalization;
using System.Linq.Expressions;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.Application.Utils;

public class ExpressionBuilder<TEntity> where TEntity : BaseEntity
{
    private readonly IEntityPropertyMap<TEntity> _propertyMap;

    public ExpressionBuilder(IEntityPropertyMap<TEntity> propertyMap)
    {
        _propertyMap = propertyMap;
    }

    public IQueryable<TEntity> ApplyFilters(IQueryable<TEntity> query, List<Filter> filters)
    {
        foreach (Filter filter in filters)
        {
            Expression<Func<TEntity, object>> expr = _propertyMap.Get(filter.Field);
            ParameterExpression param = expr.Parameters.Single();
            Expression body = expr.Body;

            if (body.NodeType == ExpressionType.Convert)
            {
                body = ((UnaryExpression)body).Operand;
            }

            Type propertyType = body.Type;
            Type underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            object parsedValue = ParseStringToType(filter.Value, underlyingType);

            Expression constant = Expression.Constant(parsedValue, underlyingType);
            if (underlyingType != propertyType)
            {
                constant = Expression.Convert(constant, propertyType);
            }

            bool isString = underlyingType == typeof(string);
            Expression comparison = filter.Operator.ToLowerInvariant() switch
            {
                "=" => Expression.Equal(body, constant),
                "!=" => Expression.NotEqual(body, constant),
                ">" => Expression.GreaterThan(body, constant),
                "<" => Expression.LessThan(body, constant),
                ">=" => Expression.GreaterThanOrEqual(body, constant),
                "<=" => Expression.LessThanOrEqual(body, constant),
                "contains" when isString => BuildStringCall(body, filter.Value, nameof(string.Contains)),
                "startswith" when isString => BuildStringCall(body, filter.Value, nameof(string.StartsWith)),
                "endswith" when isString => BuildStringCall(body, filter.Value, nameof(string.EndsWith)),
                _ => throw new ArgumentException($"Unsupported operator: {filter.Operator}")
            };

            var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, param);
            query = query.Where(lambda);
        }

        return query;
    }

    private static Expression BuildStringCall(Expression body, string value, string methodName)
    {
        Expression nonNullBody = Expression.Coalesce(body, Expression.Constant(string.Empty));
        return Expression.Call(nonNullBody, methodName, Type.EmptyTypes, Expression.Constant(value));
    }

    private static object ParseStringToType(string value, Type targetType)
    {
        if (targetType == typeof(string))
        {
            return value;
        }

        if (targetType == typeof(Guid))
        {
            return Guid.Parse(value);
        }

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, value, ignoreCase: true);
        }

        if (targetType == typeof(DateTimeOffset))
        {
            return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal);
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal);
        }

        if (targetType == typeof(bool))
        {
            return bool.Parse(value);
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    public IQueryable<TEntity> ApplyOrdering(IQueryable<TEntity> query, List<OrderBy> orderBy)
    {
        IOrderedQueryable<TEntity>? orderedQuery = null;

        for (int i = 0; i < orderBy.Count; i++)
        {
            OrderBy ord = orderBy[i];
            Expression<Func<TEntity, object>> expr = _propertyMap.Get(ord.Field);

            orderedQuery = i == 0
                ? ord.Desc
                    ? query.OrderByDescending(expr)
                    : query.OrderBy(expr)
                : ord.Desc
                    ? orderedQuery!.ThenByDescending(expr)
                    : orderedQuery!.ThenBy(expr);
        }

        return orderedQuery ?? query;
    }

    public IQueryable<TEntity> ApplyPagination(IQueryable<TEntity> query, PaginationRequest request)
    {
        query = ApplyFilters(query, request.Filters);
        query = ApplyOrdering(query, request.OrderBy);

        return query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize);
    }

    public IQueryable<TEntity> ApplyCursor(IQueryable<TEntity> query, CursorPaginationRequest request)
    {
        if (request.Cursor == null || request.Cursor == Guid.Empty)
        {
            return query;
        }

        ParameterExpression param = Expression.Parameter(typeof(TEntity), "e");
        MemberExpression idProp = Expression.PropertyOrField(param, "Id");
        ConstantExpression constant = Expression.Constant(request.Cursor.Value);
        BinaryExpression comparison = Expression.GreaterThan(idProp, constant);

        var lambda = Expression.Lambda<Func<TEntity, bool>>(comparison, param);
        return query.Where(lambda);
    }
}
