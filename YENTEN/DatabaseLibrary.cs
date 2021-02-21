using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace YENTEN
{
    class DatabaseLibrary
    {
        private static SQLiteConnection connection;

        public static void CreateConnection()
        {
            connection = new SQLiteConnection("Data Source=MainDB1.db;Version=3;New=False;Compress=True;");
        }
        public  static void ConnectionOpen()
        {
            try
            {
                CreateConnection();
                connection.Open();
            }
            catch (Exception)
            {
                string appendText = "Ошибка открытия базы";
                Console.WriteLine(DateTime.Now + "  [Log]: "+ appendText);
                System.IO.File.AppendAllText("log.txt", appendText);
            }
        }

        public static void ConnectionClose()
        {
            try
            {
                CreateConnection();
                connection.Close();
                connection.Dispose();
            }
            catch(Exception)
            {
                string appendText = "Ошибка закрытия базы";
                Console.WriteLine(DateTime.Now + "  [Log]: " + appendText);
                System.IO.File.AppendAllText("log.txt", appendText);
            }
        }

        public static int  ExecuteScalarInt(string queryString)
        {
            CreateConnection();
            ConnectionOpen();
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            Sqlcmd.CommandText = queryString;
             int result = Convert.ToInt32(Sqlcmd.ExecuteScalar());
             ConnectionClose();
            return result;
        }
        public static double ExecuteScalarDouble(string queryString)
        {
            CreateConnection();
            ConnectionOpen();
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            Sqlcmd.CommandText = queryString;
            double result = Convert.ToDouble(Sqlcmd.ExecuteScalar());
            ConnectionClose();
            return result;
        }
        public static string ExecuteScalarString(string queryString)
        {
            CreateConnection();
            ConnectionOpen();
            SQLiteCommand Sqlcmd = connection.CreateCommand();
            Sqlcmd.CommandText = queryString;
            string result = Convert.ToString(Sqlcmd.ExecuteScalar());
            ConnectionClose();
            return result;
        }
        public static void ExecuteNonQuery(string queryString)
        {
            try
            {
                CreateConnection();
                ConnectionOpen();
                SQLiteCommand Sqlcmd = connection.CreateCommand();
                Sqlcmd.CommandText = queryString;
                Sqlcmd.ExecuteNonQuery();
                ConnectionClose();
            }
            catch(Exception e)
            {
                string appendText = DateTime.Now + "  [Log]: Ошибка выполнения запроса в базу базу: " + e;
                Console.WriteLine(appendText);
                System.IO.File.AppendAllText("log.txt", appendText);
            }
            finally
            {
            }
        }


    }
}
