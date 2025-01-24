using Akka.Actor;
using Akka.Cluster.Hosting;
using Akka.DependencyInjection;
using Akka.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor.Setup;
using Akka.Routing;
using Microsoft.AspNetCore.SignalR;
using Akka.AccountServer.Controllers;
using Akka.AccountServer.Actor;
using Akka.AccountServer.Define;

namespace Akka.AccountServer.AkkaDefine
{
    public interface IActorBridge
    {
        void Tell(ActtorType type, object message);  
        Task<T> Ask<T>(ActtorType type, object message);  
    }
    public sealed class AkkaService : IHostedService, IActorBridge
    {
        private ActorSystem _serverActorSystem;
        private readonly IServiceProvider _serviceProvider;

        Dictionary<ActtorType, IActorRef> _actorRefs = new Dictionary<ActtorType, IActorRef>();

        private readonly IHostApplicationLifetime _applicationLifetime;

        public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime)
        {
            _serviceProvider = serviceProvider;
            _applicationLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));

            // Akka.NET 시스템 설정 부트스트랩
            var bootstrap = BootstrapSetup.Create()
                .WithConfig(config)  // HOCON 설정을 적용하고 환경 변수를 주입
                .WithActorRefProvider(ProviderSelection.Cluster.Instance); // Akka.Cluster를 활성화 (클러스터 모드 설정)

            // ASP.NET Core의 의존성 주입(DI)을 Akka.NET 액터 시스템과 통합
            var diSetup = ServiceProviderSetup.Create(_serviceProvider);

            // 모든 설정을 하나로 병합 (부트스트랩 + DI 설정)
            var actorSystemSetup = bootstrap.And(diSetup);

            _serverActorSystem = ActorSystem.Create(Enum.GetName(ActtorType.ClusterSystem), actorSystemSetup);

            //actor 정의
            {
                // HOCON 설정 기반 라우터 생성 (tasker 라우터는 HOCON에서 정의됨)
                /*
                var router = _serverActorSystem.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), "tasker");

                // router 액터를 CommandProcessor 액터의 생성자에서 주입받아 생성
                var processor = _serverActorSystem.ActorOf(
                    Props.Create(() => new CommandProcessor(router)),
                    "commands"

                 _actorRef = _actorSystem.ActorOf(Worker.Prop(), "heavy-weight-word");
                
                );
                */
                _actorRefs.Add(ActtorType.AccountActor, _serverActorSystem.ActorOf(Props.Create(() => new AccountActor(_serverActorSystem)), Enum.GetName(ActtorType.AccountActor)));
            }

            _serverActorSystem.WhenTerminated.ContinueWith(tr => {
                _applicationLifetime.StopApplication();  // ActorSystem이 종료되면 애플리케이션도 종료하도록 보장, 종료 시 애플리케이션 종료
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CoordinatedShutdown.Get(_serverActorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
        }

        public void Tell(ActtorType type, object message)
        {
            _actorRefs[type].Tell(message);
        }

        public Task<T> Ask<T>(ActtorType type, object message)
        {
            return _actorRefs[type].Ask<T>(message);
        }
    }
}
