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
using HtmlAgilityPack;
using System.Timers;

namespace ScreenSpotter
{

    public partial class Form1 : Form
    {
        


        Class1 cl = new Class1();
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        Logger logger = LogManager.GetCurrentClassLogger();
        DataTable dtAll = new DataTable();
        DataTable dtURI = new DataTable();
        bool timeriswork = true;
        Image imgForPB, imageSourceNew;
        int carCount = 0;
        Image img;

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
            imageSourceNew = Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\06.07.10.24.31.png");
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

                    richTextBox1.AppendText("База данных подключена.");
                }
                catch (SQLiteException e)
                {
                    richTextBox1.AppendText("Ошибка подключения к БД. Ошибка:" + e);
                }
                catch (ConstraintException e)
                {
                    richTextBox1.AppendText("Ошибка загрузки таблицы. Ошибка:" + e);
                }
                finally
                {
                    Connect.Close();
                }
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
                    if ((-278 * a + 133 * b + 2224 <= 0) && (-260 * a + 450 * b + 20800 >= 0) ||
                    (-278 * (a + 38) + 133 * b + 2224 <= 0) && (-260 * (a + 38) + 450 * b + 20800 >= 0) ||
                    (-278 * a + 133 * (b + 38) + 2224 <= 0) && (-260 * a + 450 * (b + 38) + 20800 >= 0) ||
                    (-278 * (a + 38) + 133 * (b + 38) + 2224 <= 0) && (-260 * (a + 38) + 450 * (b + 38) + 20800 >= 0))
                    {
                        int squareDist = 0;
                        best = 0;
                        for (int i = a; i < a + 45; i = i + 9)
                        {
                            for (int j = b; j < b + 45; j = j + 9)
                            {
                                if (i == a || j == b)
                                {
                                  //  bmpForPB.SetPixel(i, j, Color.Red);
                                }
                                for (int g = i; g < i + 9; g = g + 3)
                                {
                                    for (int f = j; f < j + 9; f = f + 3)
                                    {
                                        bmpForPB.SetPixel(i, j, Color.Red);
                                        int xbest = 0, ybest = 0;
                                        
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
                                        
                                        //bmpForPB.SetPixel(xbest, ybest, Color.Yellow);
                                    }
                                }
                                squareDist = squareDist + best;
                            }
                        }
                        if (squareDist >= 1500)
                        {
                            carOn++;
                            logger.Trace("Машина найдена в квадрате с координатами: x = " + a.ToString() + ", y = " + b.ToString());
                        }
                        quad[squareCount] = squareDist;

                        richTextBox1.AppendText("\n" + squareCount.ToString());
                        richTextBox1.AppendText("-   " + squareDist.ToString());

                        squareCount++;
                    }
                }
            }


            if (carOn >= 1)
            {
                carCount++;
                richTextBox1.AppendText("\nМашина замечена в " + carOn.ToString() + " квадратах");
                logger.Trace("Машина замечена в " + carOn.ToString() + " квадратах");
            }

            else
            {
                imageSourceNew = bmp;
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

            cl.Login(dtURI);
            DataTable dtStateOfRoad = cl.Parser(dtURI);

            int i = 1;
            foreach (DataRow row in dtAll.Rows)
            {
                string id = row["Id"].ToString();
                int id1 = Convert.ToInt32(id);
                //richTextBox1.AppendText(id1.ToString());
                if (id1 == 157)
                {
                    string searchExpression = "Id = " + row["PhotoId"].ToString();
                    DataRow rowsForId = dtStateOfRoad.Select(searchExpression)[0];
                    string state = rowsForId["StateOfRoad"].ToString();

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
                            using (Stream stream = webClient.OpenRead(row["UriPhoto"].ToString() + row["PhotoId"].ToString()))
                            {
                                try
                                {
                                    img = Image.FromStream(stream);
                                    string x = row["X"].ToString();
                                    string y = row["Y"].ToString();
                                    string width = row["Width"].ToString();
                                    string height = row["Height"].ToString();
                                    int[] rectCoor = { Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(width), Convert.ToInt32(height) };
                                    img = Class1.cropImage(img, rectCoor);
                                    imgForPB = img;
                                    
                                    if(DateTime.Now.TimeOfDay > new TimeSpan(08, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(15, 00, 00))
                                    {
                                        if(state == "Сухая")
                                        {
                                            ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\08-15.png"));

                                        }
                                        else if (state == "Мокрая")
                                        {

                                        }
                                        else if (state == "Влажная")
                                        {

                                        }
                                    }
                                    else if(DateTime.Now.TimeOfDay > new TimeSpan(15, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(20, 00, 00))
                                    {
                                        //ImageProcessing(img, imageSourceNew);
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\15-20.png"));
                                    }
                                    else if (DateTime.Now.TimeOfDay > new TimeSpan(20, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(23, 00, 00))
                                    {
                                        if (state == "Сухая")
                                        {
                                            ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\20-23.png"));

                                        }
                                        else if (state == "Мокрая")
                                        {

                                        }
                                        else if (state == "Влажная")
                                        {

                                        }
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\20-23.png"));
                                    }
                                    else if (DateTime.Now.TimeOfDay > new TimeSpan(23, 00, 00) || DateTime.Now.TimeOfDay <= new TimeSpan(05, 00, 00))
                                    {
                                        if (state == "Сухая")
                                        {
                                            ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\23-05.png"));

                                        }
                                        else if (state == "Мокрая")
                                        {

                                        }
                                        else if (state == "Влажная")
                                        {

                                        }
                                        ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\23-05.png"));
                                    }
                                    else if (DateTime.Now.TimeOfDay > new TimeSpan(05, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(08, 00, 00))
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

                    if (i == dtAll.Rows.Count)
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
        int o = 0;



        CookieContainer container = new CookieContainer();
        private void button2_Click(object sender, EventArgs e)
        {

            //cl.Login(dtURI);
            //DataTable dtStateOfRoad = cl.Parser(dtURI);
            //foreach (DataRow row in dtStateOfRoad.Rows)
            //{
            //    string l = row["Id"].ToString();
            //    richTextBox1.AppendText("\n" + l + row["StateOfRoad"].ToString());
            //}

            //SelectionDirectory();
            //switch (o)
            //{
            //    case 0:
            //        imageSourceNew = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.23.10.37.png");
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.22.50.37.png");
            //        break;
            //    case 1:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.23.00.37.png");
            //        break;
            //    case 2:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.23.10.37.png");
            //        break;
            //    case 3:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.23.20.37.png");
            //        break;
            //    case 4:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.23.30.37.png");
            //        break;
            //    case 5:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.23.40.39.png");
            //        break;
            //    case 6:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.05.23.50.38.png");
            //        break;
            //    case 7:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.00.00.38.png");
            //        break;
            //    case 8:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.00.10.37.png");
            //        break;
            //    case 9:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.00.20.37.png");
            //        break;
            //    case 10:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.00.30.38.png");
            //        break;
            //    case 11:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.00.40.36.png");
            //        break;
            //    case 12:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.00.50.37.png");
            //        break;
            //    case 13:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.01.00.37.png");
            //        break;
            //    case 14:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.01.10.37.png");
            //        break;
            //    case 15:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.01.20.36.png");
            //        break;
            //    case 16:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.01.30.36.png");
            //        break;
            //    case 17:
            //        img = Image.FromFile(@"C:\Users\Антон\Music\Roads\Night\06.06.01.40.36.png");
            //        break;
            //}
            //DownloadImages();
            //o++;







        }


    }
}
