using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Framework.Application.Extensions;
using MasLazu.AspNet.Framework.Domain.Entities;

namespace MasLazu.AspNet.Framework.Application.Extensions;

public static class FrameworkApplicationExtension
{
    public static IServiceCollection AddFrameworkApplication(this IServiceCollection services)
    {
        services.AddFrameworkApplicationValidators();

        return services;
    }
}