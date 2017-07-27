using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindow
{
    class Pr0List
    {
        public bool atEnd { get; set; }
        public bool atStart { get; set; }
        public object error { get; set; }
        public List<Pr0Element> items { get; set; }
        public int ts { get; set; }
        public string cache { get; set; }
        public int rt { get; set; }
        public int qc { get; set; }
    }
}
