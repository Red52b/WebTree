using Microsoft.EntityFrameworkCore;
using WebTree.DbObjects;

namespace WebTree;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        
        // Add services to the container.
        builder.Services.AddSingleton<Logic.Forest>();
        builder.Services.AddSingleton<Logic.LogReader>();
        builder.Services.AddScoped<Logic.RequestEventForLog>();
        string connectionStr = builder.Configuration.GetConnectionString("DbConnection");
        builder.Services.AddDbContextFactory<AppDbContext>(opt=> opt.UseNpgsql(connectionStr));
        builder.Services.AddControllers();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(sw=>sw.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "WebTree.xml")));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}