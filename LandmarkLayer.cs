using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

namespace WinFormsApp1
{
    class LandmarkLayer
    {
        private SqlConnection sql = new SqlConnection(string.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0}\resource\LandmarkDataBase.mdf;Integrated Security=True", Environment.CurrentDirectory));
        private SqlCommand cmd = new SqlCommand();

        private readonly Image gas = Image.FromFile(string.Format(@"{0}\resource\gas.png", Environment.CurrentDirectory));
        private readonly Image mrt = Image.FromFile(string.Format(@"{0}\resource\mrt.png", Environment.CurrentDirectory));
        private readonly Image parking = Image.FromFile(string.Format(@"{0}\resource\parking.png", Environment.CurrentDirectory));
        private readonly Image school = Image.FromFile(string.Format(@"{0}\resource\school.png", Environment.CurrentDirectory));

        public int level = 1;
        public int offset_x;
        public int offset_y;

        public List<LandmarkInfo> landmarkInfos;
        public LandmarkLayer()
        {

        }

        public void Draw(Graphics g)
        {
            double tl_latitude, tl_longitude;
            double br_latitude, br_longitude;
            TileSystem.PixelXYToLatLong(-offset_x, -offset_y, level, out tl_latitude, out tl_longitude);
            TileSystem.PixelXYToLatLong(-offset_x + 512, -offset_y + 512, level, out br_latitude, out br_longitude);

            cmd.Connection = sql;
            cmd.CommandText = string.Format("SELECT * FROM Landmark WHERE latitude >= {0} AND latitude <= {1} AND longitude >= {2} AND longitude <= {3} AND (type = 405 OR type = 303 OR type = 201 OR type = 202 OR type = 203 OR type = 306)", br_latitude, tl_latitude, tl_longitude, br_longitude);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            sql.Open();
            da.Fill(dt);
            sql.Close();

            List<LandmarkInfo> temp = new List<LandmarkInfo>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int x, y;
                TileSystem.LatLongToPixelXY((double)dt.Rows[i]["latitude"], (double)dt.Rows[i]["longitude"], level, out x, out y);

                temp.Add(new LandmarkInfo(x, y, (string)dt.Rows[i]["info"]));

                switch ((int)dt.Rows[i]["type"])
                {
                    case 405:
                        g.DrawImage(gas, x + offset_x - 10, y + offset_y - 21);
                        break;
                    case 303:
                        g.DrawImage(parking, x + offset_x - 10, y + offset_y - 21);
                        break;
                    case 306:
                        g.DrawImage(mrt, x + offset_x - 10, y + offset_y - 21);
                        break;
                    default:
                        g.DrawImage(school, x + offset_x - 10, y + offset_y - 21);
                        break;
                }
            }

            landmarkInfos = temp;
        }
    }
}
