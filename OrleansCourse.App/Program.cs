using OrleansCourse.App.Areas.Identity;
using OrleansCourse.App.Data;
using OrleansCourse.App.Services;
using OrleansCourse.App.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrleansCourse.App.Models.Profiles;

var builder = WebApplication.CreateBuilder(args);
var orleansClusterConnectionString = builder.Configuration.GetConnectionString("OrleansCluster") 
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Host
    .UseOrleansClient(client =>
    {
        client
            .UseAdoNetClustering((options =>
            {
                options.ConnectionString = orleansClusterConnectionString;
                options.Invariant = "Npgsql";
            }));
    })
    .ConfigureLogging(logging => logging.AddConsole());
builder.Services.AddAutoMapper(typeof(MappingProfile));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();
builder.Services.AddScoped<ComponentStateChangedObserver>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<IMLRecommendationService, MLRecommendationService>();
builder.Services.AddScoped<IGradePredictionService, GradePredictionService>();
builder.Services.AddScoped<IAtRiskIdentificationService, AtRiskIdentificationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
