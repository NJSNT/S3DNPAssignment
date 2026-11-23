using EfcRepositories;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using WebAPI.ErrorHandling;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

string connectionString = builder.Configuration
    .GetConnectionString("ForumDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'ForumDatabase' was not found.");

builder.Services.AddDbContext<EfcRepositories.AppContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IUserRepository, EfcUserRepository>();
builder.Services.AddScoped<IPostRepository, EfcPostRepository>();
builder.Services.AddScoped<ICommentRepository, EfcCommentRepository>();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    AllowStatusCode404Response = true
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
