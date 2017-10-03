using System;
using RedditSharp;
using RedditSharp.Things;
using System.Data.SQLite;
using System.Threading;

namespace MainWindow
{
    class SteamFree
    {
        private static readonly int TIME_FILTER = 2; //days
        private static readonly int CHECK_INTERVAL = 5; //minutes
        private static readonly string SUBREDDIT_NAME = "/r/FreeGamesOnSteam";
        private static SQLController sqlController = new SQLController("reddit");
        private static Reddit reddit = new Reddit();
        private static Bot _bot = null;
        private static long id;

        public SteamFree(long chatID, Bot bot)
        {
            if (_bot == null)
            {
                id = chatID;
                _bot = bot;
                ValidateDatabase();
                while (true)
                {
                    FetchNewPosts();
                    Console.WriteLine("[SteamFree] Next fetch in " + CHECK_INTERVAL + " minutes...");
                    Thread.Sleep(CHECK_INTERVAL * 60000);
                }
            }
        }

        private static void FetchNewPosts()
        {
            Subreddit subreddit = reddit.GetSubreddit(SUBREDDIT_NAME);
            //Fetching posts
            foreach (Post post in subreddit.GetNew())
            {
                //Determing timedifference
                long diff = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds() - post.CreatedUTC.ToUnixTimeSeconds();
                if (diff < (TIME_FILTER * 86400))
                {
                    //New post found (by time)
                    if (post.LinkFlairText != "Ended")
                    {
                        //New post found (by state)
                        AddPost(post);
                    }
                }
                else
                {
                    //Escape the loop, because of no more new posts
                    break;
                }
            }
        }

        private static void ValidateDatabase()
        {
            sqlController.ExecuteQuery("CREATE TABLE IF NOT EXISTS redditpost (postID VARCHAR(20), postName VARCHAR(255), postURL VARCHAR(255), hasVisited INT(1))");
        }

        private static void AddPost(Post post)
        {
            sqlController.ExecuteQuery("INSERT INTO redditpost(postID, postName, postURL, hasVisited) SELECT '" + SQLController.SafeSQL(post.Id) + "', '" + SQLController.SafeSQL(post.Title) + "', '" + SQLController.SafeSQL(post.Url.ToString()) + "', '0' WHERE NOT EXISTS(SELECT 1 FROM redditpost WHERE postID = '" + SQLController.SafeSQL(post.Id) + "')");
            SQLiteDataReader response = sqlController.ExecuteQuery("SELECT * FROM redditpost WHERE postID = '" + SQLController.SafeSQL(post.Id) + "'");

            while (response.Read())
            {
                bool visited = false;
                if (response["hasVisited"].ToString() == "1")
                {
                    visited = true;
                }
                if (!visited)
                {
                    _bot.SendMessageHTML(id, "<code>Es ist ein neues FreeSteamGame verfügbar!</code>" + Environment.NewLine + post.Url.ToString());
                    sqlController.ExecuteQuery("UPDATE redditpost SET hasVisited = 1 WHERE postID = '" + SQLController.SafeSQL(post.Id) + "'");
                }
            }
        }
    }
}
