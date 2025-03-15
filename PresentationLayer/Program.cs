using DataAccessLayer;
using Npgsql;
using NpgsqlTypes;
using Microsoft.Extensions.Configuration;
using BusinessLayer;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in the appsetting.json");
}
builder.Services.AddNpgsqlDataSource(
    connectionString,
    npgsqlBuilder => npgsqlBuilder.EnableParameterLogging()
);

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<ISkillsRepository, SkillsRepository>();
builder.Services.AddScoped<ISkillsService, SkillsService>();
builder.Services.AddScoped<IPostsRepository, PostsRepository>();
builder.Services.AddScoped<IPostsService, PostsService>();
builder.Services.AddScoped<IConnectionsRepository, ConnectionsRepository>();
builder.Services.AddScoped<IConnectionsService, ConnectionsService>();




builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.ConfigObject.AdditionalItems["docExpansion"] = "none";
    });
}

app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.Run();
