using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;
using OrusFinancas.Services;
using OrusFinancas.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// SQLServer
builder.Services.AddDbContext<Contexto>(options =>
    options.UseSqlServer(
        "Server=(localdb)\\mssqllocaldb;Database=NomeDoBanco;Trusted_Connection=True;")
);

builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login"; // Define a URL para redirecionar em caso de não autenticado
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Opcional
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Duração do Cookie
        options.SlidingExpiration = true; // Renova o cookie automaticamente
    });
    
// Registrar todos os serviços necessários
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<OrcamentoService>();
builder.Services.AddScoped<InsightFinanceiroService>();
builder.Services.AddScoped<AssinaturaService>();
builder.Services.AddScoped<SeedDataService>(); // Novo serviço

// Registrar o serviço de background
builder.Services.AddHostedService<TarefasAutomaticasService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Em desenvolvimento, mostrar detalhes dos erros
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
