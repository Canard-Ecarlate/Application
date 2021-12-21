using System.Text;
using AutoMapper;
using DuckCity.Api;
using DuckCity.Api.Mappings;
using DuckCity.Application;
using DuckCity.Infrastructure;
using DuckCity.Infrastructure.Repositories;
using DuckCity.Infrastructure.StoreDatabaseSettings;
using DuckCity.Infrastructure.StoreDatabaseSettings.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

string root = Directory.GetCurrentDirectory();
string dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;
IServiceCollection services = builder.Services;

/*
 * Services
 */
Cors();
Singletons();
services.AddControllers();
MongoServices();
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
    services.AddSingleton<UserRepository>();
    services.AddSingleton<RoomRepository>();
    services.AddSingleton<AuthenticationService>();
    services.AddSingleton<RoomService>();
}
void MongoServices()
{
    services.Configure<UserStoreDatabaseSettings>(configuration.GetSection(nameof(UserStoreDatabaseSettings)));
    services.AddSingleton<IUserStoreDatabaseSettings>(sp =>
        sp.GetRequiredService<IOptions<UserStoreDatabaseSettings>>().Value);

    services.Configure<RoomStoreDatabaseSettings>(configuration.GetSection(nameof(RoomStoreDatabaseSettings)));
    services.AddSingleton<IRoomStoreDatabaseSettings>(sp =>
        sp.GetRequiredService<IOptions<RoomStoreDatabaseSettings>>().Value);

    services.Configure<UserStatisticsStoreDatabaseSettings>(configuration.GetSection(nameof(UserStatisticsStoreDatabaseSettings)));
    services.AddSingleton<IUserStatisticsStoreDatabaseSettings>(sp =>
        sp.GetRequiredService<IOptions<UserStatisticsStoreDatabaseSettings>>().Value);
            
    services.Configure<GlobalStatisticsStoreDatabaseSettings>(configuration.GetSection(nameof(GlobalStatisticsStoreDatabaseSettings)));
    services.AddSingleton<IGlobalStatisticsStoreDatabaseSettings>(sp =>
        sp.GetRequiredService<IOptions<GlobalStatisticsStoreDatabaseSettings>>().Value);
            
    services.Configure<CardsConfigurationUserStoreDatabaseSettings>(configuration.GetSection(nameof(CardsConfigurationUserStoreDatabaseSettings)));
    services.AddSingleton<ICardsConfigurationUserStoreDatabaseSettings>(sp =>
        sp.GetRequiredService<IOptions<CardsConfigurationUserStoreDatabaseSettings>>().Value);
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
    TokenValidationParameters tokenValidationParameters = new()
    {
        ValidIssuer = "https://canardecarlate.fr",
        ValidAudience = "https://canardecarlate.fr",
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ")),
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