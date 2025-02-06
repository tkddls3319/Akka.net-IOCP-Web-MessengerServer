🔥 **프로젝트 진행 중!** 🚀
## 개발 체크리스트
- [x] IOCP + Akka 채팅 서버
- [x] Cluster LogServer
- [x] Client 콘솔
- [x] Protobuf패킷 자동화
- [x] IOCP socket통신 라이브러리화 하기
- [x] 채팅 룸별 로그 JSon으로 Serialize, Deserialize( protobuf.time을 위한 newtonjson 커스텀 )
- [x] AccountServer 개발 (Web API Server(ASP.NET)) 
- [x] AccountServer를 Akka.net Cluster 적용해 채팅 서버와 통신하기
- [x] Entity framework Mssql으로 DB 개발
- [x] 대규모 클라이언트 테스트 ( 10,000명까지 테스트해봄 )
- [ ] Client Unity로 개발하기

# Akka.NET + IOCP Server + ASP.NET

Akka.NET과 IOCP(Input/Output Completion Port)를 결합하여 **고성능 메신저 채팅 서버**를 개발 중입니다. 현재 설계 및 구현 방안을 고민하며, 확장성과 유지보수성을 강화하는 데 중점을 두고 있습니다.

추 후 기회가 된다면 클라이언트는 콘솔이 아닌 Unity나 머 WPF나 다른 걸로 개발해 볼 예정.

채팅 서버를 목표로 하지만 채팅을 일반적인 패킷으로 본다면 해당 서버를 베이스로 다양한 분야에서 사용할 수 있을 것 같음. 

