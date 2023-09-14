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
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
var _logger = builder.Services?.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
// Add builder.Services to the container.

builder.Services!.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services!.AddEndpointsApiExplorer();

//config swagger
builder.Services!.AddSwaggerGen(setup =>
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

builder.Services!.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
//config dbcontext
builder.Services!.AddDbContext<ChatAPPContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DatabaseAWS")));

//config kafka
builder.Services!.AddHostedService<ConsumerService>();

builder.Services!.AddSingleton<IConsumer<Ignore, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"],
        GroupId = "chat-consumer",
        AutoOffsetReset = AutoOffsetReset.Latest,
        EnableAutoCommit = true
    };
    return new ConsumerBuilder<Ignore, string>(config)
     .SetPartitionsAssignedHandler((c, partitions) =>
     {
         _logger!.LogInformation($"Assigned partitions: [{string.Join(", ", partitions)}]");
         return partitions.Select(tp => new TopicPartitionOffset(tp, Offset.Beginning)).ToList();
     })
                .SetPartitionsRevokedHandler((c, partitions) =>
                {
                    _logger!.LogInformation($"Revoking assignment: [{string.Join(", ", partitions)}]");
                })
                .SetErrorHandler((_, e) => _logger!.LogError($"Error: {e.Reason}"))
                .SetValueDeserializer(Deserializers.Utf8)
    .Build();
});

builder.Services!.AddSingleton<IProducer<string, string>>(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = builder.Configuration["Kafka:BootstrapServers"]
    };
    return new ProducerBuilder<string, string>(config).Build();
});

//config signalr
builder.Services!.AddSignalR();

//config authentication
builder.Services!.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
builder.Services!.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services!.AddAuthorization(options =>
{
    var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
    policy = policy.RequireAuthenticatedUser();
    options.DefaultPolicy = policy.Build();
});

builder.Services!.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:4200") // Replace with the actual origin of your Angular app
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // Enable credentials (cookies) if needed
    });
});

// Add services 
builder.Services!.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400000;
    options.StreamBufferCapacity = 102400000;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
    options.KeepAliveInterval = TimeSpan.FromMinutes(1);
}
);

builder.Services!.AddSingleton<ChatHub>();

builder.Services!.AddScoped<UserServices>();
builder.Services!.AddScoped<ChannelServices>();
builder.Services!.AddScoped<NotificationServices>();
builder.Services!.AddSingleton<ConsumerService>();
builder.Services!.AddScoped<AwsSNSServices>();
builder.Services!.AddHostedService<AwsSNSServices>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/channels");
});


app.UseHttpsRedirection();


app.MapControllers();

app.Run();
