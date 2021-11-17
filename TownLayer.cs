using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    class TownLayer
    {
        private List<double[]> lines_geo = new List<double[]>();
        private Dictionary<int, string> lines_csv = new Dictionary<int, string>();

        public int level = 1;
        public int offset_x;
        public int offset_y;

        private readonly Pen pen = new Pen(Color.Blue, 3);
        private readonly SolidBrush brush = new SolidBrush(Color.FromArgb(60, 0, 0, 255));
        private readonly Font font = new Font(FontFamily.GenericSansSerif, 12.0F, FontStyle.Regular);
        public TownLayer()
        {
            using (StreamReader reader1 = new StreamReader(string.Format(@"{0}\resource\Khsc_town.geo", Environment.CurrentDirectory)))
            {
                string line1;
                line1 = reader1.ReadLine();
                while (line1 != null)
                {
                    lines_geo.Add(Array.ConvertAll<string, double>(line1.Split(','), value => Convert.ToDouble(value)));
                    line1 = reader1.ReadLine();
                }
                reader1.Close();
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (StreamReader reader2 = new StreamReader(string.Format(@"{0}\resource\Khsc_town.csv", Environment.CurrentDirectory), Encoding.GetEncoding(950)))
            {
                string line2;
                line2 = reader2.ReadLine();
                while (line2 != null)
                {
                    string[] str = line2.Split(',');
                    lines_csv.Add(Convert.ToInt32(str[0]), str[4]);
                    line2 = reader2.ReadLine();
                }
                reader2.Close();
            }
        }

        public void Draw(Graphics g)
        {
            Dictionary<int, List<Point>> towns = new Dictionary<int, List<Point>>();

            foreach (double[] line in lines_geo)
            {
                int town = Convert.ToInt32(line[1]);

                List<Point> temp;
                if (!towns.TryGetValue(town, out temp)) towns.Add(town, new List<Point>());

                for (int i = 3; i < line.Length; i += 2)
                {
                    int x, y;
                    TileSystem.LatLongToPixelXY(line[i + 1], line[i], level, out x, out y);
                    towns[town].Add(new Point(x + offset_x, y + offset_y));
                }
            }
            
            foreach(KeyValuePair<int, List<Point>> town in towns)
            {
                g.FillPolygon(brush, town.Value.ToArray());
                g.DrawPolygon(pen, town.Value.ToArray());

                g.DrawString(lines_csv[town.Key], font, new SolidBrush(Color.White), GetMidPoint(town.Value.ToArray()));
            }
        }

        private Point GetMidPoint(Point[] points)
        {
            double s_x = points[0].X;
            double s_y = points[0].Y;
            double l_x = points[0].X;
            double l_y = points[0].Y;

            foreach (Point p in points)
            {
                s_x = Math.Min(p.X, s_x);
                s_y = Math.Min(p.Y, s_y);
                l_x = Math.Max(p.X, l_x);
                l_y = Math.Max(p.Y, l_y);
            }

            return new Point((int)((s_x + l_x) / 2) - 30, (int)((s_y + l_y) / 2) - 6);
        }
    }
}
