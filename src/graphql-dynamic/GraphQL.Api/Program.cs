using GraphQL.DomainService;
using GraphQL.DomainService.Enities;
using GraphQL.DomainService.Repositories;
using GraphQL.DomainService.Resolvers;
using GraphQL.DomainService.Services;
using HotChocolate;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Register MongoDB services
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IMongoClient>().GetDatabase("GraphQL"));

builder.Services.AddSingleton<IRepository<BsonDocument>, GraphqlRepository>();
builder.Services.AddSingleton<IRepository<SchemaDefinition>, SchemaDefinitionRepository>();
builder.Services.AddSingleton<SchemaBuilderService>();
builder.Services.AddSingleton<ISchemaConfigurationService, SchemaConfigurationService>();
builder.Services.AddSingleton<QueryResolver>();
builder.Services.AddSingleton<MutationResolver>();

// Register GraphQL
builder.Services
    .AddGraphQLServer()
    .ConfigureSchemaAsync(async (services, schemaBuilder, cancellationToken) =>
    {
        var provider = services.GetRequiredService<ISchemaConfigurationService>();
        await provider.ConfigureSchemaAsync(schemaBuilder, cancellationToken);

    });

builder.Services.AddControllers();

var app = builder.Build();

app.MapGraphQL();
app.MapControllers();
app.Run();