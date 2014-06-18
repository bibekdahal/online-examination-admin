using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms; 
using System.Collections.Specialized;
using System.Globalization;

namespace Online_Examination_Admin
{
    public partial class Question : Form
    {
        public const string ADDR = "http://localhost/online-examination/";
        private int m_setid;
        private int m_numquestions;
        private int m_currentid;

        //private string m_imagefolder;

        public static string Post(string url, NameValueCollection formData)
        {
            WebClient webClient = new WebClient();
            byte[] responseBytes = webClient.UploadValues(url, "POST", formData);
            string responsefromserver = Encoding.UTF8.GetString(responseBytes);
            webClient.Dispose();
            return responsefromserver;
        }

        public Question(int setid)
        {
            InitializeComponent();
            
            m_setid = setid;
            //m_numquestions = 0;
            m_currentid = 0;
            UpdateUI();
        }

        public string GetTagData(string text, string tag)
        {
            int i = text.IndexOf("<" + tag + ">") + ("<" + tag + ">").Length;
            int f = text.LastIndexOf("</" + tag + ">");
            return text.Substring(i, f - i);
        }

        public void UpdateData()
        {
            if (m_currentid >= 0 && m_currentid < m_numquestions)
            {
                string url = ADDR + "question.php?setid=" + m_setid.ToString() + "&qsn=" + (m_currentid + 1).ToString();
                wb_question.Navigate(url);
                
                wb_optiona.Navigate(url + "&optid=0");
                wb_optionb.Navigate(url + "&optid=1");
                wb_optionc.Navigate(url + "&optid=2");
                wb_optiond.Navigate(url + "&optid=3");
            }
        }

        public void UpdateUI()
        {
            NameValueCollection data = new NameValueCollection();
            data["setid"] = m_setid.ToString();
            m_numquestions = Convert.ToInt32(Post(ADDR + "admin/getqns.php", data));

            if (m_currentid >= m_numquestions)
            {
                btn_next.Enabled = false;
                btn_addupdate.Text = "Add";
                btn_addupdate.Visible = true;
                rtb_question.Text = rtb_optiona.Text = rtb_optionb.Text = rtb_optionc.Text = rtb_optiond.Text = "";
                rtb_question.Visible = rtb_optiona.Visible = rtb_optionb.Visible = rtb_optionc.Visible = rtb_optiond.Visible = true;
                wb_question.Visible = wb_optiona.Visible = wb_optionb.Visible = wb_optionc.Visible = wb_optiond.Visible = false;
            }
            else
            {
                UpdateData();
                btn_next.Enabled = true;
                btn_addupdate.Text = "Update";
                btn_addupdate.Visible = false;
                rtb_question.Visible = rtb_optiona.Visible = rtb_optionb.Visible = rtb_optionc.Visible = rtb_optiond.Visible = false;
                wb_question.Visible = wb_optiona.Visible = wb_optionb.Visible = wb_optionc.Visible = wb_optiond.Visible = true;
            }

            if (m_currentid <= 0) btn_prev.Enabled = false;
            else btn_prev.Enabled = true;
        }

        private void btn_prev_Click(object sender, EventArgs e)
        {
            if (m_currentid > 0)
            {
                --m_currentid;
                UpdateUI();
            }
        }

        private void btn_next_Click(object sender, EventArgs e)
        {
            if (m_currentid <= m_numquestions)
            {
                ++m_currentid;
                UpdateUI();
            }
        }

