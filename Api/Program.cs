﻿using System.Text;
using Api.Data;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddScoped<EmailService>();

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            // validate the token based on the key we have provided inside appsettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            // the issuer which in here is the api project url we are using
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            // validate the issuer (who ever is issuing the JWT)
            ValidateIssuer = true,
            // dont validated audience (angular side)
            ValidateAudience = false
        };
    });

builder.Services.AddCors();

builder.Services.Configure<ApiBehaviorOptions>(opt =>
{
    opt.InvalidModelStateResponseFactory = (context) =>
    {
        var errors = context.ModelState
        .Where(i => i.Value.Errors.Count > 0)
        .SelectMany(i => i.Value.Errors)
        .Select(i => i.ErrorMessage).ToArray();

        var result = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(result);
    };
});

var app = builder.Build();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["JWT:ClientUrl"]);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// adding UseAuthentication into our pipeline and this should come before UseAuthorization
// Authentication verifies the identity of a user or service, and authorization determines their access rights
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

