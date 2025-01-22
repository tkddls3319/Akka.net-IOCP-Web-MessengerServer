using Akka.AccountServer.DB;
using Microsoft.EntityFrameworkCore;

namespace Akka.AccountServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; //제이슨으로 보낼 떄 대소문자 유지 (PascalCase)
            });

            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(7022, listenOptions =>
                {
                    listenOptions.UseHttps();  // 로컬에서도 HTTPS 허용
                });

                options.ListenLocalhost(5181, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            #region db
            builder.Services.AddDbContext<AppDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            #endregion

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
}
