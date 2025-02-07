using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using ServerCore;
using DummyClient.Session;
using Akka.Util;

namespace DummyClient
{
    public class Program
    {
        public static string AccountName { get; set; }
        static bool isLoggedIn = false; // 로그인 상태 플래그
        static bool isSigedIn = false; // 회원가입 상태 플래그

        public static List<RoomInfo> RoomInfos = new List<RoomInfo>();
        static ManualResetEventSlim _resetEvent = new ManualResetEventSlim(false);
        public static bool IsMultitest = false;

        static void Main(string[] args)
        {
            //서버 보다 빨리 켜져서 Log 클러스터가 서버에 붙기 전에 켜짐 그래서 sleep 걸어놈
            if (Environment.GetEnvironmentVariable("VisualStudioEdition") != null)
            {
                Thread.Sleep(3000); // Visual Studio 환경에서만 동작
            }

            #region 컨텐츠 역역입니다. 그렇게 중요하진 않아요. WebManager를 사용해 Web Api와 통신하는 부분만 보셔도 됩니다.
            ManuChoice();
            //로그인성공하면 Server 접속으로 넘어가게 막는용
            while (isLoggedIn == false) { }

            #endregion

            Console.Clear();

            #region 채팅 Server 접속
            string hostName = Dns.GetHostName();

            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            //IPAddress ipAddr = IPAddress.Parse("localhost");
            IPAddress ipAddr = ipEntry.AddressList[1];

            IPEndPoint endPoint = new IPEndPoint(ipAddr, 8888);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); }, IsMultitest);
            #endregion

            if (IsMultitest)
                Task.Run(() => MultiTestSendMessageAsync());

            _resetEvent.Wait();
        }
        static async Task MultiTestSendMessageAsync()
        {
            while (true)
            {
                SessionManager.Instance.FlushAllSessions();
                await Task.Delay(1000); // 1초마다 실행 (비동기)
            }
        }
        static void ManuChoice()
        {
            List<string> menuOptions = new List<string>() { " Sign Up", " Login", "MultiChatTest(player998명 생성)", " Exit." };
            int selectedIndex = 0;

            while (!isLoggedIn)
            {
                Console.Clear();
                Util.DrawBox("=== Welcome to ChatApp ===", menuOptions, selectedIndex);

                ConsoleKey key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        selectedIndex = (selectedIndex == 0) ? menuOptions.Count - 1 : selectedIndex - 1;
                        break;
                    case ConsoleKey.DownArrow:
                        selectedIndex = (selectedIndex == menuOptions.Count - 1) ? 0 : selectedIndex + 1;
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        Console.WriteLine($"선택된 메뉴: {menuOptions[selectedIndex]}");
                        if (selectedIndex == 0)
                        {
                            SignUp();
                            return;
                        }
                        else if (selectedIndex == 1)
                        {
                            Login();
                            return;
                        }
                        else if (selectedIndex == 2)
                        {
                            MultiTest();
                            return;
                        }
                        else if (selectedIndex == 3)
                        {
                            Console.WriteLine("프로그램을 종료합니다.");
                            Environment.Exit(0);
                            return;
                        }
                        break;
                }
            }
        }
        static async void SignUp()
        {
            Console.Clear();
            Util.DrawMessageBox("회원가입", ConsoleColor.Green);
            while (true)
            {
                Console.Write("[아이디를 입력하세요] ");
                string userId = Console.ReadLine();

                Console.Write("[비밀번호를 입력하세요] ");
                string password = Console.ReadLine();

                var signUpInfo = new CreateAccountPacketReq() { AccountName = userId, Password = password };

                CreateAccountPacketRes result = await WebManager.Instance.SendPostRequest<CreateAccountPacketRes>("account/create", signUpInfo);

                if (result.CreateOk)
                {
                    isSigedIn = true; // 회원가입 성공 후 로그인 상태로 설정

                    Login(true);
                }
                else
                {
                    Console.WriteLine("아이디 중복 회원가입 실패! 다시 시도해 주세요.");
                }

                // 회원가입 성공 시 루프 탈출
                if (isSigedIn) break;
            }
        }
        static async void Login(bool signup = false)
        {
            Console.Clear();
            Util.DrawMessageBox(signup ? "회원가입 성공! 로그인 해주세요." : "로그인", ConsoleColor.Blue);

            while (true)
            {
                Console.Write("[아이디를 입력하세요] ");
                string userId = Console.ReadLine();

                Console.Write("[비밀번호를 입력하세요] ");
                string password = Console.ReadLine();

                var info = new LoginAccountPacketReq() { AccountName = userId, Password = password };

                // 로그인 요청 보내기
                var result = await WebManager.Instance.SendPostRequest<LoginAccountPacketRes>("account/login", info);

                if (result.LoginOk)
                {
                    Console.Clear();
                    AccountName = userId;
                    RoomInfos = result.RoomList;
                    isLoggedIn = true; // 로그인 성공 시 메뉴 빠져나가기
                }
                else
                {
                    Console.WriteLine("로그인 실패! 다시 시도해 주세요.");
                    isLoggedIn = false;
                }

                // 로그인 성공 시 루프 탈출
                if (isLoggedIn) break;
            }
        }
        static void MultiTest()
        {
            isLoggedIn = true;
            IsMultitest = true;
        }
    }
}
