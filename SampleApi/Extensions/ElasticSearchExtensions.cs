using Nest;
using SampleApi.Models;

namespace SampleApi.Extensions;

public static class ElasticSearchExtensions
{
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["ElasticConfiguration:Uri"];
        var defaultIndex = configuration["ElasticConfiguration:DefaultIndex"];
        var additionalIndexes = configuration.GetSection("ElasticConfiguration:AdditionalIndexes")
                                      .Get<List<string>>();


        var settings = new ConnectionSettings(new Uri(url))            
            .PrettyJson()
            .DefaultIndex(defaultIndex);

        AddDefaultMappings(settings);

        var client = new ElasticClient(settings);

        services.AddSingleton<IElasticClient>(client);

        CreateIndexForProduct(client, defaultIndex);

        CreateIndexForBook(client, additionalIndexes);
    }

    private static void AddDefaultMappings(ConnectionSettings settings)
    {
        settings.DefaultMappingFor<Product>(m => m);
                 

        settings.DefaultMappingFor<Book>(m => m);
    }

    private static void CreateIndexForProduct(IElasticClient client, string indexName)
    {
        client.Indices.Create(indexName, i => i.Map<Product>(x => x.AutoMap()).Settings(s=>
                                               s.NumberOfShards(2)
                                               .NumberOfReplicas(3)));
    }

    private static void CreateIndexForBook(IElasticClient client, IEnumerable<string> indexName)
    {
        foreach (var item in indexName)
        {
            client.Indices.Create(item, i => i.Map<Book>(x => x.AutoMap().Properties(props => props
            .Text(t => t
                .Name(book => book.Title)
                .Fields(f => f
                    .Keyword(k => k
                        .Name("keyword")
                        .IgnoreAbove(256)
                    )
                )))));
        }

    }
}
