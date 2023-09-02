using ChatServer.Data;
using ChatServer.Hubs;
using ChatServer.Services;
using Confluent.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add builder.Services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//config swagger
builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});
//config dbcontext
builder.Services.AddDbContext<ChatAPPContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("ChatAPP")));

//config kafka
builder.Services.AddHostedService<ConsumerService>();

builder.Services.AddSingleton<ProducerConfig>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = configuration["Kafka:BootstrapServers"],
		Debug = "all"
    };

    return config;
});
builder.Services.AddSingleton<ConsumerConfig>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = configuration["Kafka:BootstrapServers"],
        ClientId = configuration["Kafka:ClientId"],
        GroupId = configuration["Kafka:GroupId"],
        AutoOffsetReset = AutoOffsetReset.Earliest,
        SocketTimeoutMs = 5000,
		EnableAutoCommit = true,
		EnableAutoOffsetStore = true,
		EnablePartitionEof = true,
		AutoCommitIntervalMs = 5000,
		StatisticsIntervalMs = 60000,
		Debug = "all",
    };

    return config;
});

//config signalr
builder.Services.AddSignalR();

//config authentication
builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization(options =>
{
    var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
    policy = policy.RequireAuthenticatedUser();
    options.DefaultPolicy = policy.Build();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


// Add services 
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400000;
    options.StreamBufferCapacity = 102400000;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
    options.KeepAliveInterval = TimeSpan.FromMinutes(1);
}
);

builder.Services.AddSingleton<ChatHub>();

builder.Services.AddScoped<UserServices>();
builder.Services.AddScoped<ChannelServices>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/hubs/chat");
});


app.UseHttpsRedirection();


app.MapControllers();

app.Run();
