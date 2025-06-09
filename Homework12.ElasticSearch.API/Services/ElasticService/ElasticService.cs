using System.Collections.ObjectModel;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Mapping;
using Homework12.ElasticSearch.API.Models;
using Homework12.ElasticSearch.API.Options;

namespace Homework12.ElasticSearch.API.Services.ElasticService;

public class ElasticService<T> : IElasticService<T>
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly string _index;

    public ElasticService(ElasticOptions options, string index)
    {
        _options = options;
        _index = index;

        var settings = new ElasticsearchClientSettings(new Uri(options.Url))
            // .Authentication()
            .DefaultIndex(_index);

        _client = new ElasticsearchClient(settings);
        
        if (!_client.Indices.Exists(_index).Exists)
            _client.Indices.Create(_index);
    }

    public async Task<bool> AddOrUpdateAsync(T entity)
    {
        var response = await _client.IndexAsync(entity, idx => 
            idx.Index(_index)
                .OpType(OpType.Index));
        
        return response.IsValidResponse;
    }

    public async Task<bool> AddOrUpdateBulkAsync(IEnumerable<T> entities)
    {
        var response = await _client.BulkAsync(b => b.Index(_index)
            .UpdateMany(entities, (nd, n) => nd.Doc(n).DocAsUpsert()));
        
        return response.IsValidResponse;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        var response = await _client.DeleteAsync<T>(key, d => 
            d.Index(_index));
        
        return response.IsValidResponse;
    }

    public async Task<T?> GetAsync(string key)
    {
        var response = await _client.GetAsync<T>(key, g =>
            g.Index(_index));

        return response.Source;
    }

    public async Task<IReadOnlyCollection<T>> SearchAsync(string searchString, string fieldName)
    {
        var response = await _client.SearchAsync<T>(s => s
            .Query(q => q
                .Wildcard(w => w
                        .Field(fieldName)
                        .Value($"*{searchString.ToLower()}*")  // lowercase обязательно, если keyword
                )
            )
        );

        return response.Documents;
    }

    public async Task<double> GetAvgId()
    {
        // var response = await _client.SearchAsync<Note>(s => s
        //     .Index("your-index")
        //     .Size(0) // мы только агрегируем, документы не нужны
        //     .RuntimeMappings(rm => rm
        //         .Add("word_length", new RuntimeField
        //         {
        //             Type = "long",
        //             Script = new Script()
        //             {
        //                 Source = """
        //                              if (doc['message.keyword'].size() > 0) {
        //                                  def words = doc['message.keyword'].value.splitOnToken(' ');
        //                                  if (words.length > 0) {
        //                                      emit(words[0].length());
        //                                  }
        //                              }
        //                          """
        //             }
        //         })
        //     )
        //     .Aggregations(agg => agg
        //         .Add("by_word_length", new TermsAggregation
        //         {
        //             Field = "word_length",
        //             Size = 100,
        //             Order = new Collection<KeyValuePair<Field, SortOrder>>
        //             {
        //                 new KeyValuePair<Field, SortOrder>("word_length", SortOrder.Asc)
        //             }
        //         })
        //     )
        // );
        //
        // var result = new Dictionary<int, long>();
        // if (response.Aggregations.TryGetValue("by_word_length", out var agg) &&
        //     agg is TermsAggregate<long> termAgg)
        // {
        //     foreach (var bucket in termAgg.Buckets)
        //     {
        //         if (bucket.Key.HasValue)
        //             result[(int)bucket.Key.Value] = bucket.DocCount ?? 0;
        //     }
        // }
        //
        // return result;
        
        var response = await _client
            .SearchAsync<Note>(search => search
                .Index(_index)
                .Query(query => query
                    .MatchAll(_ => {})
                )
                .Aggregations(aggregations => aggregations
                    .Add("max_id", aggregation => aggregation
                        .Avg(avg => avg
                            .Field(x => x.Id)
                        )
                    )
                )
                .Size(10)
            );
        
        return response.Aggregations!.GetAverage("max_id")!.Value!.Value;
    }
}