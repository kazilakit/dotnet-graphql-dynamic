// using System;
// using GraphQL.DomainService.Enities;
// using GraphQL.DomainService.Models;
// using GraphQL.DomainService.Models.Constants;
// using GraphQL.DomainService.Repositories;
// using HotChocolate.Language;
// using HotChocolate.Resolvers;
// using MongoDB.Bson;
// using MongoDB.Driver;
//
// namespace GraphQL.DomainService.Resolvers;
//
// public class MutationResolver
// {
//     private readonly IRepository<BsonDocument> _repository;
//     public MutationResolver(IRepository<BsonDocument> repository)
//     {
//         _repository = repository;
//     }
//     public void ResolveInsertSchema(
//     IObjectTypeDescriptor descriptor,
//     SchemaDefinition schema,
//     InputObjectType inputType)
//     {
//         var fieldName = "insert" + schema.SchemaName;
//
//         descriptor.Field(fieldName)
//             .Argument("input", a => a.Type(inputType))
//             .Type<ObjectType<InsertResponse>>()
//             .Resolve(async ctx => await InsertAsync(schema, ctx));
//     }
//
//     public void ResolveUpdateSchema(
//         IObjectTypeDescriptor descriptor,
//         SchemaDefinition schema,
//         InputObjectType inputType)
//     {
//         var fieldName = "update" + schema.SchemaName;
//
//         descriptor.Field(fieldName)
//             .Argument("filter", a => a.Type<StringType>())
//             .Argument("input", a => a.Type(inputType))
//             .Type<ObjectType<UpdateResponse>>()
//             .Resolve(async ctx => await UpdateAsync(schema, ctx));
//     }
//
//     public void ResolveDeleteSchema(
//         IObjectTypeDescriptor descriptor,
//         SchemaDefinition schema)
//     {
//         var fieldName = "delete" + schema.SchemaName;
//
//         descriptor.Field(fieldName)
//             .Argument("filter", a => a.Type<StringType>())
//             .Type<ObjectType<DeleteResponse>>()
//             .Resolve(async ctx => await DeleteAsync(schema, ctx));
//     }
//     
//     public async Task<InsertResponse> InsertAsync(
//         SchemaDefinition schema,
//         IResolverContext context)
//     {
//         
//         var input = context.ArgumentValue<Dictionary<string, object>>("input");
//         if (input.ContainsKey(GraphQLConstant.InputEntityIdFieldName))
//         {
//             var itemId = input[GraphQLConstant.InputEntityIdFieldName];
//             input.Add(GraphQLConstant.DbEntityIdFieldName, itemId ?? Guid.NewGuid().ToString());
//             input.Remove(GraphQLConstant.InputEntityIdFieldName);
//         }
//         else
//         {
//             input.Add(GraphQLConstant.DbEntityIdFieldName, Guid.NewGuid().ToString());
//         }
//
//         var document = BsonDocument.Create(input);
//         await _repository.InsertAsync(schema.CollectionName, document);
//
//         return new InsertResponse
//         {
//             Acknowledged = true,
//             InsertedId = document[GraphQLConstant.DbEntityIdFieldName].ToString()
//         };
//     }
//     public async Task<UpdateResponse> UpdateAsync(
//         SchemaDefinition schema,
//         IResolverContext context)
//     {
//         var filterJson = context.ArgumentValue<string>("filter");
//
//         var inputLiteral = context.ArgumentLiteral<IValueNode>("input") as ObjectValueNode;
//
//         var input = inputLiteral?.Fields.ToDictionary(
//             f => f.Name.Value,
//             f => ParseValueNode(f.Value)
//         ) ?? new Dictionary<string, object>();
//         var filter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filterJson);
//
//         var update = new BsonDocument(
//             input.Select(kv => new BsonElement(kv.Key, BsonValue.Create(kv.Value))));
//
//         var result = await _repository.UpdateAsync(schema.CollectionName, filter, update);
//
//         return new UpdateResponse
//         {
//             Acknowledged = result.IsAcknowledged,
//             TotalUpdated = result.ModifiedCount
//         };
//     }
//     public async Task<DeleteResponse> DeleteAsync(
//         SchemaDefinition schema,
//         IResolverContext context)
//     {
//         var filterJson = context.ArgumentValue<string>("filter");
//         var filter = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(filterJson);
//         var result = await _repository.DeleteAsync(schema.CollectionName, filter);
//
//         return new DeleteResponse
//         {
//             Acknowledged = result.IsAcknowledged,
//             TotalDeleted = result.DeletedCount
//         };
//     }
//     
//     private object? ParseValueNode(IValueNode valueNode)
//     {
//         return valueNode switch
//         {
//             IntValueNode iv => int.Parse(iv.Value),
//             FloatValueNode fv => double.Parse(fv.Value),
//             StringValueNode sv => sv.Value,
//             BooleanValueNode bv => bv.Value,
//             NullValueNode => null,
//             ListValueNode lv => lv.Items.Select(ParseValueNode).ToList(),
//             ObjectValueNode ov => ov.Fields.ToDictionary(f => f.Name.Value, f => ParseValueNode(f.Value)),
//             _ => throw new NotSupportedException($"Unsupported value node type: {valueNode.GetType().Name}")
//         };
//     }
//
//
//
// }
