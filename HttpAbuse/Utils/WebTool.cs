using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Net;
using System.IO;

namespace SimpleHttpBrowser.Utils
{
    using Model;
    
    public static class WebTools
    {
        public const string srcPattern = "src\\s*=\\s*(?:[\"'](?<1>[^\"']*)[\"']|(?<1>\\S+))";

        /// <summary>
        /// Задает начальные настройки для запроса
        /// </summary>
        private static void _baseSettings(HttpWebRequest webRequest, RequestParametrs parametrs)
        {
            webRequest.ServicePoint.Expect100Continue = false; // strange shit should be disabled
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webRequest.ProtocolVersion = HttpVersion.Version11;

            webRequest.AllowAutoRedirect = false;
            webRequest.UserAgent = parametrs.UserAgent;
            webRequest.KeepAlive = parametrs.KeepAlive;
            webRequest.Timeout = parametrs.MinimumTimeout;
        }
        /// <summary>
        /// Объединяет заголовки браузера и заголовки данных запроса с приоритетом заголовков данных запроса.
        /// Переносит константные заголовки из общего списка в параметры запроса и добавляет заголовки в запрос.
        /// </summary>
        private static void _addHeaders(HttpWebRequest webRequest, Dictionary<string, string> defaultHeaders, Dictionary<string, string> headers)
        {
            Dictionary<string, string> newHeaders = new Dictionary<string, string>(headers);
            foreach (var h in defaultHeaders)
                if (!headers.ContainsKey(h.Key))
                    headers.Add(h.Key, h.Value);

            if (newHeaders.ContainsKey("Accept"))
            {
                webRequest.Accept = newHeaders["Accept"];
                newHeaders.Remove("Accept");
            }
            if (newHeaders.ContainsKey("Referer"))
            {
                webRequest.Referer = newHeaders["Referer"];
                newHeaders.Remove("Referer");
            }
            if (newHeaders.ContainsKey("Expect"))
            {
                webRequest.Expect = newHeaders["Expect"];
                newHeaders.Remove("Expect");
            }
            if (newHeaders.ContainsKey("Host"))
            {
                webRequest.Host = newHeaders["Host"];
                newHeaders.Remove("Host");
            }

            foreach (var h in newHeaders)
                webRequest.Headers.Add(h.Key, h.Value);

            newHeaders.Clear();
        }
        /// <summary>
        /// Добавляет cookies к запросу если они есть
        /// </summary>
        /// <param name="cookies">Cookies</param>
        private static void _addCookies(HttpWebRequest webRequest, CookieCollection cookies)
        {
            webRequest.CookieContainer = new CookieContainer();
            if (cookies != null)
            {
                foreach (Cookie c in cookies)
                    webRequest.CookieContainer.Add(c);
            }
        }
        private static void _processPOST(HttpWebRequest webRequest, RequestData data, string userAgent, Func<string> boundaryGenerator)
        {
            string boundary = boundaryGenerator != null ? boundaryGenerator() : WebTools.RandomBoundaryGenerator();
            string tmp;
            byte[] tb;
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                if (data.Data.Count > 0)
                {
                    webRequest.ContentType = "Content-Type: multipart/form-data; " + boundary;
                    if (data.Parametrs.Count > 0)
                    {
                        tmp = "";
                        foreach (var p in data.Parametrs)
                        {
                            tmp += string.Format("--{0}\\r\\nContent-Disposition: form-data; name=\"{1}\"\\r\\n\\r\\n{2}\\r\\n",
                                boundary, p.Key, p.Value);
                        }
                        tb = Encoding.ASCII.GetBytes(tmp);
                        requestStream.Write(tb, 0, tb.Length);

                    }
                    tmp = "";
                    foreach (MultipartData d in data.Data)
                    {
                        tmp += string.Format("--{0}\\r\\n{1}\\r\\n", boundary, d.Header);
                        tb = Encoding.ASCII.GetBytes(tmp);
                        requestStream.Write(tb, 0, tb.Length);
                        requestStream.Write(d.Data, 0, d.Data.Length);
                    }

                    // Closing boundary
                    string b_end = "--" + boundary + "--\\r\\n";
                    requestStream.Write(Encoding.ASCII.GetBytes(b_end), 0, Encoding.ASCII.GetByteCount(b_end));
                }
                else if (data.Parametrs.Count > 0)
                {
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    string t = "";
                    foreach (var p in data.Parametrs)
                        t += string.Format("{0}={1}&", boundary, p.Key, p.Value);
                    requestStream.WriteAsync(Encoding.ASCII.GetBytes(t), 0, Encoding.ASCII.GetByteCount(t));
                }
            }
        }
        public static HttpWebRequest PrepareWebRequest(CookieCollection cookies, RequestParametrs parametrs, RequestData data)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(data.URL);
            webRequest.Method = data.Method.ToString("F");
            _baseSettings(webRequest, parametrs);

