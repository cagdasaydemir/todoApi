using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using todoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
public readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
{
    builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
}));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "todoApi", Version = "v1" });
    c.AddSecurityDefinition("Bearer",
      new OpenApiSecurityScheme
      {
          Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
          Name = "Authorization",
          In = ParameterLocation.Header,
          Type = SecuritySchemeType.ApiKey,
          Scheme = "Bearer"
      });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
});

var connectionString = builder.Configuration.GetConnectionString("TodoDb");

builder.Services.AddDbContext<TodoContext>(x =>
{
    x.UseSqlServer(connectionString);
});

var key = Encoding.ASCII.GetBytes(_appsettings.application.Secret);

// þema formatýnýn nasýl olacaðýný belirtiyoruz
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme =
    JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme =
    JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.Audience = "TodoApi";
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.ClaimsIssuer = "burasýne";
    x.TokenValidationParameters = new TokenValidationParameters
    {
        // jwtnin oluþuturulmasý ve doðrulanmasý için string key
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = true
    };
    x.Events = new JwtBearerEvents()
    {
        OnTokenValidated = (context) =>
        {
            var name = context.Principal.Identity.Name;
            if (string.IsNullOrEmpty(name))
            {
                context.Fail("Unathorized. Please re-login.");
            }
            return Task.CompletedTask;
        }
    };

});
        



builder.Services.AddAuthentication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
   
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "todoApi v1"));
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseCors("MyPolicy");

app.MapControllers();

app.Run();
