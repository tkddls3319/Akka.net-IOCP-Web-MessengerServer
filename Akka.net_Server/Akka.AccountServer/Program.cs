using Akka.AccountServer.AkkaDefine;
using Akka.AccountServer.DB;
using Akka.Actor;

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

            #region 로컬환경 실행 때문에 
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
            #endregion

            #region db
            builder.Services.AddDbContext<AppDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            #endregion

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Akka
            // DI등록  Controller.cs파일 생성자로 받을 수 있음
            builder.Services.AddSingleton<IActorBridge, AkkaService>();

            // AkkaService 실행 (클러스터 구성 포함)
            builder.Services.AddHostedService<AkkaService>(sp => (AkkaService)sp.GetRequiredService<IActorBridge>());
            #endregion

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
