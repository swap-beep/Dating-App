using System;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationsServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllers();
        services.AddCors();

        services.AddScoped<IToken, TokenService>();
        // Register DataContext for dependency injection
        services.AddDbContext<DataContext>(opt =>
        {
            opt.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IUserRepository, UserRepository>();
        
        services.AddScoped<IPhotoService,PhotoService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        return services;
    }
}
