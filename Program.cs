using ExpenseSplitter.Interfaces;
using ExpenseSplitter.Models.Entities;
using ExpenseSplitter.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure MySQL DbContext with EF Core
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Port=3306;Database=ExpenseSplitterDb;User=root;Password=;";

            builder.Services.AddDbContext<ExpenseSplitterDbContext>(options =>
            {
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 44)),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
            });

            // Register Dependency Injection for Repositories & Services
            builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
            builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
            builder.Services.AddScoped<ISettlementService, SettlementService>();

            var app = builder.Build();

            // Auto-create database & tables on startup if not already created
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var db = services.GetRequiredService<ExpenseSplitterDbContext>();
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Could not automatically ensure database creation. Please check MySQL server and appsettings.json connection credentials.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
