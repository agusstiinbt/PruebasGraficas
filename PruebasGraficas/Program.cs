using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PruebasGraficas;
using PruebasGraficas.Classes.Validator.Employee;
using PruebasGraficas.Classes.Validator.Vehicle;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 🌐 HttpClient
builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ✅ FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<EmployeeCreateEditModelValidator>();

// ✅ MudBlazor
builder.Services.AddMudServices();

// 🔥 LOCALIZATION (esto es lo que te faltaba)
builder.Services.AddLocalization();


var host = builder.Build();

// 🌍 Configurar cultura (Blazor WASM necesita JSInterop)
var js = host.Services.GetRequiredService<IJSRuntime>();

var reponse = CultureInfo.GetCultures;

var cultureInfo = CultureInfo.GetCultureInfo("en-GB");


CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

await host.RunAsync();