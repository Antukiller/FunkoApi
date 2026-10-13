using FunkoApiV._2.Infrastructure;

var builder = WebApplication.CreateBuilder(args); 

builder.Services.AddControllers(); 


builder.Services.AddDependencies();


var app = builder.Build(); 

app.UseHttpsRedirection(); 

app.MapControllers(); 

app.Run();