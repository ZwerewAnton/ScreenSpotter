using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Drawing;
using HtmlAgilityPack;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using NLog;

namespace ScreenSpotter
{
    class NetworkHelper
    {
        Tuple<int, List<Rectangle>> processingTuple;
        CookieContainer container = new CookieContainer();
        Logger logger = LogManager.GetCurrentClassLogger();

        public void Login(DataTable dtURI)
        {
            foreach (DataRow row in dtURI.Rows)
            {
                if (row["Id"].ToString() == "1")
                {
                    
                }
                else if (row["Id"].ToString() == "2")
                {

                }
                else if (row["Id"].ToString() == "3")
                {

                }
                else if (row["Id"].ToString() == "4")
                {
                    string data = "table=%40login&link=login&data%5B0%5D%5Bid%5D=-1&data%5B0%5D%5Blogin%5D=%D0%93%D0%BE%D1%81%D1%82%D1%8C&data%5B0%5D%5Bremember%5D=1";
                    byte[] requestData = Encoding.UTF8.GetBytes(data);

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(row["UriLogin"].ToString());
                    request.AllowAutoRedirect = true;
                    request.Method = "POST";
                    request.CookieContainer = container;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = requestData.Length;

                    Stream stream = Stream.Null;
                    try
                    {
                        stream = request.GetRequestStream();
                        stream.Write(requestData, 0, requestData.Length);
                        var response = (HttpWebResponse)request.GetResponse();
                        var newPageCode = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    finally
                    {
                        if (stream != null)
                            ((IDisposable)stream).Dispose();
                    }
                }
            }
        }

        public DataTable Parser(DataTable dtURI)
        {

            DataTable dtStateOfRoad = new DataTable();
            dtStateOfRoad.Columns.Add("Id", typeof(string));
            dtStateOfRoad.Columns.Add("StateOfRoad", typeof(string));
            foreach (DataRow row in dtURI.Rows)
            {
                if (row["Id"].ToString() == "1")
                {

                }
                else if (row["Id"].ToString() == "2")
                {

                }
                else if (row["Id"].ToString() == "3")
                {

                }
                else if (row["Id"].ToString() == "4")
                {
                    try
                    {
                        
                        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(row["UriMeteo"].ToString());
                        webRequest.Method = "GET";
                        webRequest.CookieContainer = container;
                        string pageText;
                        using (var response = webRequest.GetResponse().GetResponseStream())
                        {
                            pageText = (new StreamReader(response, Encoding.UTF8)).ReadToEnd();
                        }

                        XDocument xdoc = XDocument.Parse(pageText);

                        foreach (XElement rowElement in xdoc.Element("data").Elements("row"))
                        {
                            DataRow drStateOfRoad = dtStateOfRoad.NewRow();

                            XElement idElement = rowElement.Element("id");
                            XElement stateElement = rowElement.Element("f16");

                            if (idElement != null && stateElement != null)
                            {
                                drStateOfRoad["Id"] = idElement.Value;
                                drStateOfRoad["StateOfRoad"] = Regex.Match(stateElement.Value, @"\>(.+?)\<").Groups[1].Value;
                                dtStateOfRoad.Rows.Add(drStateOfRoad);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return dtStateOfRoad;
        }

        public Tuple<int, List<Rectangle>, Image> DownloadImages(DataSet dataSet)
        {
            Image imgForPB, img = null;
            DataTable dtAll = dataSet.Tables[0];
            DataTable dtURI = dataSet.Tables[1];
            Login(dtURI);
            DataTable dtStateOfRoad = Parser(dtURI);

            List<Rectangle> listOfFoundRect = new List<Rectangle>();

            int carCount = 0;

            foreach (DataRow row in dtAll.Rows)
            {
                string id = row["Id"].ToString();
                int id1 = Convert.ToInt32(id);
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
                                    img = ImageHelper.cropImage(img, rectCoor);
                                    imgForPB = img;
                                   
                                    processingTuple = ProcessingByTime(state, img);

                                    img.Save(dir + @"\" + DateTime.Now.ToString("MM.dd.HH.mm.ss") + @".png");

                                    logger.Trace("Изображение сохранено по адресу" + dir + @"\" + DateTime.Now.ToString("MM.dd.HH.mm.ss") + @".png");
                                    
                                }
                                catch (ArgumentException)
                                {

                                }
                            }
                        }
                        catch (WebException)
                        {

                        }
                    }
                }
            }
            carCount = processingTuple.Item1;
            listOfFoundRect = processingTuple.Item2;
            return Tuple.Create(carCount, listOfFoundRect, img);

        }

        public Tuple<int, List<Rectangle>> ProcessingByTime(string state, Image img)
        {
            
            if (DateTime.Now.TimeOfDay > new TimeSpan(08, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(15, 00, 00))
            {
                if (state == "Сухая")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\08-15.png"));
                }
                else if (state == "Мокрая")
                {

                }
                else if (state == "Влажная")
                {

                }
            }
            else if (DateTime.Now.TimeOfDay > new TimeSpan(15, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(20, 00, 00))
            {
                if (state == "Сухая")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\15-20.png"));
                                    }
                else if (state == "Мокрая")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\15-20.png"));
                }
                else if (state == "Влажная")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\15-20.png"));
                }
            }
            else if (DateTime.Now.TimeOfDay > new TimeSpan(20, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(23, 00, 00))
            {
                if (state == "Сухая")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\20-23.png"));
                }
                else if (state == "Мокрая")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\20-23.png"));
                }
                else if (state == "Влажная")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\20-23.png"));
                }
            }
            else if (DateTime.Now.TimeOfDay > new TimeSpan(23, 00, 00) || DateTime.Now.TimeOfDay <= new TimeSpan(05, 00, 00))
            {
                if (state == "Сухая")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\23-05.png"));
                                    }
                else if (state == "Мокрая")
                {

                }
                else if (state == "Влажная")
                {

                }
            }
            else if (DateTime.Now.TimeOfDay > new TimeSpan(05, 00, 00) && DateTime.Now.TimeOfDay <= new TimeSpan(08, 00, 00))
            {
                if (state == "Сухая")
                {
                    processingTuple = ImageHelper.ImageProcessing(img, Image.FromFile(@"C:\Users\Антон\source\repos\ScreenSpotter\ScreenSpotter\Data\Алтайский край\Малиновский\с.Малиновский\08-15.png"));
                }
                else if (state == "Мокрая")
                {

                }
                else if (state == "Влажная")
                {

                }
            }
            return processingTuple;
        }
    }
}
