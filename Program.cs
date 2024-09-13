using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;
using Azure.Identity;
using Microsoft.Data.SqlClient;
var builder = WebApplication.CreateBuilder(args);

// Add database context and cache
if(builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("MyDbConnection")));
    builder.Services.AddDistributedMemoryCache();
}
else
{
    var credential = new DefaultAzureCredential();
    var connectionString = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
    var sqlConnection = new SqlConnection(connectionString);

    sqlConnection.AccessToken = credential.GetToken(
        new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" })).Token;

    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseSqlServer(sqlConnection));
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
        options.InstanceName = "SampleInstance";
    });
}


// Add services to the container.
builder.Services.AddControllersWithViews();

// Add App Service logging
builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
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
    pattern: "{controller=Todos}/{action=Index}/{id?}");

app.Run();
