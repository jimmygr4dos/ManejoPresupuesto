using ManejoPresupuesto.Models;
using ManejoPresupuesto.Repositories;
using ManejoPresupuesto.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<ITiposCuentasRepository, TiposCuentasRepository>();
builder.Services.AddTransient<ICuentasRepository, CuentasRepository>();
builder.Services.AddTransient<ICategoriasRepository, CategoriasRepository>();
builder.Services.AddTransient<ITransaccionesRepository, TransaccionesRepository>();
builder.Services.AddTransient<IUsersRepository, UsersRepository>();
builder.Services.AddTransient<IUsersService, UsersService>();
builder.Services.AddTransient<IReportService, ReportService>();

builder.Services.AddTransient<IUserStore<Usuario>, UserStore>();
builder.Services.AddIdentityCore<Usuario>(opciones =>
{
    //opciones.Password.RequireDigit = false;
    //opciones.Password.RequireLowercase = false;
    //opciones.Password.RequireUppercase = false;
    //opciones.Password.RequireNonAlphanumeric = false;
}).AddErrorDescriber<MensajesDeErrorIdentity>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(typeof(Program));

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
    pattern: "{controller=Transacciones}/{action=Index}/{id?}");

app.Run();