        int imgcnt = 0;
        private void btn_addupdate_Click(object sender, EventArgs e)
        {
            if (btn_addupdate.Text=="Add")
            {
                NameValueCollection data = new NameValueCollection();
                data["setid"] = m_setid.ToString();
                data["qsn"] = (m_currentid + 1).ToString();
                imgcnt = 0;
                string imagefolder = m_setid.ToString() + "acnsj";
                string imgdir = "images/" + imagefolder;
                if (!System.IO.Directory.Exists(imgdir))
                    System.IO.Directory.CreateDirectory(imgdir);
                else
                    Array.ForEach(Directory.GetFiles(imgdir), File.Delete);
                data["question"] = ConvertRtfToHtml(rtb_question, imagefolder, (m_currentid + 1).ToString() + "x");
                data["optiona"] = ConvertRtfToHtml(rtb_optiona, imagefolder, (m_currentid + 1).ToString() + "x");
                data["optionb"] = ConvertRtfToHtml(rtb_optionb, imagefolder, (m_currentid + 1).ToString() + "x");
                data["optionc"] = ConvertRtfToHtml(rtb_optionc, imagefolder, (m_currentid + 1).ToString() + "x");
                data["optiond"] = ConvertRtfToHtml(rtb_optiond, imagefolder, (m_currentid + 1).ToString() + "x");
                string response = Post(ADDR + "admin/addquestion.php", data);

                
                NameValueCollection data1 = new NameValueCollection();
                data1["setid"] = m_setid.ToString();
                string[] fileEntries = Directory.GetFiles(imgdir);
                foreach (string fileName in fileEntries)
                    UploadImage(ADDR + "admin/addimage.php", fileName, data1);
                MessageBox.Show(response);
                UpdateUI();
            }
            else
            {
                // update
                UpdateUI();
            }
        }


        public string ConvertRtfToHtml(RichTextBox rtf, string imagefolder, string imagefnameprefix)
        {
            //string imgdir = "D:/wamp/www/online-examination/";
            bool b = false, i = false, u = false;
            string html = "";
            int x = 0;
            while (x < rtf.Text.Length)
            {
                rtf.Select(x, 1);

                if (rtf.SelectionFont.Bold)
                { if (!b) { html += "<strong>"; b = true; } }
                else if (b)
                { html += "</strong>"; b = false; }

                if (rtf.SelectionFont.Underline)
                { if (!u) { html += "<u>"; u = true; } }
                else if (u)
                { html += "</u>"; u = false; }

                if (rtf.SelectionFont.Italic)
                { if (!i) { html += "<i>"; i = true; } }
                else if (i)
                { html += "</i>"; i = false; }

                if (rtf.SelectedRtf.Contains(@"\pict"))
                {
                    int width = 0, height = 0;
                    string imageDataHex = ExtractImg(rtf.SelectedRtf, ref width, ref height);
                    byte[] imageBuffer = ToBinary(imageDataHex);
                    Image image;
                    using (System.IO.MemoryStream stream = new System.IO.MemoryStream(imageBuffer))
                        image = Image.FromStream(stream);
                    if (image != null)
                    {

                        Bitmap newimage = new Bitmap(image, new Size(width, height));
                        newimage.MakeTransparent(Color.White);                        
                        string imgdir = "images/" + imagefolder;
                        string imgpath = imgdir + "/" + imagefnameprefix + imgcnt.ToString() + ".png";
                        newimage.Save(imgpath, System.Drawing.Imaging.ImageFormat.Png);
                        imgcnt++;
                        html += "<img src=\"" + imgpath + "\" />";
                    }

                }
                else if (rtf.SelectedText == "\n")
                    html += "<br/>";
                else if (rtf.SelectedText == "\t")
                    html += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                else
                    html += WebUtility.HtmlEncode(rtf.SelectedText);
                x++;
            }
            return html;
        }



        public static byte[] ToBinary(string imageDataHex)
        {
            if (imageDataHex == null)
            {
                throw new ArgumentNullException("imageDataHex");
            }

            int hexDigits = imageDataHex.Length;
            int dataSize = hexDigits / 2;
            byte[] imageDataBinary = new byte[dataSize];

            StringBuilder hex = new StringBuilder(2);

            int dataPos = 0;
            for (int i = 0; i < hexDigits; i++)
            {
                char c = imageDataHex[i];
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                hex.Append(imageDataHex[i]);
                if (hex.Length == 2)
                {
                    imageDataBinary[dataPos] = byte.Parse(hex.ToString(), System.Globalization.NumberStyles.HexNumber);
                    dataPos++;
                    hex.Remove(0, 2);
                }
            }
            return imageDataBinary;
        }

