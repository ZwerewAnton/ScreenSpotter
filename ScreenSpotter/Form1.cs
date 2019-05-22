using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Data.SQLite;

namespace ScreenSpotter
{
    public partial class Form1 : Form
    {
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        DataTable dt = new DataTable();
        bool timeriswork = true;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!timeriswork)
            {
                timer1.Start();
                button1.Text = "Таймер работает!";
                button1.BackColor = Color.Green;
                timeriswork = true;
            }
            else
            {
                timer1.Stop();
                button1.Text = "Таймер остановлен!";
                button1.BackColor = Color.Red;
                timeriswork = false;
            }
        }
                                  
        private void Form1_Load(object sender, EventArgs e)
        {
            Database();
            DownloadImages();
            button1.Text = "Таймер запущен!";
            button1.BackColor = Color.Green;
            timer1.Interval = 600000;
            timer1.Enabled = true;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
        }

        private void Database()
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + projectDirectory + @"\DB\MyDB.db; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand SelectCommand = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT Subjects.Name as Subject, Regions.Name as Region, Cameras.Name as Name, Url FROM Cameras, Subjects, Regions
                                    WHERE Cameras.Region = Regions.Id And Regions.Subjects = Subjects.Id"
                };
                SQLiteDataReader sqlReader;
                try
                {
                    sqlReader = SelectCommand.ExecuteReader();
                    richTextBox1.AppendText("База данных подключена.");
                }
                catch (Exception e)
                {
                    richTextBox1.AppendText("Ошибка подключения к БД. Ошибка:" + e);
                    throw;
                }
                try
                {
                    dt.Load(sqlReader);
                }
                catch (Exception e)
                {
                    richTextBox1.AppendText("Ошибка загрузки таблицы. Ошибка:" + e);
                    throw;
                }
                Connect.Close();
            }
        }

        private void DownloadImages()
        {
            int i = 1;
            foreach (DataRow row in dt.Rows)
            {
                string dir = projectDirectory + @"\Data\" + row["Subject"].ToString();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                dir = projectDirectory + @"\Data\" + row["Subject"].ToString() + @"\" + row["Region"].ToString();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                dir = projectDirectory + @"\Data\" + row["Subject"].ToString() + @"\" + row["Region"].ToString() + @"\" + row["Name"].ToString();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (WebClient client = new WebClient())
                {
                   client.DownloadFileAsync(new Uri(row["Url"].ToString()), dir + @"\" + DateTime.Now.ToString("MM.dd.hh.mm.ss") + @".png");
                }
                if (i == dt.Rows.Count)
                {
                    richTextBox1.AppendText( "\n" + "Все фотографии загружены. Время:" + DateTime.Now.ToLongTimeString());
                }
                else
                {
                    i++;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DownloadImages();
        }
    }
}
