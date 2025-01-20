using Google.Protobuf.ClusterProtocol;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Akka.LogServer
{
    #region DataContents
    public interface ILoader { }

    public class ChatLogLoader : ILoader
    {
        public List<ChatObject> ChatLogLoaders = new List<ChatObject>();
    }
    #endregion

    public class DataManager
    {
        public static void LogData()
        {
            //최초 로드하고 싶은게 있다면
        }

        // Loader 인터페이스 사용하는데 Json파일에 Loader이름 있을 경우
        static Loader LoadJson<Loader>(string path) where Loader : ILoader
        {
            string text = File.ReadAllText($"{ConfigManager.Config.logPath}/{path}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
        // JSON 배열 로드 (List<T>)
        public static List<T> LoadJsonArray<T>(string path)
        {
            string jsonArray = ReadAndFormatJson(path, false);

            if (string.IsNullOrEmpty(jsonArray))
                return null;

            try
            {
                return JsonConvert.DeserializeObject<List<T>>(jsonArray);
            }
            catch (JsonException ex)
            {
                throw new Exception("JSON parsing error: " + ex.Message);
            }
        }

        // JSON 객체 로드 (Loader 인터페이스 사용하는데 Json파일에 Loader이름 이없을 경우)
        public static Loader LoadJsonLoader<Loader>(string path) where Loader : ILoader
        {
            string propertyName = typeof(Loader).GetFields().FirstOrDefault()?.Name ??
                     throw new Exception("No properties found in Loader class.");

            string jsonArray = ReadAndFormatJson(path, true, $"{propertyName}");

            try
            {
                return JsonConvert.DeserializeObject<Loader>(jsonArray);
            }
            catch (JsonException ex)
            {
                throw new Exception("JSON parsing error: " + ex.Message);
            }
        }
        // 공통 JSON 로드 메서드
        private static string ReadAndFormatJson(string path, bool wrapWithObject, string objectName = null)
        {
            string pattern = @"\{[^}]+\}";
            string filePath = $"{ConfigManager.Config.logPath}/{path}.json";

            if (!File.Exists(filePath))
                return string.Empty;

            string text;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                text = reader.ReadToEnd();
            }

            MatchCollection matches = Regex.Matches(text, pattern);

            StringBuilder sb = new StringBuilder();

            if (wrapWithObject)
            {
                sb.Append($"{{\"{objectName}\":[");
            }
            else
            {
                sb.Append("[");
            }

            for (int i = 0; i < matches.Count; i++)
            {
                sb.Append(matches[i].Value);
                if (i < matches.Count - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append(wrapWithObject ? "]}" : "]");

            return sb.ToString();
        }


        //static List<T> LoadJson<T>(string path) 
        //{
        //    string pattern = @"\{[^}]+\}";

        //    string text = File.ReadAllText($"{ConfigManager.Config.logPath}/{path}.json");
        //    MatchCollection matches = Regex.Matches(text, pattern);

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append($"[");

        //    for (int i = 0; i < matches.Count; i++)
        //    {
        //        sb.Append(matches[i].Value);
        //        if (i < matches.Count - 1)
        //        {
        //            sb.Append(",");
        //        }
        //    }

        //    sb.Append("]");
        //    string jsonArray = sb.ToString();

        //    try
        //    {
        //        var a = JsonConvert.DeserializeObject<List<T>>(jsonArray);

        //        return null;
        //    }
        //    catch (JsonException ex)
        //    {
        //        throw new Exception("JSON parsing error: " + ex.Message);
        //    }
        //}
        //static Loader LoadJson<Loader>(string path) where Loader : ILoader
        //{
        //    string pattern = @"\{[^}]+\}";

        //    string text = File.ReadAllText($"{ConfigManager.Config.logPath}/{path}.json");
        //    MatchCollection matches = Regex.Matches(text, pattern);

        //    StringBuilder sb = new StringBuilder();
        //    sb.Append($"{{\"{typeof(Loader).Name}s\":[");

        //    for (int i = 0; i < matches.Count; i++)
        //    {
        //        sb.Append(matches[i].Value);
        //        if (i < matches.Count - 1)
        //        {
        //            sb.Append(",");
        //        }
        //    }

        //    sb.Append("]}");
        //    string jsonArray = sb.ToString();

        //    try
        //    {
        //        return JsonConvert.DeserializeObject<Loader>(jsonArray);
        //    }
        //    catch (JsonException ex)
        //    {
        //        throw new Exception("JSON parsing error: " + ex.Message);
        //    }
        //}
    }
}
