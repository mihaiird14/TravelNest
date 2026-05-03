using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using QuestPDF.Infrastructure;
using SendGrid.Helpers.Mail;
using System.Globalization;
using TravelNest.Data;
using TravelNest.Hubs;
using TravelNest.Models;
using TravelNest.Services;
using Neo4j.Driver;


var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailSender, MailSend>();
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
.AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
//api ptr zboruri + cazare
builder.Services.Configure<AmadeusSettings>(builder.Configuration.GetSection("Amadeus"));
builder.Services.AddHttpClient<FlightService>();
builder.Services.AddHttpClient<HotelService>();
builder.Services.AddHttpClient<RecomandariForYou>();
//add api gemini +  serviciu ptr functii
builder.Services.AddScoped<GeminiService>();
//Adaugare serviciu Python
builder.Services.AddHttpClient<PythonFaceService>();
builder.Services.AddScoped<CalculFaceRec>();
builder.Services.AddHostedService<TravelNest.Services.PythonRunnerService>();
//serviciu  unsplash ptr poze travel assistant
builder.Services.AddScoped<PostariAssistant>();
builder.Services.AddHttpClient<PostariAssistant>();
//conexiune driver neo4j + populare bd
var neo4jConfig = builder.Configuration.GetSection("Neo4j");
builder.Services.AddSingleton(GraphDatabase.Driver(
    neo4jConfig["Uri"],
    AuthTokens.Basic(neo4jConfig["User"], neo4jConfig["Password"])
));
builder.Services.AddScoped<GrafInterfata, GraphService>();
builder.Services.AddSignalR(options => {
    options.EnableDetailedErrors = true; // Permite trimiterea mesajelor de eroare cãtre client
});
var app = builder.Build();
//seed graf db
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var grafService = services.GetRequiredService<GrafInterfata>();
        await grafService.InitializareGraf();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
}
//signalR ptr notificari + chat
app.MapHub<NotificariHub>("/NotificariHub");
app.MapHub<ChatHub>("/ChatHub");
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
