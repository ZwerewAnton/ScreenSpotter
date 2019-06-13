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
        
        NetworkHelper netHelper = new NetworkHelper();
        string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        Logger logger = LogManager.GetCurrentClassLogger();
        DataSet dataSet = new DataSet();
        DataTable dtAll = new DataTable();
        DataTable dtURI = new DataTable();
        bool timeriswork = true;
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

            try
            {
                dataSet = DatabaseHelper.Database(projectDirectory);
                richTextBox1.AppendText("База данных подключена.");
            }
            catch (SQLiteException ex)
            {
                richTextBox1.AppendText("Ошибка подключения к БД. Ошибка:" + ex);
            }
            catch (ConstraintException ex)
            {
                richTextBox1.AppendText("Ошибка загрузки таблицы. Ошибка:" + ex);
            }

            NetworkHelperCallback();

            button1.Text = "Таймер запущен!";
            button1.BackColor = Color.Green;
            timer1.Interval = 600000;
            timer1.Enabled = true;
            timer1.Start();
        }


        int[] quad = new int[91];

        private void timer1_Tick(object sender, EventArgs e)
        {
            NetworkHelperCallback();
        }

        private void NetworkHelperCallback()
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

            try
            {
                List<Rectangle> listRect = new List<Rectangle>();
                netHelper.DownloadImages(dataSet);
                label1.Text = (carCount + netHelper.DownloadImages(dataSet).Item1).ToString();

                Pen pen = new Pen(Color.Red);
                img = netHelper.DownloadImages(dataSet).Item3;
                Graphics gr = Graphics.FromImage(img);
                listRect = netHelper.DownloadImages(dataSet).Item2;

                if (listRect.Count != 0)
                {
                    listRect.ForEach(delegate (Rectangle rect)
                    {
                        gr.DrawRectangle(pen, rect);
                    });
                    if(listRect.Count == 1)
                        richTextBox1.AppendText("\n" + "Машина найдена в 1 квадрате");
                    if (listRect.Count > 1)
                        richTextBox1.AppendText("\n" + "Машина найдена в: " + listRect.Count.ToString() + " квадратах");
                }

                pictureBox1.Width = img.Width;
                pictureBox1.Height = img.Height;
                pictureBox1.Image = img;

                richTextBox1.AppendText("\n" + "Все фотографии загружены. Время: " + DateTime.Now.ToLongTimeString());
            }
            catch (ArgumentException)
            {
                //по адресу:" + row["Url"].ToString() + "
                richTextBox1.AppendText("\nИзображение недоступно");
            }
            catch (WebException)
            {
                richTextBox1.AppendText("\nПроблема доступа к сети. Время:" + DateTime.Now.ToString());
            }
        }
    }
}
