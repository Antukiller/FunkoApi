using FunkoApiV._2.Infrastructure;
using FunkoApiV._2.Repository;
using FunkoApiV._2.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args); 

builder.Services.AddControllers(); 


builder.Services.AddDependencies();


var app = builder.Build(); 

app.UseHttpsRedirection(); 

app.MapControllers(); 

app.Run();