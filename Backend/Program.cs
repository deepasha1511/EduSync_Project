using EduSync.Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using EduSync.Backend.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddControllers();
builder.Services.AddSingleton<BlobService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS (single configuration is enough)
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactAppPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:3000") // Your frontend URL
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add DbContext
builder.Services.AddDbContext<EduSyncDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
// Global exception handler
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom 404 error handling middlewear
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseHttpsRedirection();

// CORS middleware - use the policy you defined
app.UseCors("ReactAppPolicy");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Error endpoint
app.Map("/error", (HttpContext context) =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    return Results.Problem(
        title: exception?.Message ?? "An error occurred",
        statusCode: context.Response.StatusCode
    );
});

app.MapControllers();

app.Run();