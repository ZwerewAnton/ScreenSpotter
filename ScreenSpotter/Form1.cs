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
using System.Runtime.InteropServices;

namespace ScreenSpotter
{
    public partial class Form1 : Form
    {
        Class1 cl = new Class1();
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
            //DownloadImages();
            button1.Text = "Таймер запущен!";
            button1.BackColor = Color.Green;
            timer1.Interval = 600000;
            //timer1.Enabled = true;
            //timer1.Start();
        }

        private void Database()
        {
            using (SQLiteConnection Connect = new SQLiteConnection(@"Data Source=" + projectDirectory + @"\DB\MyDB.db; Version=3;"))
            {
                Connect.Open();
                SQLiteCommand SelectCommand = new SQLiteCommand
                {
                    Connection = Connect,
                    CommandText = @"SELECT Subjects.Name as Subject, Regions.Name as Region, Cameras.Name as Name, Url, X, Y, Width, Height FROM Cameras, Subjects, Regions
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

                //using (WebClient client = new WebClient())
                //{
                //    Image img;
                //    string imagePath = dir + @"\" + DateTime.Now.ToString("MM.dd.HH.mm.ss") + @".png";
                //    client.DownloadFile(new Uri(row["Url"].ToString()), imagePath);
                //    try
                //    {
                //        img = Image.FromFile(imagePath);
                //    }
                //    catch (Exception)
                //    {
                //        richTextBox1.AppendText("");
                //        throw;
                //    }
                    
                //    int[] rectCoor = { 171, 50, 530, 306 };
                //    int[] rectCoor2 = { 20, 20, 50, 50 };
                //    img = Class1.cropImage(img, rectCoor2);
                //    img.Save(imagePath + ".png");
                //}

                using (WebClient webClient = new WebClient())
                {
                    using (Stream stream = webClient.OpenRead(row["Url"].ToString()))
                    {
                        try
                        {
                            Image img = Image.FromStream(stream);
                            string x = row["X"].ToString();
                            string y = row["Y"].ToString();
                            string width = row["Width"].ToString();
                            string height = row["Height"].ToString();
                            int[] rectCoor = { Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(width), Convert.ToInt32(height) };
                            img = Class1.cropImage(img, rectCoor);
                            img.Save(dir + @"\" + DateTime.Now.ToString("MM.dd.HH.mm.ss") + @".png");
                        }
                        catch (ArgumentException)
                        {
                            richTextBox1.AppendText("\nИзображение по адресу:" + row["Url"].ToString() + " недоступно");
                        }
                    }

                }

                if (i == dt.Rows.Count)
                {
                    richTextBox1.AppendText("\n" + "Все фотографии загружены. Время:" + DateTime.Now.ToLongTimeString());
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


        private void button2_Click(object sender, EventArgs e)
        {
            DownloadImages();
        }
    }
}
