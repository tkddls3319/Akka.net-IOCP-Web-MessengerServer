pushd %~dp0
protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
protoc.exe -I=./ --csharp_out=./ ./ClusterProtocol.proto 
IF ERRORLEVEL 1 PAUSE

START ../../../PacketGenerator/bin/Debug/net8.0/PacketGenerator.exe ./Protocol.proto
XCOPY /Y Protocol.cs "../../../Akka.Protocol.Shared"

XCOPY /Y ClusterProtocol.cs "../../../Akka.Protocol.Shared"

XCOPY /Y ClientPacketManager.cs "../../../DummyClient/Packet"
XCOPY /Y ServerPacketManager.cs "../../../Akka.Server/Packet"
