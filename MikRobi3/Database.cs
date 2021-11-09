using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MikRobi3
{
    class Database
    {
        string connStr;
        MySqlConnection myConn;

        public Database()
        {
            connStr = "Server=" + Program.settings["sql-server"] + "; Port=" + Program.settings["sql-port"] + "; Database=" + Program.settings["sql-database"] +
                "; Uid=" + Program.settings["sql-user"] + "; Pwd=" + Program.settings["sql-password"] + ";";
            myConn = new MySqlConnection(connStr);
        }

        public void TestDB()
        {
            try
            {
                myConn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            myConn.Close();
        }
    }
}
