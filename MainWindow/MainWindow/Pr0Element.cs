namespace MainWindow
{
    class Pr0Element
    {
        public int id { get; set; }
        public int promoted { get; set; }
        public int up { get; set; }
        public int down { get; set; }
        public int created { get; set; }
        public string image { get; set; }
        public string thumb { get; set; }
        public string fullsize { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public bool audio { get; set; }
        public string source { get; set; }
        public int flags { get; set; }
        public string user { get; set; }
        public int mark { get; set; }

        public string GetUrl()
        {
            return "https://img.pr0gramm.com/" + image;
        }
        public string GetThumb()
        {
            int lastIndex = image.LastIndexOf('.');
            string tmpImage = image.Remove(lastIndex, image.Length - lastIndex);
            return "https://thumb.pr0gramm.com/" + tmpImage + ".jpg";
        }
    }
}
