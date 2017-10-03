using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainWindow
{
    public static class TextHelper
    {
        public static string StringBetweenStrings(string s, string s1, string s2)
        {
            try
            {
                string output = s;
                output = output.Split(new string[] { s1 }, StringSplitOptions.None)[1];
                output = output.Split(new string[] { s2 }, StringSplitOptions.None)[0];
                return output;
            }
            catch
            {
                return "";
            }
        }
        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }
    }
}
