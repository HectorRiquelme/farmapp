using CommunityToolkit.Maui;
using FarmApp.Application;
using FarmApp.Constants;
using FarmApp.Domain.Interfaces;
using FarmApp.Domain.Services;
using FarmApp.Infrastructure.Api;
using FarmApp.Infrastructure.Cache;
using FarmApp.Infrastructure.Location;
using FarmApp.Presentation.Pages;
using FarmApp.Presentation.ViewModels;
using Microsoft.Extensions.Logging;

namespace FarmApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // --- HTTP ---
        builder.Services.AddHttpClient<MinSalApiService>(client =>
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(AppConstants.ApiTimeoutSegundos + 2);
        });

        builder.Services.AddHttpClient<GeocodingService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", AppConstants.NominatimUserAgent);
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        // --- Dominio ---
        builder.Services.AddSingleton<AperturaService>();
        builder.Services.AddSingleton<GeoDistanciaService>();

        // --- Infraestructura ---
        builder.Services.AddSingleton<IFarmaciaProvider, MinSalApiService>();
        // DatabaseConnection debe registrarse antes que los repositorios que la consumen
        builder.Services.AddSingleton<DatabaseConnection>();
        builder.Services.AddSingleton<IFarmaciaRepository, FarmaciaRepository>();
        builder.Services.AddSingleton<IGeoCacheRepository, GeoCacheRepository>();
        builder.Services.AddSingleton<ILocationService, MauiLocationService>();
        // GeocodingService ya registrado vía AddHttpClient arriba — no duplicar

        // --- Casos de uso ---
        builder.Services.AddTransient<BuscarFarmaciasUseCase>();

        // --- ViewModels ---
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ResultadosViewModel>();
        builder.Services.AddTransient<DetalleFarmaciaViewModel>();

        // --- Páginas ---
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ResultadosPage>();
        builder.Services.AddTransient<DetallePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
