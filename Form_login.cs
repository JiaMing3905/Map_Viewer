using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form_login : Form
    {
        private SqlConnection sql = new SqlConnection(string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0}\resource\UserDatabase.mdf;Integrated Security=True", Environment.CurrentDirectory));
        private SqlCommand cmd = new SqlCommand();
        public Form_login()
        {
            InitializeComponent();
        }

        private void Form_login_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            string password = textBox2.Text;

            if(name == "" || password == "")
            {
                MessageBox.Show("請勿空白");
                return;
            }

            cmd.Connection = sql;
            cmd.CommandText = "SELECT name, password FROM Users";

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            sql.Open();
            da.Fill(dt);
            sql.Close();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (name == (string)dt.Rows[i]["name"])
                {
                    if (password == (string)dt.Rows[i]["password"])
                    {
                        Program.name = (string)dt.Rows[i]["name"];
                        Program.IsLogin = true;
                        Program.IsOpen = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("密碼錯誤");
                        return;
                    }
                }
            }
            MessageBox.Show("帳號不存在");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string name = textBox1.Text;
            string password = textBox2.Text;

            if (name == "" || password == "")
            {
                MessageBox.Show("請勿空白");
                return;
            }

            cmd.Connection = sql;
            cmd.CommandText = "SELECT name FROM Users";

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            sql.Open();
            da.Fill(dt);
            sql.Close();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (name == (string)dt.Rows[i]["name"])
                {
                    MessageBox.Show("帳號已經存在");
                    return;
                }
            }

            cmd.CommandText = string.Format("INSERT INTO Users(name, password, latitude, longitude, levelOfDetail) VALUES('{0}', '{1}', 0, 0, 1)", name, password);
            sql.Open();
            cmd.ExecuteNonQuery();
            sql.Close();
            MessageBox.Show("創建完成，請登入");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Program.IsOpen = true;
            this.Close();
        }
    }
}
