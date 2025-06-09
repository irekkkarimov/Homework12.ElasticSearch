using System.Text.RegularExpressions;
using Homework12.ElasticSearch.API.Models;
using Homework12.ElasticSearch.API.Services.ElasticService;
using Microsoft.AspNetCore.Mvc;

namespace Homework12.ElasticSearch.API.Controllers;

[Route("[controller]")]
[ApiController]
public class NotesController(IElasticService<Note> elasticService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(string key)
    {
        var result = await elasticService.GetAsync(key);
        
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetAsync(string key, string fieldName)
    {
        var result = await elasticService.SearchAsync(key, fieldName);
        
        return Ok(result);
    }
    
    [HttpGet("getAvgNoteId")]
    public async Task<IActionResult> GetAvgNoteIdAsync()
    {
        var result = await elasticService.GetAvgId();
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddOrUpdate(Note note)
    {
        var result = await elasticService.AddOrUpdateAsync(note);
        
        return Ok(result);
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        var result = await elasticService.DeleteAsync(key);
        
        return Ok(result);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed()
    {
        const string text = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum" +
                            " deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non " +
                            "provident, similique sunt in culpa qui officia deserunt mollitia animi, id est laborum et dolorum" +
                            " fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta " +
                            "nobis est eligendi optio cumque nihil impedit quo minus id quod maxime placeat facere possimus, " +
                            "omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis" +
                            " debitis aut rerum necessitatibus saepe eveniet ut et voluptates repudiandae sint et molestiae " +
                            "non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus" +
                            " maiores alias consequatur aut perferendis doloribus asperiores repellat.";

        var words = Regex.Split(text, @"[\s,\.]+")
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Select((word, index) => new Note
            {
                Id = index + 1,
                Message = word
            })
            .ToList();

        var response = await elasticService.AddOrUpdateBulkAsync(words);
        
        return Ok(response);
    }
}