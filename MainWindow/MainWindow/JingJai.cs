using System;

namespace MainWindow
{
    class JingJai
    {
        public string day { get; set; }
        public string title { get; set; }
        public string desc { get; set; }

        public void AddDesc(string desc)
        {
            string newDesc = desc;
            newDesc = newDesc.Replace((char)13, '\0');
            newDesc = newDesc.Replace((char)10, '\0');
            newDesc = newDesc.Replace(Environment.NewLine, "");
            if (this.desc == "")
            {
                this.desc = newDesc;
            }
            else
            {
                this.desc += "," + newDesc;
            }
        }

        public JingJai(string day)
        {
            this.day = day;
            this.desc = "";
        }

        public JingJai(string day, string title)
        {
            this.day = day;
            this.title = title;
            this.desc = "";
        }
    }
}
