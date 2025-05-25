using GraphQL.DomainService;
using GraphQL.DomainService.Resolvers;
using GraphQL.DomainService.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Register MongoDB services
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IMongoClient>().GetDatabase("GraphQL"));

builder.Services.AddSingleton<SchemaBuilderService>();
builder.Services.AddSingleton<QueryResolver>();
builder.Services.AddSingleton<MutationResolver>();

// Register GraphQL
builder.Services
    .AddGraphQLServer()
    .ConfigureSchemaAsync(async (services, schemaBuilder, cancellationToken) =>
    {
        var _schemaBuilder = services.GetRequiredService<SchemaBuilderService>();
        await _schemaBuilder.BuildSchema(schemaBuilder, cancellationToken);
    });

var app = builder.Build();

app.MapGraphQL();
app.Run();