using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace YENTEN
{
    class DatabaseLibrary
    {
        private static SQLiteConnection connection;

        public  static void ConnectionOpen()
        {
            try
            {
                connection = new SQLiteConnection("Data Source=MainDB1.db;Version=3;");
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
                connection.Close();
                connection.Dispose();
            }
            catch
            {
                string appendText = "Ошибка закрытия базы";
                Console.WriteLine(DateTime.Now + "  [Log]: " + appendText);
                System.IO.File.AppendAllText("log.txt", appendText);
            }
        }

        public static void CreateCommand(string queryString)
        {
            try
            {
                DatabaseLibrary.ConnectionOpen();
                SQLiteCommand Sqlcmd = new SQLiteCommand(queryString);
                Sqlcmd.ExecuteNonQuery();

            }catch(Exception e)
            {
                string appendText = "Ошибка выполнения запроса в базу базу: " +e;
                Console.WriteLine(DateTime.Now + "  [Log]: " + appendText);
                System.IO.File.AppendAllText("log.txt", appendText);
            }
            finally
            {
                DatabaseLibrary.ConnectionClose();
            }
        }
    }
}
