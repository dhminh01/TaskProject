using System.Reflection;
using HotChocolate.Types;
using HotChocolate.Execution.Configuration;
using TaskService.Api.GraphQL.Mutations;
using TaskService.Api.GraphQL.Queries;
using TaskService.API.GraphQL.Filters;
using TaskService.Application.Common.Markers;

namespace TaskService.API.Extensions;

public static class GraphQLExtensions
{
    public static IServiceCollection AddGraphQLServices(this IServiceCollection services, IWebHostEnvironment environment)
    {
        var graphQLBuilder = services
            .AddGraphQLServer()
            .AddQueryType<TaskQueries>()
            .AddMutationType<TaskMutations>()
            .AddErrorFilter<GraphQLErrorFilter>()
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = environment.IsDevelopment())
            .AddProjections()
            .AddFiltering()
            .AddSorting();

        // Automatically register all GraphQL types from the assembly
        RegisterGraphQLTypes(graphQLBuilder, typeof(IGraphQLMarker).Assembly);
        RegisterGraphQLTypes(graphQLBuilder, typeof(Program).Assembly);

        return services;
    }

    private static void RegisterGraphQLTypes(IRequestExecutorBuilder builder, Assembly assembly)
    {
        var types = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                (typeof(IType).IsAssignableFrom(type) || // HotChocolate types
                 typeof(IGraphQLMarker).IsAssignableFrom(type))); // Our custom marked types

        foreach (var type in types)
        {
            builder.AddType(type);
        }
    }
}
