using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MainWindow
{
    public static class HTTPRequester
    {
        public static string SimpleRequest(string url, CookieContainer cookieCon = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                if (cookieCon != null)
                {
                    request.CookieContainer = cookieCon;
                }

                var response = (HttpWebResponse)request.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                return responseString;
            }
            catch
            {
                return "";
            }
        }

        public static string PostRequest(string url, string args, CookieContainer cookieCon = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            if (cookieCon != null)
            {
                request.CookieContainer = cookieCon;
            }
            var data = Encoding.ASCII.GetBytes(args);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;
        }
    }
}