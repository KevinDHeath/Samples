global using ClassLibrary.Models;
global using ClassLibrary.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

// Add services to the container.

_ = builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
_ = builder.Services.AddEndpointsApiExplorer();
_ = builder.Services.AddSwaggerGen();
_ = builder.Services.AddScoped<IWeatherForecastService, WeatherForecastServiceEx>();
_ = builder.Services.AddScoped<IMovieReviewService, MovieReviewService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if( app.Environment.IsDevelopment() )
{
	_ = app.UseSwagger();
	_ = app.UseSwaggerUI();
}

_ = app.UseHttpsRedirection();

_ = app.UseAuthorization();

_ = app.MapControllers();

app.Run();