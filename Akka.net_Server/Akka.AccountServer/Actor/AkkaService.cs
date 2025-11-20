using Akka.Actor;
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
using Akka.DependencyInjection;

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
        private readonly IHostApplicationLifetime _applicationLifetime;

        Dictionary<ActtorType, IActorRef> _actorRefs = new Dictionary<ActtorType, IActorRef>();

        public AkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifetime)
        {
            _serviceProvider = serviceProvider;
            _applicationLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("hocon.conf"));

            // Akka 부트스트랩 구성
            var bootstrap = BootstrapSetup
                .Create()
                .WithConfig(config)
                .WithActorRefProvider(ProviderSelection.Cluster.Instance);

            // !!! 여기만 변경 !!!
            var diSetup = ServiceProviderSetup.Create(_serviceProvider);

            // Setup 병합
            var actorSystemSetup = bootstrap.And(diSetup);

            _serverActorSystem = ActorSystem.Create(
                Enum.GetName(ActtorType.ClusterSystem),
                actorSystemSetup
            );

            // 액터 등록
            _actorRefs.Add(
                ActtorType.AccountActor,
                _serverActorSystem.ActorOf(
                    Props.Create(() => new AccountActor(_serverActorSystem)),
                    Enum.GetName(ActtorType.AccountActor)
                )
            );

            _serverActorSystem.WhenTerminated.ContinueWith(_ =>
            {
                _applicationLifetime.StopApplication();
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
