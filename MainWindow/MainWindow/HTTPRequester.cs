using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MainWindow
{
    public static class HTTPRequester
    {
        private static WebClient client = null;
        private static void Init()
        {
            if (client == null)
            {
                client = new WebClient();
                client.Proxy = null;
            }
        }

        public static string SimpleRequest(string url)
        {
            Init();
            return client.DownloadString(url);
        }
    }
}
