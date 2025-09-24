/* LUCAS CHANGE DETAIL */
using System.Text;
using System.Web;

namespace Web_PCN.Utils
{
    /// <summary>
    /// Mã hóa/giải mã đường dẫn file để không lộ UNC ra client.
    /// Dùng UrlToken để URL-safe và ngắn gọn.
    /// </summary>
    public static class FileToken
    {
        public static string Encode(string s)
            => string.IsNullOrEmpty(s) ? "" : HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(s));

        public static string Decode(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            try
            {
                var b = HttpServerUtility.UrlTokenDecode(token);
                return b == null ? null : Encoding.UTF8.GetString(b);
            }
            catch { return null; }
        }
    }
}
