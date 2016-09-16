using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;


namespace SimpleHttpBrowser
{
    using Model;
    using Utils;

    public class SimpleHttpBrowser
    {
        /// <summary>
        /// Параметры HTTP клиента (заголовки по умолчанию)
        /// </summary>
        public RequestParametrs Parametrs;
        /// <summary>
        /// Данные запроса, который будет отправлен
        /// </summary>
        public RequestData Data;
        /// <summary>
        /// Ответ на последний отправленный запрос
        /// </summary>
        public ResponseData Response = null;

        internal CookieCollection t_cookies;

        /// <summary>
        /// Подготавливает запрос исходя из указанных данных и передает ссылку на него.
        /// Метод выполняет полный пересбор запроса каждый раз
        /// </summary>
        /// <param name="cookies">Cookies для будущего запроса</param>
        /// <returns>Сформированный экземпляр HttpWebRequest</returns>
        public HttpWebRequest GetRequest(CookieCollection cookies)
        {
            return WebTools.PrepareWebRequest(cookies, Parametrs, Data);
        }
        /// <summary>
        /// Подготавливает запрос исходя из указанных данных и передает ссылку на него.
        /// Метод выполняет полный пересбор запроса каждый раз
        /// </summary>
        /// <param name="cookies">Cookies запроса</param>
        /// <returns>Ответ от выполненного запроса HttpWebRequest</returns>
        public HttpWebResponse GetResponse(ref CookieCollection cookies)
        {
            if (cookies == null)
                cookies = new CookieCollection();
            HttpWebRequest req = GetRequest(cookies);
            HttpWebResponse response = null;
            int errors = 0;
            bool success = false;
            while (!success)
            {
                try
                {
                    response = (HttpWebResponse)req.GetResponse();
                    cookies.Add(response.Cookies);
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            success = true;
                            break;
                        case HttpStatusCode.Redirect:
                            if (Parametrs.AllowAutoRedirect)
                                req = WebTools.PrepareWebRequest(cookies, Parametrs, new RequestData(WebTools.RedirectURL(req.RequestUri.OriginalString, response.Headers["Location"]), RequestMethod.GET, null));
                            else
                                goto case HttpStatusCode.OK;
                            break;
                        default:
                            goto case HttpStatusCode.OK;
                    }
                }
                catch (Exception)
                {
                    if (errors <= Parametrs.TryCount)
                    {
                        errors++;
                    }
                    else
                        success = true;
                }
            }
            Response = new ResponseData(response);
            return response;
        }
        public HttpWebResponse GetResponse()
        {
            t_cookies = new CookieCollection();
            return GetResponse(ref t_cookies);
        }
        /// <summary>
        /// Подготавливает запрос исходя из указанных данных и передает ссылку на него.
        /// Метод выполняет полный пересбор запроса каждый раз
        /// </summary>
        /// <param name="cookies">Cookies запроса</param>
        /// <param name="data">Данные запроса</param>
        /// <returns>Ответ от выполненного запроса HttpWebRequest</returns>
        public HttpWebResponse GetResponse(ref CookieCollection cookies, RequestData data)
        {
            Data = data;
            return GetResponse(ref cookies);
        }
        public ResponseData GetResponseData(ref CookieCollection cookies)
        {
            GetResponse(ref cookies);
            return Response;
        }
        public ResponseData GetResponseData()
        {
            t_cookies = new CookieCollection();
            return GetResponseData(ref t_cookies);
        }
        public ResponseData GetResponseData(ref CookieCollection cookies, RequestData data)
        {
            Data = data;
            return GetResponseData(ref cookies);
        }

        public SimpleHttpBrowser(RequestParametrs defaultParametrs = null)
        {
            Parametrs = defaultParametrs == null ? new RequestParametrs() : defaultParametrs;
        }
        public SimpleHttpBrowser() : this(null) { }
    }
}