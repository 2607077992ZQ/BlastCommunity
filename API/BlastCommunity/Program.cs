var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyOrigins",
        builder => builder
            .WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    //    c.RoutePrefix = string.Empty;
    //});
}
app.UseCors("AllowMyOrigins");

app.UseAuthorization();

app.MapControllers();

string value()
{
    var address = System.Text.Json.JsonDocument.Parse(File.ReadAllText("config.json")).RootElement.GetProperty("apiaddress");
    return $"{address.GetProperty("webType").GetString()}://{address.GetProperty("address").GetString()}:{address.GetProperty("port").GetInt32()}";
}
app.Run(value());

//app.Run();