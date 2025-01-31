using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Projekt_zaliczeniowy.Data;
using Projekt_zaliczeniowy.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
	options.SignIn.RequireConfirmedAccount = false;
	// Configure password requirements
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;
	options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add Identity UI services
builder.Services.AddRazorPages();
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Add role-based authorization policies
builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrator"));
	options.AddPolicy("RequireTutorRole", policy => policy.RequireRole("Korepetytor"));
	options.AddPolicy("RequireUserRole", policy => policy.RequireRole("Uzytkownik"));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
	var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
	var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

	var roles = new[] { "Administrator", "Korepetytor", "Uzytkownik" };

	foreach (var role in roles)
	{
		if (!await roleManager.RoleExistsAsync(role))
			await roleManager.CreateAsync(new IdentityRole(role));
	}
	string[,] newUser = new string[3, 3];
	newUser[0, 0] = "admin@admin.com";
	newUser[0, 1] = "Admin1234!";
	newUser[0, 2] = "Administrator";


	newUser[1, 0] = "TestT@test.com";
	newUser[1, 1] = "Test1234!";
	newUser[1, 2] = "Korepetytor";


	newUser[2, 0] = "TestU@test.com";
	newUser[2, 1] = "Test1234!";
	newUser[2, 2] = "Uzytkownik";


	for (int i = 0; i < newUser.GetLength(0); i++)
	{
		if (await userManager.FindByEmailAsync(newUser[i, 0]) == null)
		{
			var newUserInject = new ApplicationUser
			{
				UserName = newUser[i, 0],
				Email = newUser[i, 0],
				FirstName = newUser[i, 2],
				LastName = "Testowski",
				UserType = roles[i] == "Korepetytor" ? UserType.Korepetytor :
						  roles[i] == "Administrator" ? UserType.Administrator :
						  UserType.Uzytkownik
			};

			var result = await userManager.CreateAsync(newUserInject, newUser[i, 1]);
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(newUserInject, newUser[i, 2]);
			}
			else
			{
				var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
				foreach (var error in result.Errors)
				{
					logger.LogError($"Nie uda³o siê utworzyæ konta: {error.Description}");
				}
			}
		}
	}

	app.Run();
}
