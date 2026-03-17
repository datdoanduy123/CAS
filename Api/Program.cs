using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<Infrastructure.Persistence.AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection
builder.Services.AddScoped<Application.IRepositories.User.IUserRepository, Infrastructure.Repositories.User.UserRepository>();
builder.Services.AddScoped<Application.IServices.User.IUserService, Application.Services.User.UserService>();
builder.Services.AddScoped<Application.IRepositories.Role.IRoleRepository, Infrastructure.Repositories.Role.RoleRepository>();
builder.Services.AddScoped<Application.IServices.Role.IRoleService, Application.Services.Role.RoleService>();

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
