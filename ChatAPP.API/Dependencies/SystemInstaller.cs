using ChatAPP.API.Contexts;
using ChatAPP.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace ChatAPP.API.Dependencies
{
    public class SystemInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "ChatAPP", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
		            Enter 'Bearer' [space] and then your token in the text input below.
		            \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            services.AddControllers();

            services.AddDbContext<ChatAPPContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("ChatAPP")));

            services.AddSignalR();
            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            var signingKey = Convert.FromBase64String(configuration["Jwt:SignKey"]);
                            var encryptKey = Convert.FromBase64String(configuration["Jwt:EncryptKey"]);
                            var issuer = configuration["APIUrl"];

                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidIssuer = issuer,
                                ValidateIssuer = false,
                                ValidateAudience = false,
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(signingKey),
                                TokenDecryptionKey = new SymmetricSecurityKey(encryptKey),
                                RequireExpirationTime = true,
                                ValidateLifetime = true,
                                ClockSkew = TimeSpan.Zero
                            };
                            options.Events = new JwtBearerEvents
                            {
                                OnMessageReceived = context =>
                                {
                                    var accessToken = context.Request.Query["access_token"];
                                    var path = context.HttpContext.Request.Path;
                                    if (!string.IsNullOrEmpty(accessToken) &&
                                        (path.StartsWithSegments("/hubs")))
                                    {
                                        context.Token = accessToken;
                                    }
                                    return Task.CompletedTask;
                                }
                            };

                        });

           services.AddAuthorization(options =>
            {
                var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                policy = policy.RequireAuthenticatedUser();
                options.DefaultPolicy = policy.Build();
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

        }
    }
}
