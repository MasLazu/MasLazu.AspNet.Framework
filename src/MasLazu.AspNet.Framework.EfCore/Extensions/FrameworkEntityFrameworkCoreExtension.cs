using Microsoft.Extensions.DependencyInjection;
using MasLazu.AspNet.Framework.Application.Interfaces;
using MasLazu.AspNet.Framework.EfCore.Data;
using MasLazu.AspNet.Framework.EfCore.Repositories;

namespace MasLazu.AspNet.Framework.EfCore.Extensions;

public static class FrameworkEntityFrameworkCoreExtension
{
    public static IServiceCollection AddFrameworkEntityFrameworkCore(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, SharedTransactionUnitOfWork>();

        return services;
    }
}
