using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<VtContext>(i =>
{
    i.UseSqlite(builder.Configuration.GetConnectionString("Local"));
});

builder.Services.AddIdentityCore<User>(i =>
{
    i.Password.RequiredLength = 6;
    i.Password.RequireDigit = false;
    i.Password.RequireLowercase = false;
    i.Password.RequireUppercase = false;
    i.Password.RequireNonAlphanumeric = false;

    i.SignIn.RequireConfirmedEmail = true;
}).AddRoles<IdentityRole>() // be able to add roles
  .AddRoleManager<RoleManager<IdentityRole>>() //be able to make use of RoleManager
  .AddEntityFrameworkStores<VtContext>() // providing our context
  .AddSignInManager<SignInManager<User>>() // make use of signin manager
  .AddUserManager<UserManager<User>>() // make use of UserManager to create users
  .AddDefaultTokenProviders(); // be able to create tokens for email confirmation

builder.Services.AddScoped<JWTService>();

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

