using ClassLibrary.Services;
using BlazorWebApp.Components;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

// Add services to the container.
_ = builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();
_ = builder.Services.AddScoped<IWeatherForecastService, WeatherForecastServiceEx>();
_ = builder.Services.AddScoped<IMovieReviewService, MovieReviewService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if( !app.Environment.IsDevelopment() )
{
	_ = app.UseExceptionHandler( "/Error", createScopeForErrors: true );
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	_ = app.UseHsts();
}

_ = app.UseHttpsRedirection();

_ = app.UseStaticFiles();
_ = app.UseAntiforgery();

_ = app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();