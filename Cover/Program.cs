using System.Reflection;
using System.Text.Json.Serialization;
using Covers.Auditing;
using Covers.Auditing.Interfaces;
using Covers.Services;
using Covers.Services.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}
);

builder.Services.AddSingleton(
    InitializeCosmosClientInstanceAsync(builder.Configuration.GetSection("CosmosDb")).GetAwaiter().GetResult());

builder.Services.AddDbContext<AuditContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IAuditContext, AuditContext>();
builder.Services.AddScoped<ICoverService, CoverService>(sp => {
    var databaseName = builder.Configuration.GetValue<string>("CosmosDb:DatabaseName");
    if (databaseName == null)
    {
        throw new InvalidOperationException("Database name is not configured.");
    }
    string? containerName = builder.Configuration.GetValue<string>("CosmosDb:ContainerName");
    if (containerName == null)
    {
        throw new InvalidOperationException("Container name is not configured.");
    }
    var cosmosClient = sp.GetRequiredService<CosmosClient>();
    var auditContext = sp.GetRequiredService<IAuditContext>();
    return new CoverService(cosmosClient, databaseName, containerName, auditContext);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();
    context.Database.Migrate();
}

app.Run();

static async Task<CosmosClient> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
{
    string? databaseName = configurationSection.GetSection("DatabaseName").Value;
    string? containerName = configurationSection.GetSection("ContainerName").Value;
    string? account = configurationSection.GetSection("Account").Value;
    string? key = configurationSection.GetSection("Key").Value;
    if (string.IsNullOrEmpty(databaseName)
        || string.IsNullOrEmpty(containerName)
        || string.IsNullOrEmpty(account) || string.IsNullOrEmpty(key))
    {
        throw new InvalidOperationException("Invalid Cosmos DB configuration. Ensure all required values are provided.");
    }
    Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
    Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

    return client;
}

#pragma warning disable 1591 // Disable warning related to missing XML comments

public partial class Program { }