using Microsoft.EntityFrameworkCore;
using EHRsystem.Data;
using EHRsystem.Models.Entities;

namespace EHRsystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSession();

            // Register ApplicationDbContext using MySQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 32))
                ));

            var app = builder.Build();

            // === SEED TEST DOCTOR ACCOUNT ===
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (!db.Doctors.Any())
                {
                    var doctor = new Doctor
                    {
                        Name = "Dr. Sarah",
                        Email = "doctor@example.com",
                        PasswordHash = "123",
                        Role = "Doctor",
                        Specialization = "Cardiology",
                        Specialty = "Heart",
                        Location = "Cairo"
                    };

                    db.Users.Add(doctor); // inserts with Role="Doctor" (TPH)
                    db.SaveChanges();
                }
            }

            // Configure middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseSession();       // ✅ Enables HttpContext.Session
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
