using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SalesToCSV
{
    class Connection
    {
        public MySqlConnection connection;
        public string server;
        public string database;
        public string username;
        public string password;


        public void Initialize(string DB)
        {
            server = "10.10.0.199";
            database = DB;
            username = "root";
            password = "123";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + username + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);

        }

        public bool OpenConnection(string DB)
        {
            Initialize(DB);
            try
            {
                connection.Open();
                System.Diagnostics.Debug.WriteLine(connection.ToString());
                return true;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
                return false;
            }
        }

        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message.ToString());
                return false;
            }
        }
    }
}
