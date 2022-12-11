using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Varausharjoitus.Middleware;
using Varausharjoitus.Models;
using Varausharjoitus.Repositories;
using Varausharjoitus.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // Add services to the container.
builder.Services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddEndpointsApiExplorer();

//swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ToDo API",
        Description = "An ASP.NET Core Web API for managing ToDo items",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Example Contact",
            Url = new Uri("https://example.com/contact")
        },
        License = new OpenApiLicense
        {
            Name = "Example License",
            Url = new Uri("https://example.com/license")
        }
    });
    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});
builder.Services.AddDbContext<ReservationContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("VarausharjoitusDB")));
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    ReservationContext dbcontexct = scope.ServiceProvider.GetRequiredService<ReservationContext>();
    dbcontexct.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

