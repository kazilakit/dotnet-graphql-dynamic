using System;
using HotChocolate.Execution;

namespace GraphQL.DomainService.Services;

public class SchemaConfigurationService : ISchemaConfigurationService
{
    private readonly SchemaBuilderService _schemaBuilderService;
    private readonly IRequestExecutorResolver _executorResolver;
    public SchemaConfigurationService(SchemaBuilderService schemaBuilderService, IRequestExecutorResolver executorResolver)
    {
        _executorResolver = executorResolver ?? throw new ArgumentNullException(nameof(executorResolver));
        _schemaBuilderService = schemaBuilderService ?? throw new ArgumentNullException(nameof(schemaBuilderService));
    }
    public async Task<ISchema> BuildSchemaAsync(CancellationToken cancellationToken)
    {
        var builder = SchemaBuilder.New();
        await _schemaBuilderService.BuildSchema(builder, cancellationToken);
        return builder.Create();
    }
    public async Task ConfigureSchemaAsync(ISchemaBuilder schemaBuilder, CancellationToken cancellationToken)
    {
        await _schemaBuilderService.BuildSchema(schemaBuilder, cancellationToken);
    }
    public async Task ReloadAsync(CancellationToken cancellationToken)
    {
        await BuildSchemaAsync(cancellationToken);
        _executorResolver.EvictRequestExecutor(Schema.DefaultName);

    }
}
