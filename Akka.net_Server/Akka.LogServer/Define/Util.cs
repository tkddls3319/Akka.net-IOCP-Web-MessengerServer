using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    public static class Util
    {
        public static string RoomNameing(int roomId)
        {
            string today = DateTime.Now.ToString("yyyyMMdd");
            return $"Room{roomId}_{today}";
        }
    }
}
