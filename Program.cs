using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DailyPlanner.Data;
using DailyPlanner.Services;

namespace DailyPlanner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Bağlantı
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // SADECE BU OLSUN
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages(); // Identity için zorunlu
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<GptService>();
            builder.Services.AddHttpClient(); // HttpClient kullanılacak
            builder.Services.AddScoped<GptService>(); // GptService bağımlılığı


            var app = builder.Build();

            // middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // login işlemleri için zorunlu
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages(); // Identity UI sayfaları için

            app.Run();




        }
    }
}