        string ExtractImg(string s, ref int width, ref int height)
        {

            int pictTagIdx = s.IndexOf("{\\pict\\");
            {
                int widthindex = s.IndexOf(@"\picwgoal", pictTagIdx) + (@"\picwgoal").Length;
                int i = 0; int tens = 1;
                width = 0;
                char c;
                while (Char.IsDigit(c = s[i + widthindex]))
                {
                    width = width * 10 + (c - '0');
                    i++; tens *= 10;
                }
            }

            {
                int heightindex = s.IndexOf(@"\pichgoal", pictTagIdx) + (@"\pichgoal").Length;
                int i = 0; int tens = 1;
                height = 0;
                char c;
                while (Char.IsDigit(c = s[i + heightindex]))
                {
                    height = height * 10 + (c - '0');
                    i++; tens *= 10;
                }
            }

            {
                int index = s.IndexOf(@"\picscalex", pictTagIdx);
                if (index >= 0)
                {
                    index += (@"\picscalex").Length;
                    int i = 0; int tens = 1;
                    int scale = 0;
                    char c;
                    while (Char.IsDigit(c = s[i + index]))
                    {
                        scale = (c - '0') + tens * scale;
                        i++; tens *= 10;
                    }
                    width *= scale / 100;
                }
            }
            {
                int index = s.IndexOf(@"\picscaley", pictTagIdx);
                if (index >= 0)
                {
                    index += (@"\picscaley").Length;
                    int i = 0; int tens = 1;
                    int scale = 0;
                    char c;
                    while (Char.IsDigit(c = s[i + index]))
                    {
                        scale = (c - '0') + tens * scale;
                        i++; tens *= 10;
                    }
                    height *= scale / 100;
                }
            }

            using (var g = CreateGraphics())
            {
                width = ConvertTwipsToXPixels(g, width);
                height = ConvertTwipsToYPixels(g, height);
            }


            int startIndex = s.IndexOf(" ", pictTagIdx) + 1;
            int endIndex = s.IndexOf("}", startIndex);
            return s.Substring(startIndex, endIndex - startIndex);
        }

        public static int ConvertTwipsToXPixels(Graphics source, int twips)
        {
            return (int)(((double)twips) * (1.0 / 1440.0) * source.DpiX);
        }
        public static int ConvertTwipsToYPixels(Graphics source, int twips)
        {
            return (int)(((double)twips) * (1.0 / 1440.0) * source.DpiY);
        }


        void UploadImage(string address, string imagepath, NameValueCollection values)
        {
            using (var stream = File.Open(imagepath, FileMode.Open))
            {
                var files = new[] 
                    {
                        new UploadFile
                        {
                            Name = "file",
                            Filename = Path.GetFileName(imagepath),
                            ContentType = "text/plain",
                            Stream = stream
                        }
                    };

               string result = UploadFiles(address, files, values);
            }
        }
        public class UploadFile
        {
            public UploadFile()
            {
                ContentType = "application/octet-stream";
            }
            public string Name { get; set; }
            public string Filename { get; set; }
            public string ContentType { get; set; }
            public Stream Stream { get; set; }
        }

        public string UploadFiles(string address, IEnumerable<UploadFile> files, NameValueCollection values)
        {
            var request = WebRequest.Create(address);
            request.Method = "POST";
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;

            using (var requestStream = request.GetRequestStream())
            {
                // Write the values
                foreach (string name in values.Keys)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(values[name] + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                // Write the files
                foreach (var file in files)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", file.Name, file.Filename, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", file.ContentType, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    file.Stream.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
            }

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var stream = new MemoryStream())
            {
                responseStream.CopyTo(stream);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }

}
