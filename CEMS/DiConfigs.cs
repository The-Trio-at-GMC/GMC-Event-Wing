using CEMS.Data;
using CEMS.Services;
using Microsoft.EntityFrameworkCore;

namespace CEMS
{
    //ASP.NET creating and providing objects automatically
    public static class DiConfigs
    {
        //IServiceCollection is the built-in container where all services are registered
        //(this IServiceCollection service) makes ConfigureServices an EXTENSION METHOD, hence builder.Services.ConfigureServices();
        public static void ConfigureServices(this IServiceCollection service)
        {
            //AddScoped = one instance per HTTP request
            service.AddScoped<DbContext, ApplicationDbContext>();
            
            //For password reset link via email
            service.AddScoped<EmailService>();
        }
    }
}