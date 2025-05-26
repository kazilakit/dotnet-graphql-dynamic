using System;

namespace GraphQL.DomainService.Services;

public interface ISchemaConfigurationService
{
    Task<ISchema> BuildSchemaAsync(CancellationToken cancellationToken);
    Task ConfigureSchemaAsync(ISchemaBuilder schemaBuilder, CancellationToken cancellationToken);
    Task ReloadAsync(CancellationToken cancellationToken);
}