![chat](https://github.com/user-attachments/assets/360c1989-9bbb-423a-9c57-e95a4b07c286)

---
## 프로젝트 목표
1. **Akka.NET으로 서버 개발**
2. **HOCON 설정을 통한 Akka Cluster 구성.**
3. **Akka.Remote를 사용한 원격 액터 간 통신.**
4. **IOCP 기반 TCP/IP 소켓 통신 구현.**
5. **ASP.NET을 사용한 Web API Server 구현**
6. **Entity Framework를 사용한 DB구현**

### 주요 사용 기술
**Akka.net, IOCP, WebAPI, Json, protobuf, EntityFrameWork, Cluster, Serilog, bat파일, MMO 등**

---
## 테스트 방법
#### Visual Studio 빌드로 테스트하기
**ServerCore안 Connector**
1. 솔루션 선택 후 **속성** -> **여러 시작 프로젝트**를 선택.
2. `Akka.Server`, `Akka.LogServer`, Akka.AccountServer, `DummyClient` 작업 시작으로 설정.
3. Library->PacketGenerator->빌드 ( **Akka.Server가 빌드되면 빌디 전 이벤트로 GenProto.bat파일이 실행됩니다. 해당 배치 파일은 PacketGenerator.exe를 실행 시키기 때문에 빌드를 해놓지 않으면 오류가 날 수 있습니다.**)
4. F5 키를 눌러 실행. ( **만약 Akka.Server.csproj안에 <Exec Command="CALL $(SolutionDir)Protobuf\protoc-3.12.3-win64\bin\GenProto.bat" />에서 오류가 난다면 그냥 지우고 빌드 해도 됩니다. Akka.Server 빌드 전 이벤트 경로 문제일 가능성이 큽니다.** )
5. 메뉴선택
   - 'DummyClient'가 켜지면 회원가입 및 로그인 먼저 진행
   - 대규모 채팅 테스트를 먼저 하고 싶으면 회원가입 안해도됨. ( 998명의 채팅인원 생성되며 방안에 100명씩 존재 )
   - 대규모 채팅 테스트 진행 후 새로운 DummyClient를 실행하고 원하는 채팅방에 들어가면됨.
   - 채팅방이 많기 때문에 채팅창 20개 정도만 출력되지만 콘솔창을 늘리고 방향키를 눌르면 채팅창이 콘솔창 크기에 맞게 보여짐
7. 로그인 하면 채팅룸 선택창
8. 채팅룸 선택하면 그동안 채팅 룸 에서 채팅 했던 기록이 먼저 뜸
9. `DummyClient`에서 키보드 입력으로 채팅 메시지 전송.
   - `DummyClient.exe`를 여러 개 실행하면 멀티 채팅 테스트 가능.
10. `Akka.LogServer`의 Debug or Release 폴더에서 채팅 룸 별 로그 확인.
11. 채팅방에서 ESC를 입력 후 Entrer key를 누르면 채팅방에서 나옴

#### 추가 설정
- 방(Room) 안에는 클라이언트 100명만 입장 가능.
- 클라이언트 입장 최대 인원은 `Akka.Server`의 `Define`에 'RoomMaxCount' 설정 변경 가능.

---

## 프로젝트 구성

### 1. Akka.Server
- **역할**: 클러스터 중심 Seed-Node로서 채팅 서버 역할.
- **기능**:
  - `DummyClient`와 TCP/IP 소켓 통신 수행.
  - `Akka.LogServer` 노드에 채팅 기록 전달.
  - Google Protobuf를 사용한 데이터 직렬화/역직렬화.

### 2. Akka.LogServer
- **역할**: 채팅 기록 관리 서버.
- **기능**:
  - `Akka.Server`에서 받은 채팅을 채팅룸 별로 .json으로 기록 저장.
  - Serilog를 사용해 로그 작성. ( 바로 바로 로그를 남기는 것 이아닌 로그 모아서 한 번에 쓰는 방식으로 성능 향상 )
  - 룸별로 채팅을 읽어 Server에 전달. ( message는 12800kb를 넘으면 보낼 수 없음. 너무 많은 채팅 기록이 있다면 전송 안되게 함. ) 
  
### 3. Akka.AccountServer
- **역할**: Client의 회원가입과 로그인 관리
- **기능**: REST API와 EntityFrameWork Mssql로 구현, Cluster중 하나로 Akka.Server와 actor 통신

### 4. DummyClient
- **역할**: 채팅 클라이언트.
- **기능**: `Akka.Server`와 비동기 TCP 통신 수행., 로그인, 회원가입, 멀티테스트 

### 개발한 라이브러리 폴더
### 1. Akka.Protocol.Shared
- **역할**: 공통 Protobuf 정의를 공유.
- **구성**: 모든 프로젝트에서 참조되는 `Protocol.cs` 포함.

### 2. ServerCore
- **역할**: `Akka.Server`와 `DummyClient` 간 TCP 통신 지원 라이브러리.
- **기능**: IOCP 기반의 TCP 통신 로직 구현., Connector클래스 안에 Connect의 파라미터 count를 증가시키면 멀티 테스트 인원 변경 가능함.

### 3. Akka.ClusterCore
- **역할**: `Akka를 사용하는 Cluster`간 액터를 관리하며 메세지를 보내기 위한 라이브러리.
- **기능**: 클러스터 Actor 재사용, 클러스터에 Actor를 찾아 있으면 전송.
---

## 프로젝트 개요

이 서버는 다음과 같은 구조를 가지고 있습니다:

- **Akka.NET 기반 설계**
  - 액터 모델(Actor Model)을 활용한 비동기 메시지 기반 설계.
  - 클라이언트 연결과 방(Room) 관리 로직을 명확히 분리.

- **Cluster와 Router 설계**
  - 채팅룸에서 발생하는 채팅을 Json으로 기록하는 Cluster를 설계.
  - 채팅을 기록하거나 읽는 Actor를 Router로 설계하여 분산 처리.
     
- **IOCP 기반 네트워크 처리**
  - 비동기 소켓 통신으로 대량의 클라이언트 연결을 효율적으로 처리.
  - `Listener`와 `Session` 클래스를 통해 클라이언트의 데이터 송수신과 연결 해제 관리.

- **확장 가능한 설계**
  - 클라이언트 수에 따라 동적으로 방(Room)을 생성하고 부하를 분산.
  - 독립적인 액터로 설계된 세션 관리와 방 관리.

 - **Web API Server**
   - 클라이언트의 회원가입 및 로그인 담당.

 - **Entity Framework Mssql**
   - DB를 EF로 개발하여 Web Server에서 사용
   

### Akka.NET 프로젝트 진행전 만든 설명 블로그
- [Akka.NET 기본 설명 1](https://usingsystem.tistory.com/545)
- [Akka.NET 기본 설명 2](https://usingsystem.tistory.com/547)
- [Akka.NET 기본 설명 3](https://usingsystem.tistory.com/548)
- [Akka.NET 기본 설명 4](https://usingsystem.tistory.com/549)

---
## 사용한 Message 네이밍 컨벤션

| 메시지 유형 | 예제 | 설명 |
|------------|--------------------|------------------------------|
| **Command (명령)** | `CreateRoomCommand`, `AddClientCommand` | 액터에게 특정 동작을 요청하는 메시지 |
| **Event (이벤트)** | `RoomCreatedEvent`, `ClientAddedEvent` | 상태 변경을 다른 액터에게 알리는 메시지 |
| **Query (쿼리)** | `GetRoomInfoQuery`, `GetClientListQuery` | 데이터를 조회하는 요청 메시지 |
| **Response (응답)** | `RoomInfoResponse`, `ClientListResponse` | 쿼리 요청에 대한 응답 메시지 |


---

## Protobuf 자동화

### Protobuf 관련 파일 위치
- **Protobuf 폴더**: `Akka.net_Server/Protobuf`
- 주요 파일:
  - `protobuf.proto`: 서버와 클라이언트 간 송수신 패킷 정의.
  - `ClusterProtocol.proto`: 클러스터 간 송수신 패킷 정의.

### 자동화 설명
- 기본적으로 Akka.Server를 빌드를 하면 자동으로 GenProto.bat를 실행시키게 만들어놈. ( 빌드 전 이벤트 적용 )
- `GenProto.bat` 실행:
  1. `protobuf.proto`와 `ClusterProtocol.proto`를 읽어 .cs 파일 생성.
  2. 생성된 파일을 아래와 같이 복사:
     - `protobuf.cs`: `DummyClient`와 `Akka.Server` 프로젝트의 `Packet` 폴더.
     - `ClusterProtocol.cs`: `Akka.Protocol.Shared` 폴더.
     
### PacketGenerator 프로젝트
- 역할: `Protocol.proto`를 기반으로 `ClientPacketManager.cs`와 `ServerPacketManager.cs` 생성.
- 파일 복사:
  - `ClientPacketManager.cs`, `ServerPacketManager.cs`를 각각 `DummyClient`와 `Akka.Server`의 `Packet` 폴더에 복사.

---

## Akka 에서의 글로벌에 대한 고찰

1. ActorSelection을 사용하여 특정 액터를 찾는 문제
ActorSelection은 경로(Path) 기반으로 액터를 찾음 → 탐색 비용이 발생.
이를 해결하기 위해 싱글톤 캐싱 방식으로 한 번만 ActorSelection을 실행하고, 이후 캐시된 IActorRef를 사용.
하지만 최초 ActorSelection 실행 시 lock이 필요함.
2. Akka.NET에서 lock을 최소화하려는 이유
액터 모델을 사용하는 주요 이유 중 하나는 lock을 줄여 동시성 비용을 줄이는 것.
하지만 싱글톤 방식으로 ActorSelection을 캐싱할 경우, lock을 사용할 필요가 있음.
→ 이 방식은 액터 모델의 철학과 충돌할 수 있음.
3. lock을 사용하지 않고 ActorSelection 문제를 해결하는 방법
대안 1: Ask<T>()와 PipeTo() 활용하기

ActorSelection을 비동기로 실행하고 ResolveOne()을 통해 IActorRef를 캐싱.
이후 PipeTo(Self)를 사용하여 액터 내부에서 비동기 결과를 처리.
var actorSelection = Context.ActorSelection("/user/SomeActor");
actorSelection.ResolveOne(TimeSpan.FromSeconds(5))
    .PipeTo(Self, success: actorRef => new StoreActorRef(actorRef));
이 방식은 lock 없이 IActorRef를 안전하게 캐싱 가능.

4. 싱글톤 액터를 직접 만드는 문제 (비동기 처리 문제 발생)
싱글톤 액터를 만들면 특정 액터가 중앙 집중화됨.
Ask<T>()를 통해 응답을 받아야 하는 경우 비동기 호출이 중첩되면서 "비동기 지옥"에 빠질 가능성이 있음.

---

## Akka 직렬화에 대한 고찰

### 1. Protobuf와 사용자 정의 프로토콜
- 기존 IOCP 서버에서 Protobuf 직렬화 및 사용자 정의 프로토콜 사용 경험을 활용.

### 2. Akka 메시지 직렬화 문제
- 동일 프로젝트 내에서는 문제가 없으나, 서로 다른 프로젝트 간 직렬화 시 Manifest 충돌 발생.
- 원인: 각 프로젝트에서 동일한 `Protocol.cs` 파일을 사용해도 어셈블리가 다르기 때문.

### 3. 해결 방법: SerializerWithStringManifest 사용
- `SerializerWithStringManifest`를 상속받아 커스텀 직렬화기 작성.
- HOCON 설정으로 등록하여 Manifest 문제 해결.

#### SerializerWithStringManifest 적용 사례:
1. 서로 다른 Protobuf 정의를 사용하는 경우.
2. 직렬화 포맷이 다른 경우 (예: Protobuf ↔ JSON).
3. 특정 데이터 타입에 대해 커스텀 직렬화가 필요한 경우 (예: `DateTimeOffset`, `decimal`).

---

## 개발하면서 느낀점.

Actor 모델을 사용하여 멀티스레딩 환경에서 대규모 분산 처리를 구현하고, 비동기 방식으로 병렬 처리를 수행하면서 성능이 매우 뛰어나다는 점을 체감했다. 기존에 IOCP와 Job Queue를 활용하던 방식보다 더 안정적이고 확장성이 뛰어난 구조라는 점에서 큰 장점을 느낄 수 있었다.

특히, 클러스터 간 통신이 마치 하나의 프로그램 내에서 실행되는 것처럼 자연스럽게 동작하는 점이 인상적이었다. 별도의 프로세스 간 복잡한 동기화 없이도 Actor 모델을 활용하면 일관된 메시지 기반 통신을 유지할 수 있었으며, 시스템의 유연성이 크게 향상되었다.

하지만, 모든 프로세스를 Actor 모델로 구현해야 한다는 점과 클러스터를 설정하는 과정에서 여러 가지 고려해야 할 사항이 많았다. 처음부터 Actor 모델을 기반으로 전체 시스템을 설계하면 강력한 장점을 누릴 수 있지만, 기존에 개발된 시스템과 연동해야 하는 경우 적용이 어렵다는 한계를 느꼈다.

이를 해결하기 위한 방법으로, Actor 모델을 사용하여 로직을 구현하고, 프로세스 간 통신은 gRPC를 활용하는 방안을 고민했다. 이렇게 하면 Actor 모델의 장점을 살리면서도 기존 시스템과의 연결성을 확보할 수 있을 것으로 보인다.

**향후에는 Actor 모델과 gRPC를 결합한 서버 아키텍처를 개발하여 보다 확장성과 유연성이 뛰어난 구조를 구축할 계획이다.**

---
