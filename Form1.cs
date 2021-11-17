using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private PrintPreviewDialog printPreviewDialog1 = new PrintPreviewDialog();
        private PrintDocument printDocument1 = new PrintDocument();

        private SqlConnection sql = new SqlConnection(string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0}\resource\UserDatabase.mdf;Integrated Security=True", Environment.CurrentDirectory));
        private SqlCommand cmd = new SqlCommand();

        public Form1()
        {
            InitializeComponent();
            
            if(Program.IsLogin)
            {
                cmd.Connection = sql;
                cmd.CommandText = string.Format("SELECT latitude, longitude, levelOfDetail FROM Users WHERE name = '{0}'", Program.name);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                sql.Open();
                da.Fill(dt);
                sql.Close();

                mapView1.MoveToPosition((double)dt.Rows[0]["latitude"], (double)dt.Rows[0]["longitude"], (int)dt.Rows[0]["levelOfDetail"]);
            }
        }

        // 將MapView的MouseMove事件傳到Form1，並且改狀態列的經緯度。
        private void mapView1_MouseMovePoint(object sender, EventArgs e)
        {
            MapView.LatLong latlong = (MapView.LatLong)sender;
            this.toolStripStatusLabel1.Text = latlong.latitude.ToString();
            this.toolStripStatusLabel2.Text = latlong.longitude.ToString();
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Program.IsLogin)
            {
                double lati, longi;
                TileSystem.PixelXYToLatLong(256 - mapView1.offset_x, 256 - mapView1.offset_y, mapView1.level, out lati, out longi);

                cmd.Connection = sql;
                cmd.CommandText = string.Format("UPDATE Users SET latitude = {0}, longitude = {1}, levelOfDetail = {2} WHERE name = '{3}'", lati, longi, mapView1.level, Program.name);

                sql.Open();
                cmd.ExecuteNonQuery();
                sql.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mapView1.MapView_ZoomIn();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mapView1.MapView_ZoomOut();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            mapView1.MapView_ZoomToKaohsiung();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            mapView1.mouseMode = !mapView1.mouseMode;
            if(mapView1.mouseMode)
            {
                button4.BackColor = Color.FromName("ActiveCaption");
            }
            else
            {
                button4.BackColor = Color.FromName("ActiveBorder");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            if (text != "" && text.Contains(','))
            {
                string[] point = text.Split(',');

                double x, y;
                if(double.TryParse(point[0], out x) && double.TryParse(point[1], out y))
                {
                    if (x >= -90 && x <= 90 && y >= -180 && y <= 180)
                        mapView1.SetCoordinate(x, y);
                    else
                        MessageBox.Show("超出搜尋範圍!!");
                }
                else MessageBox.Show("輸入格式錯誤!!");
            }
            else MessageBox.Show("輸入格式錯誤!!");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void aLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapView1.ChangeBingMapLayer("a");
            aLayerToolStripMenuItem.Checked = true;
            rLayerToolStripMenuItem.Checked = false;
            hLayerToolStripMenuItem.Checked = false;
        }

        private void rLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapView1.ChangeBingMapLayer("r");
            aLayerToolStripMenuItem.Checked = false;
            rLayerToolStripMenuItem.Checked = true;
            hLayerToolStripMenuItem.Checked = false;
        }

        private void hLayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mapView1.ChangeBingMapLayer("h");
            aLayerToolStripMenuItem.Checked = false;
            rLayerToolStripMenuItem.Checked = false;
            hLayerToolStripMenuItem.Checked = true;
        }

        private void 預覽列印ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CaptureScreen();
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
        }

        Bitmap memoryImage;
        private void CaptureScreen()
        {
            memoryImage = new Bitmap(512, 512);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            mapView1.PrintDraw(memoryGraphics);
        }

        private void printDocument1_PrintPage(System.Object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(memoryImage, 0, 0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            mapView1.SetCoordinate(-200,-200);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            mapView1.IsShowTown = checkBox1.Checked;
            mapView1.Refresh();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            mapView1.IsShowMark = checkBox2.Checked;
            mapView1.Refresh();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            mapView1.IsShowMRT = checkBox3.Checked;
            mapView1.Refresh();
        }
    }
}
