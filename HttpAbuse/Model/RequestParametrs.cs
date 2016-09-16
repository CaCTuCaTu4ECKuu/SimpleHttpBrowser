using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpBrowser.Model
{
    /// <summary>
    /// Набор некоторых параметров для HttpWebRequest
    /// </summary>
    public class RequestParametrs
    {
        /// <summary>
        /// Идентификатор веб-клиента
        /// </summary>
        public string UserAgent { get; set; }
        public Func<string> BoundaryGenerator { get; set; }
        /// <summary>
        /// Автоматическая переадресация при загрузке страницы
        /// </summary>
        public bool AllowAutoRedirect { get; set; }
        public bool KeepAlive { get; set; }

        public int MinimumTimeout { get; set; }
        public int TimeoutStep { get; set; }
        public int MaximumTimeout { get; set; }
        public int TryCount { get; set; }
        /// <summary>
        /// Пытатся загрузить страницу любой ценой
        /// </summary>
        public bool TryUntilLoad { get; set; }
        /// <summary>
        /// Набор заголовков которые добавляються вместе с этим веб-клиентом
        /// </summary>
        public Dictionary<string, string> Headers { get; private set; }

        /// <summary>
        /// Изменить идентификатор веб-клиента и набор его заголовков
        /// </summary>
        /// <param name="userAgent">Новый веб-клиент</param>
        /// <param name="headers">Новые заголовки</param>
        public void ChangeBrowser(string userAgent, Dictionary<string, string> headers)
        {
            UserAgent = userAgent;
            Headers = new Dictionary<string, string>(headers);
        }

        public RequestParametrs(string userAgent = null, Dictionary<string, string> headers = null, Func<string> boundaryGenerator = null)
        {
            ChangeBrowser(userAgent, headers);
            BoundaryGenerator = boundaryGenerator;
            AllowAutoRedirect = true;
            KeepAlive = true;

            MinimumTimeout = 5000;
            TimeoutStep = 5000;
            MaximumTimeout = 15000;
            TryCount = 5;
            TryUntilLoad = true;
        }
    }

}
