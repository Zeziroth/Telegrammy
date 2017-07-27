using System.Collections.Generic;

namespace MainWindow
{
    public class Settings
    {
        internal static readonly string API_KEY = "%YOUR_API_KEY%";
        internal static readonly List<int> ADMINS = new List<int>() { 239192404 };
        internal static readonly int SPAM_SECONDS = 1;
        internal static bool ignoreInput = true;
    }
}