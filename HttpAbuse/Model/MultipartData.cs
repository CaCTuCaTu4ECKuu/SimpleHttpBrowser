using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpBrowser.Model
{
    /// <summary>
    /// Данные с бинарным содержимым
    /// </summary>
    public class MultipartData
    {
        /// <summary>
        /// Весь заголовок бинарного вложения, все поля и атрибуты в одной строке
        /// https://www.ietf.org/rfc/rfc2388.txt
        /// </summary>
        public string Header
        {
            get
            {
                string res = "content-disposition: form-data; name=\"" + Name + '"';
                if (!string.IsNullOrEmpty(FileName))
                    res += "; filename=\"" + FileName + "\\\r\\n";

                if (!string.IsNullOrEmpty(ContentType))
                    res += "content-type: " + ContentType;
                if (!string.IsNullOrEmpty(Charset))
                    res += ";charset=" + Charset + "\\\r\\n";

                if (!string.IsNullOrEmpty(ContentTransferEncoding))
                    res += "content-transfer-encoding: " + ContentTransferEncoding + "\\\r\\n";
                return res;
            }
        }
        /// <summary>
        /// Имя управляющего элемента
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Имя файла бинарного содердимого
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// MIME тип содержимого
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// Кодировка. Содержимого???
        /// </summary>
        public string Charset { get; set; }
        /// <summary>
        /// Еще кодировка содердимого? Или тип сжатия содердимого?
        /// </summary>
        public string ContentTransferEncoding { get; private set; }
        /// <summary>
        /// Бинарные данные
        /// </summary>
        public byte[] Data { get; private set; }

        public MultipartData(string name, byte[] data, string fileName = "", string contentType = "", string charset = "", string encoding = "")
        {
            Name = name;
            FileName = fileName;
            ContentType = contentType;
            Charset = charset;
            ContentTransferEncoding = encoding;
            Data = data;
        }
    }

}
