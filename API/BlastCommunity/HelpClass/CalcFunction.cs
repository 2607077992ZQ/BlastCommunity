using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using TinyPinyin;

namespace Blast_Community.HelpClass
{
    public class CalcFunction
    {
        /// <summary>
        /// GUID
        /// </summary>
        /// <returns></returns>
        public static string GuidStr()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        /// <summary>
        /// 字符串转hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetSha256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// 输出提示
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string PrintStr(string error)
        {
            return $"\"{error}\"";
        }

        private static string Strat { get => "zz"; }

        /// <summary>
        /// obj to base64
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToBase64(object obj)
        {
            if (obj == null)
                return string.Empty;

            string jsonString = JsonConvert.SerializeObject(obj);
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            return Strat + Convert.ToBase64String(byteArray);
        }

        /// <summary>
        /// base64 to object
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        public static object? Base64ToObject(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return null;

            byte[] byteArray = Convert.FromBase64String(base64String.Substring(Strat.Length));
            string jsonString = Encoding.UTF8.GetString(byteArray);
            return JsonConvert.DeserializeObject(jsonString);
        }
    
        /// <summary>
        /// Object to Json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 取首字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string InitialChar(string str)
        {
            char c = str.Trim()[0];
            if (c >= 0 && c <= 127)
            {
                return c.ToString();
            }
            else
            {
                return PinyinHelper.GetPinyin(str)[0].ToString();
            }

        }
    }
}
