using Akka.AccountServer.AkkaDefine;
using Akka.AccountServer.DB;
using Akka.Actor;

using Google.Protobuf.WellKnownTypes;

using Microsoft.EntityFrameworkCore;

namespace Akka.AccountServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            #region ����ȯ�� ���� ������ 
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; //���̽����� ���� �� ��ҹ��� ���� (PascalCase)
            });

            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(7022, listenOptions =>
                {
                    listenOptions.UseHttps();  // ���ÿ����� HTTPS ���
                });

                options.ListenLocalhost(5181, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });
            #endregion

            #region db 설정
            builder.Services.AddDbContext<AppDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            #endregion

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Akka
            // DI���  Controller.cs���� �����ڷ� ���� �� ����
            builder.Services.AddSingleton<IActorBridge, AkkaService>();

            // AkkaService ���� (Ŭ������ ���� ����)
            builder.Services.AddHostedService<AkkaService>(sp => (AkkaService)sp.GetRequiredService<IActorBridge>());
            #endregion

            var app = builder.Build();

            // 애플리케이션이 시작될 때 DB 연결을 미리 초기화하는 작업
            await InitializeDatabaseConnectionAsync(app.Services);
        
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
        // DB 연결 미리 열기
        static async Task InitializeDatabaseConnectionAsync(IServiceProvider services)
        {
            // 서비스 스코프를 만들어 DbContext를 생성.
            using (var scope = services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // DB 연결을 미리 열어두기
                try
                {
                    await dbContext.Database.OpenConnectionAsync();
                    //await dbContext.Database.MigrateAsync();  // 마이그레이션을 자동으로 실행

                    Console.WriteLine("DB 연결 초기화 완료");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB 연결 초기화 실패: {ex.Message}");
                }
            }
        }
    }
}
