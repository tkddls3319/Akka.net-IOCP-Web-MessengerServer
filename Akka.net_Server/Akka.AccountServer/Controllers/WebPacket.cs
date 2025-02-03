
public class CreateAccountPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}
public class CreateAccountPacketRes
{
    public bool CreateOk { get; set; }
}
public class LoginAccountPacketReq
{
    public string AccountName { get; set; }
    public string Password { get; set; }
}
public class RoomInfo
{
    public int RoomId { get; set; }
    public int MaxCount { get; set; }
    public int CurrentCount { get; set; }
}

public class LoginAccountPacketRes
{
    public bool LoginOk { get; set; }
    public int AccountId { get; set; }
    public List<RoomInfo> RoomList { get; set; } = new List<RoomInfo>();
}