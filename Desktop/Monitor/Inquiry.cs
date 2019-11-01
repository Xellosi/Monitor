using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace monitor
{
    public partial class Inquiry : Form
    {
        //string connectstr = "Server=.\\SQLEXPRESS;Database=Database1;Integrated Security=false";
        DataTable table = new DataTable();
        public string query_string;
        public Inquiry()
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.Yes;
            button2.DialogResult = DialogResult.Yes;
        }
        public int f { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            string qry = "";
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    if (f == 1)
                    {
                        qry = qry + ",'" + checkedListBox1.Items[i].ToString() + "'";
                    }
                    else if (f == 0)
                    {
                        qry = "'" + checkedListBox1.Items[i].ToString() + "'";
                        f = 1;
                    }
                }
            }
            if (qry =="")
            {
                query_string = "select * from arduinodata WHERE Event IN (" + qry + ")";
            }
            else
            {
                query_string = "select * from arduinodata WHERE Event IN (" + qry + ")";
            }
            //string newq = "select * from arduinodata WHERE 偵測項目 IN ("+qry+")";
            //string selectstr = newq;
            //SqlConnection conn = new SqlConnection(connectstr);
            //SqlDataAdapter adapter = new SqlDataAdapter();
            //SqlCommand cmd = new SqlCommand(selectstr, conn);
            //adapter.SelectCommand = cmd;
            //adapter.Fill(table);
            //dataGridView1.DataSource = table;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            String time = textBox1.Text;
            string newq = "select * from arduinodata WHERE Time LIKE '%" + time+"%'";
            query_string = "select * from arduinodata WHERE Time LIKE '%" + time + "%'";
            string selectstr = newq;
            //SqlConnection conn = new SqlConnection(connectstr);
            SqlDataAdapter adapter = new SqlDataAdapter();
            //SqlCommand cmd = new SqlCommand(selectstr, conn);
            //adapter.SelectCommand = cmd;
            //adapter.Fill(table);
            //dataGridView1.DataSource = table;
            this.button2.DialogResult = DialogResult.Yes;
        }
    }
}
