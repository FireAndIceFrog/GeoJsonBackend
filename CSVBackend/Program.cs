using CSVBackend.Map.DataAccess;
using CSVBackend.Map.Services;
using CSVBackend.Ryan.services;
using MongoDB;

var MyAllowSpecificOrigins = "MyPolicy";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      builder =>
                      {
                          builder
                          .AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .SetIsOriginAllowedToAllowWildcardSubdomains();
                      });
});
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mongConnString = builder.Configuration.GetValue<string>("MongoDBConnString");
var mongoDBName = builder.Configuration.GetValue<string>("MongoDbName");
builder.Services.AddSingleton<IMongoDBConnector>(new MongoDBConnector(mongConnString, mongoDBName));
builder.Services.AddSingleton<ICSVImportService, CSVImportService>();
builder.Services.AddSingleton<IGeoJsonDataAccess, GeoJsonDataAccess>();
builder.Services.AddSingleton<IMapLayerService, MapLayerService>();

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

app.UseCors(MyAllowSpecificOrigins);

app.Run();
