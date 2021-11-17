using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace WinFormsApp1
{
    class MrtLayer
    {
        private List<double[]> lines_geo = new List<double[]>();
        private Dictionary<int, string> lines_csv = new Dictionary<int, string>();

        public int level = 1;
        public int offset_x;
        public int offset_y;

        private readonly Pen red = new Pen(Color.Red, 3);
        private readonly Pen orange = new Pen(Color.Orange, 3);
        public MrtLayer()
        {
            using (StreamReader reader1 = new StreamReader(string.Format(@"{0}\resource\Khsc_mrt.geo", Environment.CurrentDirectory)))
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
            using (StreamReader reader2 = new StreamReader(string.Format(@"{0}\resource\Khsc_mrt.csv", Environment.CurrentDirectory), Encoding.GetEncoding(950)))
            {
                string line2;
                line2 = reader2.ReadLine();
                while (line2 != null)
                {
                    string[] str = line2.Split(',');
                    lines_csv.Add(Convert.ToInt32(str[0]), str[3]);
                    line2 = reader2.ReadLine();
                }
                reader2.Close();
            }
        }

        public void Draw(Graphics g)
        {
            Dictionary<int, List<Point>> mrts = new Dictionary<int, List<Point>>();

            foreach (double[] line in lines_geo)
            {
                int mrt = Convert.ToInt32(line[0]);

                List<Point> temp;
                if (!mrts.TryGetValue(mrt, out temp)) mrts.Add(mrt, new List<Point>());

                for (int i = 2; i < line.Length; i += 2)
                {
                    int x, y;
                    TileSystem.LatLongToPixelXY(line[i + 1], line[i], level, out x, out y);
                    mrts[mrt].Add(new Point(x + offset_x, y + offset_y));
                }
            }

            foreach (KeyValuePair<int, List<Point>> mrt in mrts)
            {
                if(lines_csv[mrt.Key] == "紅線")
                {
                    g.DrawLines(red, mrt.Value.ToArray());
                }
                else
                {
                    g.DrawLines(orange, mrt.Value.ToArray());
                }
            }
        }
    }
}
