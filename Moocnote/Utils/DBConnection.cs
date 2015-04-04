using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Moocnote.Utils
{
    class DBConnection
    {
        //数据库连接字符串
        public string ConnString = "Database='weibo';Data Source='localhost';User Id='root';Password='';charset='utf8';pooling=true;port=3306";
        public MySqlConnection Conn = null;

        public DBConnection(String connectString)
        {
            this.ConnString = connectString;
            this.Conn = new MySqlConnection(this.ConnString);
            Conn.Open();
        }

        public DBConnection()
        {
            this.Conn = new MySqlConnection(this.ConnString);
            Conn.Open();
        }

        public void executeUpdate(String sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, this.Conn);
            if (cmd.ExecuteNonQuery() > 0)
            {
                Console.WriteLine("插入/更新/删除成功");
            }
        }

        public MySqlDataReader executeQuery(String sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, this.Conn);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                return reader;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                //reader.Close();
            }
        }

        public void CloseConn()
        {
            this.Conn.Close();
        }
    }
}
