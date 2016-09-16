using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpBrowser.Model
{
    using Utils;

    /// <summary>
    /// Данные конкретного запроса к конкретному URL
    /// </summary>
    public class RequestData
    {
        private string _url;
        /// <summary>
        /// Адрес, по которому будет выполнен запрос.
        /// Если метод - GET и указаны параметры то вернет путь с параметрыми
        /// </summary>
        public string URL
        {
            get
            {
                if (Method == RequestMethod.GET && Parametrs.Count > 0)
                    return WebTools.MergeParametrsToURL(_url, Parametrs);
                return _url;
            }
            set { _url = value; }
        }
        public RequestMethod Method { get; set; }
        public Dictionary<string, string> Parametrs { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public List<MultipartData> Data { get; private set; }

        public void Clear()
        {
            _url = "";
            Method = RequestMethod.GET;
            Parametrs.Clear();
            Headers.Clear();
        }

        public RequestData(string url, RequestMethod method, Dictionary<string, string> parametrs, Dictionary<string, string> headers = null,  List<MultipartData> data = null)
        {
            _url = url;
            Method = method;
            Parametrs = new Dictionary<string, string>(parametrs);
            Headers = new Dictionary<string, string>(headers);
            Data = new List<MultipartData>(data);
        }
    }
}
