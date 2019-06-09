using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using HtmlAgilityPack;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Data;

namespace ScreenSpotter
{
    class Class1
    {

        CookieContainer container = new CookieContainer();

        public void ImageProcessing(System.Drawing.Image img)
        {
            for(int i = 1; i <= img.Width; i++)
            {

            }
        }

        public static System.Drawing.Image cropImage(System.Drawing.Image img, int[] rectCoor)
        {
            Rectangle cropArea = new Rectangle(rectCoor[0], rectCoor[1], rectCoor[2], rectCoor[3]);
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }
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

                    using (Stream S = request.GetRequestStream())
                        S.Write(requestData, 0, requestData.Length);

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        var newPageCode = new StreamReader(response.GetResponseStream()).ReadToEnd();
                         //response.GetResponseStream();
                    }
                }
            }
        }

        public  void Parser(DataTable dtURI)
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
                    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(row["UriMeteo"].ToString());
                    webRequest.Method = "GET";
                    webRequest.CookieContainer = container;
                    string PageText;
                    using (var response = webRequest.GetResponse().GetResponseStream())
                    {
                        PageText = (new StreamReader(response, Encoding.UTF8)).ReadToEnd();
                    }

                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

                    doc.LoadHtml(PageText);
                    foreach (HtmlNode n in doc.DocumentNode.SelectNodes("//row//id"))
                    {
                        //richTextBox1.AppendText("\n" + "\n" + n.InnerText);
                    }
                }
            }
        }
    }
}
