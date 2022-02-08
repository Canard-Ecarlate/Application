using System.Text;
using AutoMapper;
using DuckCity.Api;
using DuckCity.Api.Mappings;
using DuckCity.Application.AuthenticationService;
using DuckCity.Application.RoomPreviewService;
using DuckCity.Application.RoomService;
using DuckCity.Application.Services.Interfaces;
using DuckCity.Application.UserService;
using DuckCity.Infrastructure;
using DuckCity.Infrastructure.RoomPreviewRepository;
using DuckCity.Infrastructure.RoomRepository;
using DuckCity.Infrastructure.UserRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

InitDotEnv();

new ConfigurationBuilder()
        .AddEnvironmentVariables()
        .Build();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
IServiceCollection services = builder.Services;

/*
 * Services
 */
Cors();
Singletons();
services.AddControllers();
services.AddEndpointsApiExplorer();
SwaggerServices();
AutoMapperServices();
AuthenticationAuthorisationServices();

/*
 * Build app
 */
WebApplication app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();


/*
 * Methods
 */
void Cors()
{
    services.AddCors(options => options
        .AddPolicy("CorsPolicy",
            policyBuilder => policyBuilder.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader()
                .AllowCredentials()));
}
void Singletons()
{
    // Repositories
    services.AddSingleton<IUserRepository, UserMongoRepository>();
    services.AddSingleton<IRoomPreviewRepository, RoomPreviewMongoRepository>();
    services.AddSingleton<IRoomRepository, RoomCacheRepository>();
    
    // Services
    services.AddSingleton<IAuthenticationService, AuthenticationService>();
    services.AddSingleton<IRoomService, RoomService>();
    services.AddSingleton<IUserService, UserService>();
    services.AddSingleton<IRoomPreviewService, RoomPreviewService>();
    
    // Mongo
    services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));
    services.AddSingleton<IMongoDbSettings>(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);
}

void SwaggerServices()
{
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo {Title = "DuckCity.Api", Version = "v1"});
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter JWT with Bearer into field",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                },
                Array.Empty<string>()
            }
        });
    });
}
void AutoMapperServices()
{
    MapperConfiguration mapperConfig = new(mc => { mc.AddProfile(new MappingProfile()); });
    IMapper mapper = mapperConfig.CreateMapper();
    services.AddSingleton(mapper);
}
void AuthenticationAuthorisationServices()
{
    string strKey = Environment.GetEnvironmentVariable("SIGNATURE_KEY") ?? throw new InvalidOperationException();
    TokenValidationParameters tokenValidationParameters = new()
    {
        ValidIssuer = "https://canardecarlate.fr",
        ValidAudience = "https://canardecarlate.fr",
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey)),
        ClockSkew = TimeSpan.Zero // remove delay of token when expire
    };

    services
        .AddAuthentication(options => { options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
        .AddJwtBearer(cfg => { cfg.TokenValidationParameters = tokenValidationParameters; });

    services.AddAuthorization(cfg =>
    {
        cfg.AddPolicy("player", policy => policy.RequireClaim("type", "player"));
        cfg.AddPolicy("ClearanceLevel1", policy => policy.RequireClaim("ClearanceLevel", "1"));
    });
}

void InitDotEnv() {
    string? root = Path.GetDirectoryName(typeof(Program).Assembly.Location);
    string dotenv = Path.Combine(root ?? throw new InvalidOperationException(), ".env");
    DotEnv.Load(dotenv);
}

namespace DuckCity.Api
{
    public class ProgramApi { }
}