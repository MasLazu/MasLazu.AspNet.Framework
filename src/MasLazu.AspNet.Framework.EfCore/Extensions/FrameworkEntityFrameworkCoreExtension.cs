using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Framework.EfCore.Repositories;

namespace MasLazu.AspNet.Framework.EfCore.Extensions;

public static class FrameworkEfCoreExtension
{
    public static IServiceCollection AddFrameworkEfCore(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, SharedTransactionUnitOfWork>();

        return services;
    }
}
