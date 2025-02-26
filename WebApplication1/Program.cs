using WebApplication1.API_Models;
using WebApplication1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Konfigurerer API-innstillinger
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

// Konfigurerer HTTP-klienter
builder.Services.AddHttpClient<Kommunefinner>();

// Konfigurerer databasekonteksten
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(10, 5, 9)),
    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

// Legger til Identity-tjenester
builder.Services.AddIdentity<WebUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Legger til tjenester i containeren
builder.Services.AddControllersWithViews();

SetupAuthentication(builder);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "User", "Caseworker" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var userManager = serviceProvider.GetRequiredService<UserManager<WebUser>>();
    var caseworkerEmail = "Caseworker@test";
    var caseworkerPassword = "Test1";

    var existingUser = await userManager.FindByEmailAsync(caseworkerEmail);
    if (existingUser == null)
    {
        var caseworkerUser = new WebUser { UserName = caseworkerEmail, Email = caseworkerEmail };
        var result = await userManager.CreateAsync(caseworkerUser, caseworkerPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(caseworkerUser, "Caseworker");
        }
        else
        {
            // Håndtering av errors
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error creating user: {error.Description}");
            }
        }
    }
}

// Konfigurerer HTTP-request-pipelinen
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Legger til autentiseringsmiddleware
app.UseAuthentication(); // Påse at dette er med
app.UseAuthorization();
app.UseAntiforgery();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

void SetupAuthentication(WebApplicationBuilder builder)
{
    // Oppsett for autentisering
    builder.Services.Configure<IdentityOptions>(options =>
    {
        // Standardinnstilling for utlåsing (Lockout)
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        // Passordinnstillinger/-krav
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 5;
        options.Password.RequiredUniqueChars = 0;
    });
}