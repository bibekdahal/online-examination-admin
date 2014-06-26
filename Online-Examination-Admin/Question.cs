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
using mshtml;

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
                
                NameValueCollection data = new NameValueCollection();
                data["setid"] = m_setid.ToString();
                data["qsn"] = (m_currentid + 1).ToString();
                int answer = Convert.ToInt32(Post(ADDR + "admin/getanswer.php", data));
                switch(answer)
                {
                    case 0: radioButton1.Checked = true; break;
                    case 1: radioButton2.Checked = true; break;
                    case 2: radioButton3.Checked = true; break;
                    case 3: radioButton4.Checked = true; break;
                }
            }
        }

        public void UpdateUI()
        {
            NameValueCollection data = new NameValueCollection();
            data["setid"] = m_setid.ToString();
            m_numquestions = Convert.ToInt32(Post(ADDR + "admin/getqns.php", data));
            btn_done.Visible = false;
            if (m_currentid >= m_numquestions)
            {
                btn_next.Enabled = false;
                btn_addupdate.Text = "Add";
                btn_smartpaste.Visible = true;
                btn_passage.Visible = false;
                rtb_question.Rtf = rtb_optiona.Rtf = rtb_optionb.Rtf = rtb_optionc.Rtf = rtb_optiond.Rtf = "";
                rtb_question.Visible = rtb_optiona.Visible = rtb_optionb.Visible = rtb_optionc.Visible = rtb_optiond.Visible = true;
                wb_question.Visible = wb_optiona.Visible = wb_optionb.Visible = wb_optionc.Visible = wb_optiond.Visible = false;
                radioButton1.Checked = true; radioButton2.Checked = radioButton3.Checked = radioButton4.Checked = false;
                btn_clear.Visible = true;
            }
            else
            {
                UpdateData();
                btn_next.Enabled = true;
                btn_addupdate.Text = "Update";
                btn_smartpaste.Visible = false;
                btn_passage.Visible = true;
                rtb_question.Visible = rtb_optiona.Visible = rtb_optionb.Visible = rtb_optionc.Visible = rtb_optiond.Visible = false;
                wb_question.Visible = wb_optiona.Visible = wb_optionb.Visible = wb_optionc.Visible = wb_optiond.Visible = true;
                btn_clear.Visible = false;
            }

            if (m_currentid <= 0) btn_prev.Enabled = false;
            else btn_prev.Enabled = true;

            label6.Text = "SetId: " + m_setid.ToString() + " S.N.: " + (m_currentid + 1).ToString();
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

        void CopyWB(WebBrowser wb, RichTextBox rtb)
        {
            string html = wb.DocumentText.Replace("&#8745;", "&#199;");
            html = html.Replace("&#8746;", "&#200;");
            wb.Navigate("about:blank");
            wb.Document.OpenNew(false);
            wb.Document.Write(html);
            wb.Refresh();

            IHTMLDocument2 htmlDoc = (IHTMLDocument2)wb.Document.DomDocument;
            IHTMLBodyElement Body = htmlDoc.body as IHTMLBodyElement;
            IHTMLTxtRange range = Body.createTextRange();
            range.select();
            
            Clipboard.Clear();
            range.execCommand("Copy", false, null);
            rtb.Paste();

            int x = 0;
            while (x < rtb.Text.Length)
            {
                rtb.Select(x, 1);
                if (rtb.SelectedRtf.Contains(@"\fs24\'c7") || rtb.SelectedRtf.Contains(@"\fs24\'c8"))
                    rtb.SelectionFont = new Font("Symbol", rtb.SelectionFont.Size, rtb.SelectionFont.Style, rtb.SelectionFont.Unit, 2);
                x++;
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

                NameValueCollection data2 = new NameValueCollection();
                data2["setid"] = m_setid.ToString();
                data2["qsn"] = (m_currentid + 1).ToString();
                if (radioButton1.Checked)
                    data2["option"] = "0";
                else if (radioButton2.Checked)
                    data2["option"] = "1";
                else if (radioButton3.Checked)
                    data2["option"] = "2";
                else if (radioButton4.Checked)
                    data2["option"] = "3";
                Post(ADDR + "admin/setanswer.php", data2);

                //MessageBox.Show(response);
                UpdateUI();
            }
            else
            {
                btn_addupdate.Visible = false;
                btn_next.Visible = false;
                btn_prev.Visible = false;
                btn_done.Visible = true;

                btn_smartpaste.Visible = true;
                btn_passage.Visible = false;
                rtb_question.Rtf = rtb_optiona.Rtf = rtb_optionb.Rtf = rtb_optionc.Rtf = rtb_optiond.Rtf = "";

                var clipdata = Clipboard.GetDataObject();

                CopyWB(wb_question, rtb_question);
                CopyWB(wb_optiona, rtb_optiona);
                CopyWB(wb_optionb, rtb_optionb);
                CopyWB(wb_optionc, rtb_optionc);
                CopyWB(wb_optiond, rtb_optiond);

                Clipboard.SetDataObject(clipdata);

                rtb_question.Visible = rtb_optiona.Visible = rtb_optionb.Visible = rtb_optionc.Visible = rtb_optiond.Visible = true;
                wb_question.Visible = wb_optiona.Visible = wb_optionb.Visible = wb_optionc.Visible = wb_optiond.Visible = false;
                btn_clear.Visible = true;
            
            }
        }


        private void btn_done_Click(object sender, EventArgs e)
        {
            NameValueCollection data = new NameValueCollection();
            data["setid"] = m_setid.ToString();
            data["qsn"] = (m_currentid + 1).ToString();
            Post(ADDR + "admin/deletequestion.php", data);

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
            Post(ADDR + "admin/addquestion.php", data);

            NameValueCollection data1 = new NameValueCollection();
            data1["setid"] = m_setid.ToString();
            string[] fileEntries = Directory.GetFiles(imgdir);
            foreach (string fileName in fileEntries)
                UploadImage(ADDR + "admin/addimage.php", fileName, data1);

            NameValueCollection data2 = new NameValueCollection();
            data2["setid"] = m_setid.ToString();
            data2["qsn"] = (m_currentid + 1).ToString();
            if (radioButton1.Checked)
                data2["option"] = "0";
            else if (radioButton2.Checked)
                data2["option"] = "1";
            else if (radioButton3.Checked)
                data2["option"] = "2";
            else if (radioButton4.Checked)
                data2["option"] = "3";
            Post(ADDR + "admin/setanswer.php", data2);

            btn_addupdate.Visible = true;
            btn_next.Visible = true;
            btn_prev.Visible = true;
            btn_done.Visible = false;
            
            UpdateUI();
        }

        public void InsertPassage(RichTextBox rtf)
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
            data["passage"] = ConvertRtfToHtml(rtf, imagefolder, (m_currentid + 1).ToString() + "psx");
            string response = Post(ADDR + "admin/addpassage.php", data);

            NameValueCollection data1 = new NameValueCollection();
            data1["setid"] = m_setid.ToString();
            string[] fileEntries = Directory.GetFiles(imgdir);
            foreach (string fileName in fileEntries)
                UploadImage(ADDR + "admin/addimage.php", fileName, data1);
        }

        public string ConvertRtfToHtml(RichTextBox rtf, string imagefolder, string imagefnameprefix)
        {
            //string imgdir = "D:/wamp/www/online-examination/";
            bool b = false, i = false, u = false, sup=false, sub=false;
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

                if (rtf.SelectedRtf.Contains(@"\super"))
                { if (!sup) { html += "<sup>"; sup = true; } }
                else if (sup)
                { html += "</sup>"; sup = false; }

                if (rtf.SelectedRtf.Contains(@"\sub"))
                { if (!sub) { html += "<sub>"; sub = true; } }
                else if (sub)
                { html += "</sub>"; sub = false; }

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
                        html += "<img src=\"" + imgpath + "\" style=\"vertical-align:middle;\"/>";
                    }

                }
                else if (rtf.SelectedText == "\n")
                    html += "<br/>";
                else if (rtf.SelectedText == "\t")
                    html += "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
                else
                    html += WebUtility.HtmlEncode(rtf.SelectedText);
                html = html.Replace("&#199;", "&#8745;");
                html = html.Replace("&#200;", "&#8746;");
                x++;
            }
            if (b) html += "</strong>";
            if (i) html += "</i>";
            if (u) html += "</u>";
            if (sup) html += "</sup>";
            if (sub) html += "</sub>";
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

        private void button1_Click(object sender, EventArgs e)
        {
            rtb_question.Paste();
            int id = rtb_question.Text.IndexOf("\ta)");
            rtb_question.Select(0, id);
            string question = rtb_question.SelectedRtf;
            int nid = rtb_question.Text.IndexOf("\tb)");
            rtb_question.Select(id + "\ta) ".Length, nid - (id + "\ta) ".Length));
            rtb_optiona.Rtf = rtb_question.SelectedRtf;

            id = nid;
            nid = rtb_question.Text.IndexOf("\tc)");
            rtb_question.Select(id + "\tb) ".Length, nid - (id + "\tb) ".Length));
            rtb_optionb.Rtf = rtb_question.SelectedRtf;

            id = nid;
            nid = rtb_question.Text.IndexOf("\td)");
            rtb_question.Select(id + "\tc) ".Length, nid - (id + "\tc) ".Length));
            rtb_optionc.Rtf = rtb_question.SelectedRtf;

            id = nid;
            rtb_question.Select(id + "\td) ".Length, rtb_question.Text.Length - (id + "\td) ".Length));
            rtb_optiond.Rtf = rtb_question.SelectedRtf;

            rtb_question.Rtf = question;
        }

        private void btn_passage_Click(object sender, EventArgs e)
        {
            Passage passage = new Passage(this);
            passage.ShowDialog();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (btn_addupdate.Text == "Add") return;
            NameValueCollection data = new NameValueCollection();
            data["setid"] = m_setid.ToString();
            data["qsn"] = (m_currentid + 1).ToString();
            data["option"] = "0";
            Post(ADDR + "admin/setanswer.php", data);
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (btn_addupdate.Text == "Add") return;
            NameValueCollection data = new NameValueCollection();
            data["setid"] = m_setid.ToString();
            data["qsn"] = (m_currentid + 1).ToString();
            data["option"] = "1";
            Post(ADDR + "admin/setanswer.php", data);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (btn_addupdate.Text == "Add") return;
            NameValueCollection data = new NameValueCollection();
            data["setid"] = m_setid.ToString();
            data["qsn"] = (m_currentid + 1).ToString();
            data["option"] = "2";
            Post(ADDR + "admin/setanswer.php", data);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (btn_addupdate.Text == "Add") return;
            NameValueCollection data = new NameValueCollection();
            data["setid"] = m_setid.ToString();
            data["qsn"] = (m_currentid + 1).ToString();
            data["option"] = "3";
            Post(ADDR + "admin/setanswer.php", data);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            rtb_question.Rtf = rtb_optiona.Rtf = rtb_optionb.Rtf = rtb_optionc.Rtf = rtb_optiond.Rtf = "";
        }


    }

}
