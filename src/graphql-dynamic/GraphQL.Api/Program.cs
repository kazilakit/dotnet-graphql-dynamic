using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Register MongoDB services
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));

builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IMongoClient>().GetDatabase("GraphQL"));



var app = builder.Build();

app.MapGraphQL();
app.Run();