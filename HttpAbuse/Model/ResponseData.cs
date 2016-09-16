using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace SimpleHttpBrowser.Model
{
    /// <summary>
    /// Ответ от сервера на запрос
    /// </summary>
    public class ResponseData
    {
        public HttpWebResponse Response;

        private List<string> _lines = null;
        private Encoding _lastEncoding = null;
        private byte[] _bytes = null;

        /// <summary>
        /// Ссылка на страницу, от которой был получен этот ответ
        /// </summary>
        public string Url
        {
            get { return Response.ResponseUri.ToString(); }
        }
        /// <summary>
        /// Cookies, полученные вместе с ответом (Отправленные cookies не содержаться здесь)
        /// </summary>
        public CookieCollection Cookies
        {
            get { return Response.Cookies; }
        }

        private void _getPage(Encoding encoding)
        {
            if (_lines == null || _lastEncoding != encoding)
            {
                _lastEncoding = encoding;
                _lines = new List<string>();
                using (StreamReader reader = new StreamReader(Response.GetResponseStream(), _lastEncoding))
                {
                    while (!reader.EndOfStream)
                        _lines.Add(reader.ReadLine());
                }
            }
        }
        private Encoding _chooseEncoding(string encoding)
        {
            Encoding res;
            try
            {
                res = Encoding.GetEncoding(encoding);
            }
            catch (ArgumentException)
            {
                res = _lastEncoding == null ? Encoding.UTF8 : _lastEncoding;
            }
            return res;
        }

        #region Список строк
        public List<string> GetLines(Encoding encoding)
        {
            _getPage(encoding);
            return _lines;
        }
        public List<string> GetLines(string encoding)
        {
            return GetLines(_chooseEncoding(encoding));
        }
        #endregion
        public List<string> Lines
        {
            get
            {
                return GetLines(_chooseEncoding(""));
            }
        }
        #region Страница строкой
        public string GetPage(Encoding encoding, string separator)
        {
            return string.Join(separator, GetLines(encoding));
        }
        public string GetPage(string encoding, string separator)
        {
            return string.Join(separator, GetLines(encoding));
        }
        public string GetPage(Encoding encoding)
        {
            return GetPage(encoding, Environment.NewLine);
        }
        public string GetPage(string encoding)
        {
            return GetPage(encoding, Environment.NewLine);
        }
        #endregion
        public string Page
        {
            get
            {
                return GetPage(Response.ContentEncoding);
            }
        }
        public byte[] Bytes
        {
            get
            {
                if (_bytes == null)
                {
                    Stream source = Response.GetResponseStream();
                    MemoryStream ms;
                    int read;
                    byte[] buffer = new byte[16 * 1024];
                    using (ms = new MemoryStream())
                    {
                        while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                            ms.Write(buffer, 0, read);
                    }
                    _bytes = ms.ToArray();
                }
                return _bytes;
            }
        }

        public ResponseData(HttpWebResponse response)
        {
            Response = response;
        }

        public static implicit operator ResponseData(HttpWebResponse response)
        {
            return new ResponseData(response);
        }
        public static implicit operator HttpWebResponse(ResponseData data)
        {
            return data.Response;
        }
    }

}
