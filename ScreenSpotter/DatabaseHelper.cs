using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data;

namespace ScreenSpotter
{
    class DatabaseHelper
    {
        public static DataSet Database(string projectDirectory)
        {
            DataTable dtAll = new DataTable();
            DataTable dtURI = new DataTable();
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + projectDirectory + @"\DB\MyDB.db; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand SelectCommandAll = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT Cameras.Id as Id, Subjects.Name as Subject, Regions.Name as Region, Cameras.Name as Name, PhotoId, UriPhoto, UriMeteo, UriLogin, X, Y, Width, Height FROM Cameras, Subjects, Regions
                                    WHERE Cameras.Region = Regions.Id And Regions.Subjects = Subjects.Id"
                };
                SQLiteCommand SelectCommandURI = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT * FROM Subjects"
                };
                SQLiteDataReader sqlReaderAll, sqlReaderURI;
                try
                {
                    sqlReaderAll = SelectCommandAll.ExecuteReader();
                    sqlReaderURI = SelectCommandURI.ExecuteReader();

                    dtAll.Load(sqlReaderAll);
                    dtURI.Load(sqlReaderURI);
                }
                finally
                {
                    Connect.Close();
                }
            }
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dtAll);
            dataSet.Tables.Add(dtURI);

            return dataSet;

        }
    }
}
