using FluentValidation;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.Application.Interfaces;

public interface IPaginationValidator<TEntity> : IValidator<PaginationRequest<TEntity>> where TEntity : BaseEntity
{
}
