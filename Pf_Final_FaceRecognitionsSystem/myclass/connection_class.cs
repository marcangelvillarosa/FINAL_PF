using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace PfFinalProject.myclass
{
    internal class connection_class
    {
        public MySqlConnection con;

        public connection_class()
        {
            string host = "localhost";
            string database = "pf";
            string username = "root";
            string password = "";
            string port = "3306";

            string constring = "datasource =" + host + ";database=" + database + "; port=" + port + "; username =" + username + ";" + "password=" + password + "; SslMode =none;";

            con = new MySqlConnection(constring);
        }
    }
}
