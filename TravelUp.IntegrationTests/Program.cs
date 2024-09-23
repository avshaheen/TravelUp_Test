using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TravelUp.DataAccess;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("InMemoryDbForTesting"));
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseRouting();
app.MapControllers(); 
app.Run();
