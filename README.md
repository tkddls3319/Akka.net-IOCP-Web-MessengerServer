# Akka.netServer
 Akka.net과 IOCP를 합친 Chatting Server를 개발 중 이며 어떻게 설계를 해야할지 고민을 해보고 있습니다.

이 서버는 Akka.NET과 IOCP(Input/Output Completion Port)를 기반으로 설계된 고성능 네트워크 서버입니다. 

Akka.NET 기반의 액터 모델 액터를 활용한 비동기 메시지 기반 설계로, 클라이언트 연결과 방(Room) 관리 로직을 명확히 분리.
SessionManagerActor, RoomManagerActor, RoomActor와 같은 주요 액터가 각각의 책임을 수행하며, 독립적인 상태를 유지를 목표로 하고 있습니다.

IOCP 기반의 네트워크 처리로 비동기 소켓 통신을 사용하여 대량의 클라이언트 연결을 효율적으로 처리.
Listener와 Session 클래스는 클라이언트와의 데이터 송수신, 연결 해제 등 IO 작업을 관리.

확장 가능한 설계
클라이언트를 효율적으로 처리하기 위해 방(Room)을 동적으로 생성하고, 클라이언트 수에 따라 부하를 분산.
클라이언트 세션 관리와 방 관리를 독립적인 액터로 설계하여 시스템의 확장성과 유지보수성을 강화해보려고 여러가지를 시도 중 입니다.
