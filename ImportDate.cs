using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public static class ImportDate
    {
        public static void GetDataIntoDatabase()
        {
            SqlConnection sql = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\User\Desktop\C#地檢視器\WinFormsApp1\LandmarkDataBase.mdf;Integrated Security=True");
            SqlCommand cmd = new SqlCommand();

            Dictionary<int, double[]> lines_geo = new Dictionary<int, double[]>();
            Dictionary<int, string[]> lines_csv = new Dictionary<int, string[]>();

            using (StreamReader reader1 = new StreamReader(@"C:\Users\User\Desktop\C#地檢視器\WinFormsApp1\resource\Khsc_landmark.geo"))
            {
                string line1;
                line1 = reader1.ReadLine();
                while (line1 != null)
                {
                    double[] temp = Array.ConvertAll<string, double>(line1.Split(','), value => Convert.ToDouble(value));
                    lines_geo.Add((int)temp[0], temp);
                    line1 = reader1.ReadLine();
                }
                reader1.Close();
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            using (StreamReader reader2 = new StreamReader(@"C:\Users\User\Desktop\C#地檢視器\WinFormsApp1\resource\Khsc_landmark.csv", Encoding.GetEncoding(950)))
            {
                string line2;
                line2 = reader2.ReadLine();
                while (line2 != null)
                {
                    string[] str = line2.Split(',');
                    lines_csv.Add(Convert.ToInt32(str[0]), str);
                    line2 = reader2.ReadLine();
                }
                reader2.Close();
            }
            
            cmd.Connection = sql;

            foreach (KeyValuePair<int, double[]> line in lines_geo)
            {
                double latitude = line.Value[2];
                double longitude = line.Value[1];
                int type = Convert.ToInt32(lines_csv[line.Key][2]);

                string info = "";
                int i = 0;
                foreach(string str in lines_csv[line.Key])
                {
                    if(i >= 3)
                    {
                        info += str + "\n";
                    }
                    i++;
                }
                info.TrimEnd('\n');

                int mid = line.Key;

                cmd.CommandText = string.Format("INSERT INTO Landmark(latitude, longitude, type, info, mid) VALUES('{0}', '{1}', '{2}', N'{3}', '{4}')", latitude, longitude, type, info, mid);
                sql.Open();
                cmd.ExecuteNonQuery();
                sql.Close();
            }
        }
    }
}
