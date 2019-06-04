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
using NLog;

namespace ScreenSpotter
{

    public partial class Form1 : Form
    {
        Class1 cl = new Class1();
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        Logger logger = LogManager.GetCurrentClassLogger();
        DataTable dt = new DataTable();
        bool timeriswork = true;
        Image imgForPB;
        int carCount = 0;

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

        private void SelectionDirectory()
        {
            FolderBrowserDialog DirDialog = new FolderBrowserDialog();
            DirDialog.Description = "Выбор директории";
            DirDialog.SelectedPath = @"C:\";

            if (DirDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.imageDirectory = DirDialog.SelectedPath;
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Upgrade();
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.imageDirectory == "" || !Directory.Exists(Properties.Settings.Default.imageDirectory))
            {
                SelectionDirectory();
            }
            Database();
            DownloadImages();
            button1.Text = "Таймер запущен!";
            button1.BackColor = Color.Green;
            timer1.Interval = 600000;
            timer1.Enabled = true;
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
                    CommandText = @"SELECT Cameras.Id as Id, Subjects.Name as Subject, Regions.Name as Region, Cameras.Name as Name, Url, X, Y, Width, Height FROM Cameras, Subjects, Regions
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
        int[] quad = new int[91];
        

        public void ImageProcessing(Image img, Image imgSource)
        {
            Bitmap bmp = new Bitmap(img);
            Bitmap bmpSource = new Bitmap(imgSource);

            Bitmap bmpForPB = new Bitmap(img);

            int best = 0, bestTemp = 0, rsum = 0;

            int squareCount = 0, carOn = 0;
            for (int a = 1; a < 508; a = a + 39)
            {
                for (int b = 1; b < 274; b = b + 39)
                {
                    int squareDist = 0;
                    best = 0;
                    for (int i = a; i < a + 39; i = i + 7)
                    {
                        for (int j = b; j < b + 39; j = j + 7)
                        {
                            if (i == a || j == b)
                            {
                                bmpForPB.SetPixel(i, j, Color.Red);
                            }
                            for (int g = i; g <= i + 6; g = g + 3)
                            {
                                for (int f = j; f <= j + 6; f = f + 3)
                                {
                                    int xbest = 0, ybest = 0;
                                    if ((-278 * g + 133 * f + 2224 <= 0) && (-260 * g + 450 * f + 20800 >= 0))
                                    {
                                        bestTemp = Math.Abs(bmpSource.GetPixel(i + 3, j + 3).R - bmp.GetPixel(g, f).R) +
                                                        Math.Abs(bmpSource.GetPixel(i + 3, j + 3).G - bmp.GetPixel(g, f).G) +
                                                        Math.Abs(bmpSource.GetPixel(i + 3, j + 3).B - bmp.GetPixel(g, f).B);
                                        if ((bestTemp <= best) || ((g == i) && (f == j)))
                                        {
                                            xbest = g;
                                            ybest = f;

                                            best = bestTemp;
                                            rsum = rsum + best;
                                        }
                                    }
                                    bmpForPB.SetPixel(xbest, ybest, Color.Yellow);
                                }
                            }
                            squareDist = squareDist + best;
                        }
                    }
                    if(squareDist >= 1500)
                    {
                        carOn++;
                        logger.Trace("Машина найдена в квадрате с координатами: x = " + a.ToString() + ", y = " + b.ToString());
                    }
                    quad[squareCount] = squareDist;


                    //richTextBox1.AppendText("\n" + squareCount.ToString());
                   // richTextBox1.AppendText("-   "+squareDist.ToString());
                    


                    squareCount++;
                }
            }


            if (carOn >= 1)
            {
                carCount++;
                richTextBox1.AppendText("\nМашина замечена в " + carOn.ToString() + " квадратах");
                logger.Trace("Машина замечена в " + carOn.ToString() + " квадратах");
            }

            label1.Text = carCount.ToString();

            pictureBox1.Width = bmpForPB.Width;
            pictureBox1.Height = bmpForPB.Height;
            pictureBox1.Image = bmpForPB;
        }


        private void DownloadImages()
        {
            if (Properties.Settings.Default.imageDirectory == "")
            {
                richTextBox1.AppendText("\nНе задана директория для сохранения изображений!");
                return;
            }
                
            if (!Directory.Exists(Properties.Settings.Default.imageDirectory))
            {
                richTextBox1.AppendText("\nДиректория для изображений задана неверно!");
                return;
            }
                

            int i = 1;
            foreach (DataRow row in dt.Rows)
            {
                string id = row["Id"].ToString();
                int id1 = Convert.ToInt32(id);
                //richTextBox1.AppendText(id1.ToString());
                if (id1 == 157)
                {
                    string dir = Properties.Settings.Default.imageDirectory + @"\Data\" + row["Subject"].ToString();
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    dir = Properties.Settings.Default.imageDirectory + @"\Data\" + row["Subject"].ToString() + @"\" + row["Region"].ToString();
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    dir = Properties.Settings.Default.imageDirectory + @"\Data\" + row["Subject"].ToString() + @"\" + row["Region"].ToString() + @"\" + row["Name"].ToString();
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    using (WebClient webClient = new WebClient())
                    {
                        try
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
                                    imgForPB = img;
                                    
                                    if(DateTime.Now.TimeOfDay > new TimeSpan(08, 00, 00) && DateTime.Now.TimeOfDay < new TimeSpan(15, 00, 00))
                                    {
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\08-15.png"));
                                    }
                                    else if(DateTime.Now.TimeOfDay > new TimeSpan(15, 00, 00) && DateTime.Now.TimeOfDay < new TimeSpan(20, 00, 00))
                                    {
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\15-20.png"));
                                    }
                                    else if (DateTime.Now.TimeOfDay > new TimeSpan(20, 00, 00) && DateTime.Now.TimeOfDay < new TimeSpan(23, 00, 00))
                                    {
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\20-23.png"));
                                    }
                                    else if (DateTime.Now.TimeOfDay > new TimeSpan(22, 00, 00) && DateTime.Now.TimeOfDay < new TimeSpan(05, 00, 00))
                                    {
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\23-05.png"));
                                    }
                                    else if (DateTime.Now.TimeOfDay > new TimeSpan(05, 00, 00) && DateTime.Now.TimeOfDay < new TimeSpan(08, 00, 00))
                                    {
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\08-15.png"));
                                    }
                                    img.Save(dir + @"\" + DateTime.Now.ToString("MM.dd.HH.mm.ss") + @".png");

                                    logger.Trace("Изображение сохранено по адресу" + dir + @"\" + DateTime.Now.ToString("MM.dd.HH.mm.ss") + @".png");


                                    //ImageProcessing(img);
                                    richTextBox1.AppendText("\n" + "Все фотографии загружены. Время:" + DateTime.Now.ToLongTimeString());
                                }
                                catch (ArgumentException)
                                {
                                    
                                    richTextBox1.AppendText("\nИзображение по адресу:" + row["Url"].ToString() + " недоступно");
                                }
                            }
                        }
                        catch (WebException)
                        {
                            richTextBox1.AppendText("\nПроблема доступа к сети. Время:" + DateTime.Now.ToString());
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
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DownloadImages();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            //   DownloadImages();
            SelectionDirectory();

        }


    }
}
