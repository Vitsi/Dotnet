using System.Text;
using Common.MongoDB;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Qr.MS.Models;
using System.IdentityModel.Tokens.Jwt;

//using Common.Settings;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMongo(builder.Configuration);
builder.Services.AddMongoService<QRCodeModel>("qrcode");

//Configuration setup
var Configuration = builder.Configuration;
//configuration for jwtbearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         ValidIssuer = Configuration["Jwt:Issuer"],
                         ValidAudience = Configuration["Jwt:Audience"],
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                     };
                 });

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//to add authorize in swagger
builder.Services.AddSwaggerGen(c =>
{
    
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Qr.MS", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert token",
        Name = "QrCode",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference=new OpenApiReference{
                    Type=ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string []{}
        }
    });
}
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

//Add middleware to inspect incoming token
app.Use(async (context, next) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", ""); // Remove "Bearer " prefix if present
    Console.WriteLine($"Received Token: {token}");

    if (!string.IsNullOrEmpty(token))
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken != null)
            {
                Console.WriteLine("Decoded Token:");
                foreach (var claim in jsonToken.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("Invalid JWT token format");
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error decoding token: {ex.Message}");
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error decoding token: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Token is null or empty.");
    }

    await next.Invoke();
});
//cors middleware
app.UseCors("AllowAnyOrigin");

app.MapControllers();

app.Run();
