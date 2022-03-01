using CSVBackend.services;
using MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongConnString = builder.Configuration.GetValue<string>("MongoDBConnString");
var mongoDBName = builder.Configuration.GetValue<string>("MongoDbName");
builder.Services.AddSingleton<IMongoDBConnector>(new MongoDBConnector(mongConnString, mongoDBName));
builder.Services.AddSingleton<ICSVImportService, CSVImportService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
