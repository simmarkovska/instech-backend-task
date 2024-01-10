using System.Text.Json.Serialization;
using Claims.Auditing;
using Claims.Auditing.Interfaces;
using Claims.Services;
using Claims.Services.Interfaces;
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
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuditContext, AuditContext>();
builder.Services.AddScoped<IClaimService, ClaimService>(sp => {
    var databaseName = builder.Configuration.GetValue<string>("CosmosDb:DatabaseName");
    var containerName = builder.Configuration.GetValue<string>("CosmosDb:ContainerName");
    var cosmosClient = sp.GetRequiredService<CosmosClient>();
    var coverService = sp.GetRequiredService<ICoverService>();
    return new ClaimService(cosmosClient, databaseName, containerName, coverService);
});

builder.Services.AddTransient<ICoverService, CoverService>(sp => {
    var databaseName = builder.Configuration.GetValue<string>("CosmosDb:DatabaseName");
    var cosmosClient = sp.GetRequiredService<CosmosClient>();
    var auditContext = sp.GetRequiredService<IAuditContext>();
    return new CoverService(cosmosClient, databaseName, null, auditContext);
});

builder.Services.AddScoped<IMessageService, MessageService>();

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
    string databaseName = configurationSection.GetSection("DatabaseName").Value;
    string containerName = configurationSection.GetSection("ContainerName").Value;
    string account = configurationSection.GetSection("Account").Value;
    string key = configurationSection.GetSection("Key").Value;
    Microsoft.Azure.Cosmos.CosmosClient client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
    //CosmosDbService cosmosDbService = new CosmosDbService(client, databaseName, containerName);
    Microsoft.Azure.Cosmos.DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
    await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

    return client;
}

public partial class Program { }