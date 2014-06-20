using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Online_Examination_Admin
{
    public partial class Passage : Form
    {
        Question m_questionform;
        public Passage(Question question)
        {
            InitializeComponent();
            m_questionform = question;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_questionform.InsertPassage(rtb_question);
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        


    }
}
