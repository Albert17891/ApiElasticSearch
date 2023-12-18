using Microsoft.AspNetCore.Mvc;
using Nest;
using SampleApi.Models;

namespace SampleApi.Controllers;
[Route("[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IElasticClient _elasticClient;
    private readonly ILogger _logger;


    public ProductsController(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;

    }

    [Route("GetProducts")]
    [HttpGet]
    public async Task<IActionResult> GetProduct(string keyword)
    {
        //var result = await _elasticClient.SearchAsync<Product>(s =>
        //s.Query(q => q.QueryString(d => d.Query('*' + keyword + '*')
        //    )).Size(1000));

        //  var result = await _elasticClient.SearchAsync<Product>(s => s.Query(q=>q.Wildcard(w=>w.Field(f=>f.Name).Value('*'+keyword+'*'))));  //Widcard query
        //var result = await _elasticClient.SearchAsync<Product>(s => s.Query(q => q.Bool(b => b.Must(m=>
        //                                                                                     m.Wildcard(w=>w
        //                                                                                      .Field(f=>f.Name)
        //                                                                                      .Value('*'+keyword+'*')))
        //                                                                                 .Filter(f=>
        //                                                                                 f.Range(r=>
        //                                                                                 r.Field(x=>x.Price)
        //                                                                                 .GreaterThan(10))))));  //Bool Query

        //var result = await _elasticClient.SearchAsync<Product>(s => s.Query(q => q
        //                                                         .Match(t => t
        //                                                         .Field(f => f.Name)
        //                                                         .Query(keyword))));

        var result = await _elasticClient.SearchAsync<Product>(s => s.Query(q => q.Term(t => t
                                                                 .Field(f => f.Name)
                                                                 .Value(keyword))));



        if (result.IsValid)
        {
            return Ok(result.Documents.ToList());
        }

        return BadRequest();
    }

    [Route("AddProduct")]
    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product)
    {

        var res = await _elasticClient.IndexAsync(product, i => i.Id(product.Id));

        if (res.IsValid)
        {
            return Ok(res.Id);
        }

        return BadRequest();
    }

    [Route("UpdateProduct")]
    [HttpPut]
    public async Task<IActionResult> UpdateProduct(Product product)
    {
        var res = await _elasticClient.UpdateAsync(DocumentPath<Product>.Id(product.Id), u => u
                                     .Index<Product>()
                                     .Doc(product)
                                     .DocAsUpsert());

        if (res.IsValid)
        {
            return Ok(res.Id);
        }

        return BadRequest();
    }

    [Route("DeleteProduct")]
    [HttpDelete]
    public async Task<IActionResult> DeleteProduct(int index)
    {
        var res = await _elasticClient.DeleteAsync(DocumentPath<Product>.Id(index));

        if (res.IsValid)
            return Ok();


        return BadRequest();
    }

    [Route("Aggregation")]
    [HttpGet]
    public async Task<IActionResult> TestAggregation()
    {
        var res = await _elasticClient.SearchAsync<Product>(s =>
                                     s.Aggregations(a => a.Average("average_price", avg => avg.
                                       Field(p => p.Price))));

        if (res.IsValid)
            return Ok(res.Aggregations.Average("average_price").Value);


        return BadRequest();
    }

    [Route("Query")]
    [HttpPost]
    public async Task<IActionResult> Query([FromBody] QueryContainer queryContainer)
    {
        var response = await _elasticClient.SearchAsync<Product>(s => s.Index("").Query(q => queryContainer));
       

        if (response.IsValid)
        {
            return Ok(response.Documents.ToList());
        }
        else
        {
            return BadRequest();
        }


    }

}
