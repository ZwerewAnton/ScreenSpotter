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

namespace ScreenSpotter
{
    class Class1
    {
        CookieContainer container = new CookieContainer();

        public static Image cropImage(Image img, int[] rectCoor)
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
    }
}
