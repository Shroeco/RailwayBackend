using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Railway.Infrastructure.Data;
using Railway.Infrastructure.Data.Seed;
using Railway.Application.Repositories;
using Railway.Infrastructure.Persistence;
using Railway.Application.Services;
using Railway.Infrastructure.Carriers.Avanti;
using Railway.Infrastructure.Carriers.Lner;
using Railway.Api.ExceptionHandling;
using Railway.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RailwayDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("RailwayDatabase")));

builder.Services.AddScoped<IJourneyRepository, JourneyRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

builder.Services.AddScoped<IJourneyService, JourneyService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IBookingCancellationService, BookingCancellationService>();

builder.Services.AddScoped<ICarrierJourneyProvider, AvantiJourneyProvider>();
builder.Services.AddScoped<ICarrierJourneyProvider, LnerJourneyProvider>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database test
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();

//     Console.WriteLine($"Database provider: {dbContext.Database.ProviderName}");
//     Console.WriteLine($"Database can connect {dbContext.Database.CanConnect()}");
// }

// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Railway API V1");
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RailwayDbContext>();
    await RailwaySeedData.SeedAsync(dbContext);
}

app.Run();