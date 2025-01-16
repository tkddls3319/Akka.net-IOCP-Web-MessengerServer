# Akka.net + IOCP Server
 Akka.net과 IOCP를 합친 Chatting Server를 개발 중 이며 어떻게 설계를 해야할지 고민을 해보고 있습니다.

이 서버는 Akka.NET과 IOCP(Input/Output Completion Port)를 기반으로 설계된 고성능 네트워크 서버입니다. 

Akka.NET 기반의 액터 모델 액터를 활용한 비동기 메시지 기반 설계로, 클라이언트 연결과 방(Room) 관리 로직을 명확히 분리.
SessionManagerActor, RoomManagerActor, RoomActor와 같은 주요 액터가 각각의 책임을 수행하며, 독립적인 상태를 유지를 목표로 하고 있습니다.

IOCP 기반의 네트워크 처리로 비동기 소켓 통신을 사용하여 대량의 클라이언트 연결을 효율적으로 처리.
Listener와 Session 클래스는 클라이언트와의 데이터 송수신, 연결 해제 등 IO 작업을 관리.

확장 가능한 설계
클라이언트를 효율적으로 처리하기 위해 방(Room)을 동적으로 생성하고, 클라이언트 수에 따라 부하를 분산.
클라이언트 세션 관리와 방 관리를 독립적인 액터로 설계하여 시스템의 확장성과 유지보수성을 강화해보려고 여러가지를 시도 중 입니다.

Akka.net 기본 설명 블로그

https://usingsystem.tistory.com/545

https://usingsystem.tistory.com/547

https://usingsystem.tistory.com/548

https://usingsystem.tistory.com/549

# Akka.Server 프로젝트
데이터 송수신: Google Protobuf를 사용.

# 프로젝트 목적
Akka를 활용한 서버 개발.
HOCON 설정으로 Akka Cluster 구성.
Akka Remote를 사용한 원격 액터 간 통신.
통신 방식: IOCP 서버를 통해 클라이언트와 TCP/IP 소켓 통신.

# 프로젝트 설명

1. Akka.Server

역할: 클러스터의 중심 역할을 담당하는 Seed-Node.로 DummyClient와 소켓통신을 하며 채팅서버 역할 수행. Akka.LogServer 노드에 채팅 기록을 전달

구현 내용: IOCP 서버를 통해 DummyClient와 TCP/IP 소켓 통신. Google Protobuf를 사용하여 데이터를 직렬화 및 역직렬화.

2. Akka.LogServer

역할: Akka.Server에서 보내는 채팅 기록을 관리 및 처리 담당 서버.

구현 내용: Akka를 활용하여 로그 수집 및 저장 로직 구현. 로그는 Serilog를 사용해서 기록록

3. Akka.Protocol.Shared

역할: 공통 Protobuf 정의를 공유하기 위한 라이브러리.

구현 내용: Protocol.cs를 포함하며, 모든 프로젝트에서 참조하여 사용.

4. DummyClient

역할: 채팅 클라이언트 역할.

구현 내용: Akka.Server와 비동기적으로 TCP 통신 수행. 

5. ServerCore

역할: Akka.Server와 DummyClient 간의 소켓 통신을 위한 기본 라이브러리.

구현 내용: IOCP 기반의 TCP 통신 로직 구현.

# Akka 직렬화에 대한 고찰
1. Google Protobuf와 사용자 설정 프로토콜
기존 IOCP 서버에서 사용자 정의 프로토콜 + Protobuf 직렬화 방식을 사용한 경험이 있어, 이를 다시 구현하는 것은 어렵지 않았습니다.
2. Akka의 메시지 직렬화
Tell(new 000()) 방식으로 메시지를 전달할 때, 서로 다른 프로젝트 간 직렬화 및 역직렬화 문제를 고민했습니다.
동일 프로젝트에서는 문제가 없었지만, 다른 프로젝트에서는 Manifest가 다르다는 문제가 발생했습니다.
3. Akka.NET 직렬화 오류
Akka.NET 공식 문서에 따르면, Protobuf 직렬화는 기본적으로 제공됩니다:
"Akka.NET provides serializers for POCO's (Plain Old C# Objects) and for Google.Protobuf.IMessage by default, so you don't usually need to add configuration for that."
그러나, 다른 프로젝트 간 Protobuf 메시지 전달 시 직렬화 오류가 발생했습니다.
원인: 동일한 Protocol.cs 파일을 사용하더라도, 각 프로젝트의 어셈블리가 다르기 때문에 Manifest가 달라져 오류가 발생.
4. SerializerWithStringManifest를 사용한 직렬화
SerializerWithStringManifest를 상속받아 커스텀 직렬화기 작성:
커스텀 Serializer를 구현하고 HOCON에 등록하여 Manifest 문제를 해결.
SerializerWithStringManifest를 사용하는 사례:
서로 다른 Protobuf 정의를 사용하는 경우.
서로 다른 직렬화 포맷을 사용하는 경우(예: 한쪽은 Protobuf, 다른 쪽은 JSON).
특정 타입의 커스텀 직렬화가 필요한 경우(예: DateTimeOffset, decimal 등).
DTO 설계 원칙 준수: 데이터와 로직 분리를 위해 사용.


