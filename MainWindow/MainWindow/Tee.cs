using System.Drawing;

namespace MainWindow
{
    public class Tee
    {
        public string title;
        public string price;
        public string img;

        public Tee(string _title, string _price, string _img)
        {
            //System.Net.WebRequest request = System.Net.WebRequest.Create("https:" + _img);
            //System.Net.WebResponse response = request.GetResponse();
            //System.IO.Stream responseStream = response.GetResponseStream();
            //img = new Bitmap(responseStream);
            img = "https:" + _img;
            title = _title;
            price = _price;
        }
    }
}
