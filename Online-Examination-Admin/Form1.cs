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

namespace Online_Examination_Admin
{
    public partial class Form1 : Form
    {
        public const string ADDR = "http://localhost/online-examination/";
        public static string GetResponse(string addr)
        {
            WebRequest request = WebRequest.Create(addr);
            request.Credentials = CredentialCache.DefaultCredentials;
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return responseFromServer;
        }
        public Form1()
        {
            InitializeComponent();
            UpdateUI();

        }

        void UpdateUI()
        {
            cb_sets.Items.Clear();
            string setids = GetResponse(ADDR + "admin/showsetids.php");
            string[] lines = setids.Split(new string[] { "<br/>" }, StringSplitOptions.None);
            foreach (string setid in lines)
            {
                if (setid == "") continue;
                int id = Convert.ToInt32(setid);
                cb_sets.Items.Add(id);
            }
            if (cb_sets.Items.Count > 0) { cb_sets.SelectedIndex = 0; button1.Enabled = true; }
            else button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NameValueCollection data = new NameValueCollection();
            data["imagefolder"] = (cb_sets.Items.Count + 1).ToString() + "acnsj";
            string response = Question.Post(ADDR + "admin/newset.php", data);
            UpdateUI();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Question q = new Question((int)cb_sets.SelectedItem);
            q.Show();
        }
    }
}
