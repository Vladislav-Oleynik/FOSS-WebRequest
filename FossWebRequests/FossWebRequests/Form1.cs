using System;
using System.Windows.Forms;

namespace FossWebRequests
{
    public partial class Form1 : Form
    {
        Sitemap sitemapObj = new Sitemap();

        public Form1()
        {
            InitializeComponent();
            sitemapObj.Verification();
            UpdateListBox();
        }

        private void UpdateListBox()
        {
            listBox1.Items.Clear();
            foreach (string s in sitemapObj.myList)
                listBox1.Items.Add(s);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sitemapObj.UploadDB();
            UpdateListBox();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sitemapObj.ClearTable();
            UpdateListBox();
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sitemapObj.ConnectionClose();
        }
        
        private  void buttonPublish_Click(object sender, EventArgs e)
        {
            sitemapObj.PublishRandomLoc();
            UpdateListBox();
        }
    }
}
