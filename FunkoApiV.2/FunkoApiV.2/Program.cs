using FunkoApiV._2.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IFunkoRepository, FunkoRepository>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();