using API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration); // Custom application services
builder.Services.AddIdentityServices(builder.Configuration);    // Identity and authentication services

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Enables Swagger in development
    app.UseSwaggerUI(); // Enables the Swagger UI
}

// Middleware order matters
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS (should come early in the pipeline)

app.UseCors(x => x.AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("http://localhost:4200", "https://localhost:4200")); // CORS middleware

app.UseAuthentication(); // Auth middleware to validate tokens or credentials
app.UseAuthorization();  // Checks access rights after authentication

app.MapControllers(); // Maps controller endpoints to the pipeline

app.Run();
