using CEMS.Data;
using Microsoft.EntityFrameworkCore;

namespace CEMS
{

    public static class DiConfigs
    {
        public static void ConfigureServices(this IServiceCollection service)
        {
            service.AddScoped<DbContext, ApplicationDbContext>();
        }
    }
}