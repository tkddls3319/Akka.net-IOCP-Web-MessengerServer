🔥 **프로젝트 진행 중!** 🚀
## 개발 체크리스트
- [x] IOCP 채팅 서버 
- [x] Akka 채팅 서버
- [x] Cluster LogServer
- [x] Client 콘솔
- [x] Protobuf패킷 자동화
- [x] IOCP 라이브러리
- [x] 채팅 룸별 로그 JSon으로 Serialize
- [x] 채팅 룸별 로그 Deserialize
- [x] 채팅 시간 추가
- [x] 회원가입 디비
- [x] Web API Server(ASP.NET) 로그인용
- [ ] Web API Server(ASP.NET) Server와 Actor로 통신
- [ ] Client Unity

# Akka.NET + IOCP Server + ASP.NET

Akka.NET과 IOCP(Input/Output Completion Port)를 결합하여 **고성능 메신저 채팅 서버**를 개발 중입니다. 현재 설계 및 구현 방안을 고민하며, 확장성과 유지보수성을 강화하는 데 중점을 두고 있습니다.

추 후 기회가 된다면 클라이언트는 콘솔이 아닌 Unity나 머 WPF나 다른 걸로 개발해 볼 예정.

채팅 서버를 목표로 하지만 채팅을 일반적인 패킷으로 본다면 해당 서버를 베이스로 다양한 분야에서 사용할 수 있을 것 같음. 

![chat](https://github.com/user-attachments/assets/f3b8ecfc-7cdd-4dc9-8eab-a629436231b2)



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
    
---

## 주요 특징

### 주요 사용 기술
**Akka.net, IOCP, WebAPI, Json, protobuf, EntityFrameWork, Cluster, Serilog, bat파일, MMO 등**

### 1. Akka.NET 프로젝트 진행전 만든 설명 블로그
- [Akka.NET 기본 설명 1](https://usingsystem.tistory.com/545)
- [Akka.NET 기본 설명 2](https://usingsystem.tistory.com/547)
- [Akka.NET 기본 설명 3](https://usingsystem.tistory.com/548)
- [Akka.NET 기본 설명 4](https://usingsystem.tistory.com/549)

### 2. 테스트 방법
#### Visual Studio 빌드로 테스트하기
1. 솔루션 선택 후 **속성** -> **여러 시작 프로젝트**를 선택.
2. `Akka.Server`, `Akka.LogServer`, Akka.AccountServer, `DummyClient` 작업 시작으로 설정.
3. Library->PacketGenerator->빌드 ( **Akka.Server가 빌드되면 빌디 전 이벤트로 GenProto.bat파일이 실행됩니다. 해당 배치 파일은 PacketGenerator.exe를 실행 시키기 때문에 빌드를 해놓지 않으면 오류가 날 수 있습니다.**)
4. F5 키를 눌러 실행. ( **만약 Akka.Server.csproj안에 <Exec Command="CALL $(SolutionDir)Protobuf\protoc-3.12.3-win64\bin\GenProto.bat" />에서 오류가 난다면 그냥 지우고 빌드 해도 됩니다. Akka.Server 빌드 전 이벤트 경로 문제일 가능성이 큽니다.** )
5. 'DummyClient'가 켜지면 회원가입 및 로그인 먼저 진행
6. 로그인 하면 채팅룸 선택창
7. 채팅룸 선택하면 그동안 채팅 룸 에서 채팅 했던 기록이 먼저 뜸
8. `DummyClient`에서 키보드 입력으로 채팅 메시지 전송.
   - `DummyClient.exe`를 여러 개 실행하면 멀티 채팅 테스트 가능.
9. `Akka.LogServer`의 Debug or Release 폴더에서 채팅 룸 별 로그 확인.

#### 추가 설정
- 방(Room) 안에는 클라이언트 5명만 입장 가능.
- `RoomManagerActor`의 `AddClientToRoomHandler`에서 설정 변경 가능.

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
  - Serilog를 사용해 로그 작성.
  - 룸별로 채팅을 읽어 Server에 전달.

### 3. Akka.Protocol.Shared
- **역할**: 공통 Protobuf 정의를 공유.
- **구성**: 모든 프로젝트에서 참조되는 `Protocol.cs` 포함.

### 4. DummyClient
- **역할**: 채팅 클라이언트.
- **기능**:
  - `Akka.Server`와 비동기 TCP 통신 수행.

### 5. ServerCore
- **역할**: `Akka.Server`와 `DummyClient` 간 TCP 통신 지원 라이브러리.
- **기능**: IOCP 기반의 TCP 통신 로직 구현.
  
### 6. Akka.AccountServer
- **역할**: Client의 회원가입과 로그인 관리
- **기능**: REST API와 EntityFrameWork Mssql로 구현, Cluster중 하나로 Akka.Server와 actor 통신

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

## 프로젝트 목적
1. **Akka.NET을 활용한 고성능 서버 개발.**
2. **HOCON 설정을 통한 Akka Cluster 구성.**
3. **Akka.Remote를 사용한 원격 액터 간 통신.**
4. **IOCP 기반 TCP/IP 소켓 통신 구현.**
