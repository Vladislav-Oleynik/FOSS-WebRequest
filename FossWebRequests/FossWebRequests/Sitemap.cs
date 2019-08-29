using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Data.SqlClient;
using System.Data;




namespace FossWebRequests
{
    class Sitemap
    {
        private WebClient myClient = new WebClient();
        private string CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database.mdf") + ";Integrated Security=True";
        private string URLString;
        private string[] siteMap;
        private XmlDocument MyXmlDocument = new XmlDocument();
        private SqlConnection mySqlConn;
        public List<string> myList = new List<string>();

        public Sitemap()
        {
            Connect();
            UpdateDBList();
        }

        private void Connect()
        {
            mySqlConn = new SqlConnection(CONNECTION_STRING);
             mySqlConn.Open();
        }

        public void Verification()
        {
            URLString = "https:";
            WebRequest myRequest = HttpWebRequest.Create("https://fossdoc.com/robots.txt");
            myRequest.Method = "HEAD";
            try
            {
                using (WebResponse myResponse = myRequest.GetResponse())
                {
                    using (Stream myStream = myClient.OpenRead("https://fossdoc.com/robots.txt"))
                    {
                        using (StreamReader reader = new StreamReader(myStream))
                        {
                            string line = reader.ReadToEnd();
                            siteMap = line.Split(':');
                            URLString += siteMap.Last();
                        }
                    }
                }
            }
            catch (WebException ex) { MessageBox.Show(ex.Message); }
        }

        public  void UploadDB()
        {
            MyXmlDocument.Load(URLString);
            XmlNodeList locList = MyXmlDocument.GetElementsByTagName("loc");
            XmlNodeList lastmodList = MyXmlDocument.GetElementsByTagName("lastmod");
            XmlNodeList priorityList = MyXmlDocument.GetElementsByTagName("priority");

            for (int i = 0; i < locList.Count; i++)
            {
                SqlCommand mySqlCommand = new SqlCommand("INSERT INTO [Sitemap] (loc, lastmod, priority)VALUES(@loc, @lastmod, @priority)", mySqlConn);
                mySqlCommand.Parameters.AddWithValue("loc", locList[i].InnerText);
                mySqlCommand.Parameters.AddWithValue("lastmod", lastmodList[i].InnerText);
                mySqlCommand.Parameters.AddWithValue("priority", priorityList[i].InnerText);
                 mySqlCommand.ExecuteNonQuery();
            }
            UpdateDBList();
        }

        public void ClearTable()
        {
            SqlCommand myTruncateSqlCommand = new SqlCommand("TRUNCATE TABLE [Sitemap]", mySqlConn);
             myTruncateSqlCommand.ExecuteNonQuery();
            UpdateDBList();
        }

        public void PublishRandomLoc()
        {
            List<int> myListInt = new List<int>();
            string pathLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            string fileName = Path.Combine(pathLog, string.Format("{0}_{1:dd.MM.yyy}.log", AppDomain.CurrentDomain.FriendlyName, DateTime.Now));
            string logText;
            string loc = null;
            Random rand = new Random();
            int randomNumber = 0;
            SqlDataReader mySqlDataReader2 = null;
            SqlCommand mySqlCommandList = new SqlCommand("SELECT Id FROM [Sitemap] WHERE sent = 'False'", mySqlConn);
            mySqlDataReader2 =  mySqlCommandList.ExecuteReader();

            while ( mySqlDataReader2.Read())
            {
                myListInt.Add(mySqlDataReader2.GetInt32(0));
            }
            MessageBox.Show(myListInt.Count.ToString());
                if (mySqlDataReader2 != null)
                    mySqlDataReader2.Close();
            if (myListInt.Count != 0)
            {
                SqlDataReader mySqlDataReader = null;
                randomNumber = rand.Next(1, myListInt.Count);
                SqlCommand mySqlCommandUpdate = new SqlCommand("UPDATE Sitemap SET [sent] = 'True' WHERE Id =" + myListInt[randomNumber], mySqlConn);
                mySqlDataReader = mySqlCommandUpdate.ExecuteReader();

                if (mySqlDataReader != null)
                    mySqlDataReader.Close();

                mySqlCommandUpdate = new SqlCommand("SELECT loc FROM [Sitemap] WHERE Id =" + myListInt[randomNumber], mySqlConn);
                mySqlDataReader = mySqlCommandUpdate.ExecuteReader();
                while (mySqlDataReader.Read())
                {
                    loc = mySqlDataReader.GetString(0);
                }

                logText = string.Format("\r\n[{0:dd.MM.yyy HH:mm:ss}] [{1}]This loc was tweeted", DateTime.Now, loc);
                MessageBox.Show(myListInt[randomNumber].ToString());

                if (mySqlDataReader != null)
                    mySqlDataReader.Close();

                if (!Directory.Exists(pathLog))
                    Directory.CreateDirectory(pathLog);
                File.AppendAllText(fileName, logText);
            }
            if (myListInt.Count == 0)
                MessageBox.Show("Database empty");
            UpdateDBList();
        }

        private  void UpdateDBList()
        {
            myList.Clear();
            SqlDataReader mySqlDataReader = null;
            SqlCommand mySqlCommandSelect = new SqlCommand("SELECT * FROM [Sitemap]", mySqlConn);

            try
            {
                mySqlDataReader =  mySqlCommandSelect.ExecuteReader();
                while ( mySqlDataReader.Read())
                {
                    myList.Add(mySqlDataReader["Id"].ToString() + "__" + mySqlDataReader["loc"].ToString() + "__" + mySqlDataReader["lastmod"].ToString() + "__" + mySqlDataReader["priority"].ToString() + "__" + mySqlDataReader["sent"].ToString());
                }
            }

            catch (Exception ex) { MessageBox.Show(ex.Message); }

            finally
            {
                if (mySqlDataReader != null)
                    mySqlDataReader.Close();
            }
        }

        public void ConnectionClose()
        {
            if (mySqlConn != null && mySqlConn.State != ConnectionState.Closed)
                mySqlConn.Close();
        }
    }
}
