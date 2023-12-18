using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using SampleApi.Models;

namespace SampleApi.Controllers;
[Route("[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IElasticClient _elasticClient;

    public BookController(IElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    [Route("AddBooks")]
    [HttpGet]
    public async Task<IActionResult> AddBulkBook()
    {
        var json = System.IO.File.ReadAllText(@"C:\Users\albert.gevorqian\source\repos\SampleApiElastic\SampleApi\sample-data.json");

        var books = JsonConvert.DeserializeObject<List<Book>>(json);

        if (books != null)
        {
            int count = 0;

            foreach (var item in books)
            {
                await _elasticClient.IndexAsync(item,i=>i.Id(++count).Index("book"));
            }
        }

        return Ok();
    }

    [Route("GetBooks")]
    [HttpGet]
    public async Task<IActionResult> Get(string keyword)
    {
        // var result = await _elasticClient.SearchAsync<Book>(s =>
        //s.Query(q => q.QueryString(d => d.Query('*' + keyword + '*')
        //     )).Size(1000));

        var result = await _elasticClient.SearchAsync<Book>(s => s.Query(x=>x.MatchAll()));

        if (result.IsValid)
        {
            return Ok(result.Documents.ToList());
        }
             
        return BadRequest();    
       
    }

    [Route("UpdateBook")]
    [HttpPost]
    public async Task<IActionResult> UpdateBook(string keyword)
    {
        var result = await _elasticClient.SearchAsync<Book>(s =>
        s.Query(q => q.QueryString(d => d.Query('*' + keyword + '*')
             )).Size(1000));

        dynamic update=new System.Dynamic.ExpandoObject();
        update.Title = "My";


        foreach (var hit in result.Hits)
        {
            var bookId = hit.Id;

            var res = await _elasticClient.UpdateAsync<Book>(bookId, x => x
                                         .Index("book")
                                          .Doc(new Book
                                          {
                                              Title="Hi"
                                          })
                                          .DocAsUpsert()
                                         );
        }



        return Ok();
    }

    [Route("Query")]
    [HttpPost]
    public async Task<IActionResult> Query(QueryContainer predicate)
    {
        var response = await _elasticClient.SearchAsync<Book>(s => s.Index("book").Query(x => predicate));

        if (response.IsValid)
        {
            return Ok(response.Documents.ToList());
        }
        else
            return BadRequest();
    }
}
