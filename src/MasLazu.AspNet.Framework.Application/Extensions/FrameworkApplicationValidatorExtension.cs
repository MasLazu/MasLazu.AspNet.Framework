using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MasLazu.AspNet.Framework.Application.Validators;
using MasLazu.AspNet.Framework.Application.Models;
using MasLazu.AspNet.Framework.Domain.Entities;
using MasLazu.AspNet.Framework.Application.Interfaces;

namespace MasLazu.AspNet.Framework.Application.Extensions;

public static class FrameworkApplicationValidatorExtension
{
    public static IServiceCollection AddFrameworkApplicationValidators(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPaginationValidator<>), typeof(PaginationRequestValidator<>));

        return services;
    }
}