            _addHeaders(webRequest, parametrs.Headers, data.Headers);
            _addCookies(webRequest, cookies);

            // Write parametr & data/multipart data
            switch (data.Method)
            {
                case RequestMethod.POST:
                    _processPOST(webRequest, data, parametrs.UserAgent, parametrs.BoundaryGenerator);
                    break;
            }
            return webRequest;
        }

        public static string RedirectURL(string oldUrl, string redirectUrl)
        {
            if (redirectUrl[0] == '/')
            {
                Uri oldUri = new Uri(oldUrl);
                return oldUri.Scheme + "://" + oldUri.Host + redirectUrl;
            }
            else
                return redirectUrl;
        }

        public static CookieCollection parseCookies(CookieContainer _cookies)
        {
            CookieCollection res = new CookieCollection();
            FieldInfo fiDomainTable = typeof(CookieContainer).GetField("m_domainTable", BindingFlags.NonPublic | BindingFlags.Instance);
            Hashtable domainTable = (Hashtable)fiDomainTable.GetValue(_cookies);
            foreach (string uri in domainTable.Keys)
            {
                object obPathList = domainTable[uri];
                FieldInfo fiList = obPathList.GetType().GetField("m_list", BindingFlags.NonPublic | BindingFlags.Instance);
                SortedList pathList = (SortedList)fiList.GetValue(obPathList);
                foreach (string key in pathList.Keys)
                {
                    CookieCollection cookies = (CookieCollection)pathList[key];
                    foreach (Cookie cookie in cookies)
                        res.Add(cookie);
                }
            }
            return res;
        }
        private static string ToUriInfo(Dictionary<string, string> parametrs)
        {
            string res = "";
            foreach (var e in parametrs)
                res += e.Key + '=' + e.Value + '&';
            return res.Remove(res.Length - 1, 1);
        }
        public static string MergeParametrsToURL(string url, Dictionary<string, string> parametrs)
        {
            string res = url + '?' + ToUriInfo(parametrs);
            return res;
        }
        public static string MergeParametrsToURL(Uri url, Dictionary<string, string> parametrs)
        {
            return MergeParametrsToURL(url.OriginalString, parametrs);
        }
        public static Dictionary<string, string> ParseUrlQuery(string url)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();
            if (url.IndexOf('?') > 0)
            {
                url = url.Remove(0, url.IndexOf('?') + 1);
                string[] p;
                foreach (string pair in url.Split('&'))
                {
                    p = pair.Split('=');
                    res.Add(p[0], p[1]);
                }
            }
            return res;
        }
        public static List<string> GetPage(Stream stream, string charset)
        {
            List<string> res = new List<string>();
            if (stream != null)
            {
                StreamReader reader;
                string x;
                using (reader = new StreamReader(stream, Encoding.GetEncoding(charset)))
                {
                    while (!reader.EndOfStream)
                    {
                        x = reader.ReadLine();
                        res.Add(x);
                    }
                }
                stream.Close();
            }
            return res;
        }
        public static List<string> GetPage(Stream stream)
        {
            return GetPage(stream, "UTF-8");
        }

        public static List<string> DumpFrames(string inputString)
        {
            List<string> res = new List<string>();
            Match m;
            m = Regex.Match(inputString, srcPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            while (m.Success)
            {
                res.Add(m.Groups[1].ToString());
                m = m.NextMatch();
            }
            return res;
        }

        public static string WebKitBoundaryGenerator()
        {
            Random r = new Random();
            string res = "";
            res = "----WebKitFormBoundary";
            for (int i = 0; i < 16; i++)
                res += Convert.ToChar(Convert.ToInt32(Math.Floor(26 * r.NextDouble() + 65)));
            return res;
        }
        public static string RandomBoundaryGenerator()
        {
            Random r = new Random();
            string res = Guid.NewGuid().ToString().Replace("-", "");
            return res;
        }
    }
}
