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
                myConn.Close();
            }
            catch (Exception ex)
            {
                Program.log.Write("database", ex.Message);
            }
        }

        public int SQLCommandCount(string command)
        {
            int result = -1;
            MySqlCommand cmd = new MySqlCommand(command, myConn);
            try
            {
                myConn.Open();
                result = Convert.ToInt32(cmd.ExecuteScalar());
                myConn.Close();
            }
            catch (Exception ex)
            {
                Program.log.Write("database", ex.Message);
            }
            return result;
        }

        public int SQLCommandNonQuery(string command)
        {
            int result = -1;
            MySqlCommand cmd = new MySqlCommand(command, myConn);
            try
            {
                myConn.Open();
                result = cmd.ExecuteNonQuery();
                myConn.Close();
            }
            catch (Exception ex)
            {
                Program.log.Write("database", ex.Message);
            }
            return result;
        }

        public string SQLCommandRecord(string command)
        {
            string result = "";
            MySqlCommand cmd = new MySqlCommand(command, myConn);
            try
            {
                myConn.Open();
                result = Convert.ToString(cmd.ExecuteScalar());
                myConn.Close();
            }
            catch (Exception ex)
            {
                Program.log.Write("database", ex.Message);
            }
            return result;
        }

        public List<string> SQLSelectCol(string command, int column)
        {
            List<string> result = new List<string>();
            MySqlCommand cmd = new MySqlCommand(command, myConn);
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                myConn.Open();
                while (reader.Read())
                {
                    result.Add(reader.GetString(column));
                }
                myConn.Close();
            }
            catch (Exception ex)
            {
                Program.log.Write("database", ex.Message);
            }
            return result;
        }

        public List<List<string>> SQLSelect(string command)
        {
            List<string> row;
            List<List<string>> result = new List<List<string>>();
            MySqlCommand cmd = new MySqlCommand(command, myConn);
            try
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                myConn.Open();
                while (reader.Read())
                {
                    row = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                        row.Add(reader.GetString(i));
                    result.Add(row);
                }
                myConn.Close();
            }
            catch (Exception ex)
            {
                Program.log.Write("database", ex.Message);
            }
            return result;
        }

        public string GetLatestUpdate(bool betaTesting, string hash)
        {
            string command = "SELECT path FROM `launcher-versions` WHERE stable=";
            if (betaTesting) command += "0"; else command += "1";
            command += " AND checksum LIKE '" + hash + "'";
            int results = SQLCommandCount(command);
            switch (results)
            {
                
                case 0:  //Launcher executable is bad or tampered
                    command = "SELECT path FROM `launcher-versions` WHERE stable=";
                    if (betaTesting) command += "0"; else command += "1";
                    command += " ORDER BY date DESC LIMIT 1";
                    return "2&" + SQLCommandRecord(command);
                    break;

                case 1: //Is running current version
                    command = "SELECT checksum FROM `launcher-versions` WHERE stable=";
                    if (betaTesting) command += "0"; else command += "1";
                    command += " ORDER BY date DESC LIMIT 1";
                    string latestHash = SQLCommandRecord(command);
                    if (hash == latestHash)
                        return "0";
                    else
                    {
                        command = "SELECT path FROM `launcher-versions` WHERE stable=";
                        if (betaTesting) command += "0"; else command += "1";
                        command += " ORDER BY date DESC LIMIT 1";
                        string path = SQLCommandRecord(command);
                        return "1&" + path;
                    }
                    break;

                default: //Error while running SQL SELECT
                    return "E";
                    break;
            }
        }
    }
}
