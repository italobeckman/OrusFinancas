using Microsoft.EntityFrameworkCore;
using OrusFinancas.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// SQLServer

builder.Services.AddDbContext<Contexto>
    (options => options.UseSqlServer("Server=localhost,1433;Database=bancoOrus;User Id=sa;Password=SuaSenhaForte123@;TrustServerCertificate=True;"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
