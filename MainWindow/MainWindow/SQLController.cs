using System;
using System.Data.SQLite;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;

namespace MainWindow
{

    class SQLController : IDisposable
    {
        private string _dbName;
        private string _sqliteFilename;
        private SQLiteConnection _connection;
        private bool disposed = false;
        private SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        public SQLController(string dbName)
        {
            _dbName = dbName;
            _sqliteFilename = _dbName + ".sqlite";
            Initialize();
        }

        private void Initialize()
        {
            if (!File.Exists(Application.StartupPath + @"\" + _sqliteFilename))
            {
                SQLiteConnection.CreateFile(_sqliteFilename);
            }

            _connection = new SQLiteConnection("Data Source=" + _sqliteFilename + "; Version=3;");
            _connection.Open();
        }
        public static string SafeSQL(string val)
        {
            return val.Replace("'", "");
        }
        public SQLiteDataReader ExecuteQuery(string query)
        {
            SQLiteCommand command = new SQLiteCommand(query, _connection);
            SQLiteDataReader reader = command.ExecuteReader();

            return reader;
        }
        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            _connection.Close();
            if (disposing)
            {
                handle.Dispose();
            }
            disposed = true;
        }
        #endregion
    }
}
