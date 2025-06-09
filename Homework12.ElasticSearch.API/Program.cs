using Homework12.ElasticSearch.API.Models;
using Homework12.ElasticSearch.API.Options;
using Homework12.ElasticSearch.API.Services.ElasticService;

var builder = WebApplication.CreateBuilder(args);

var elasticOptions = builder.Configuration.GetSection(nameof(ElasticOptions)).Get<ElasticOptions>()!;
builder.Services.AddScoped<IElasticService<Note>, ElasticService<Note>>(x 
    => new ElasticService<Note>(elasticOptions, "notes"));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();