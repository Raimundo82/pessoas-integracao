using Microsoft.EntityFrameworkCore;
using SigdnRhStaggingApi.Data;
using SigdnRhStaggingApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<RhStaggingDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options => options.DocumentPath = "/openapi/v1.json");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
