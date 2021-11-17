using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class MapView : UserControl
    {
        // 各圖層
        private BingMapsLayer bingMaps;
        private TownLayer town = new();
        private MrtLayer mrt = new();
        private LandmarkLayer mark = new();

        public int level = 1;
        public int offset_x;
        public int offset_y;
        public double latitude;
        public double longitude;

        public event EventHandler MouseMovePoint;   // 自寫的事件，將MapView的MouseMove事件傳到Form1

        private bool LBDown;
        private int LBDown_x;
        private int LBDown_y;

        private Rectangle rubberBand = new Rectangle(0, 0, 0, 0);
        private readonly Pen pen = new Pen(Color.Red, 3);
        public bool mouseMode;

        private double coordinate_latitude = -200;
        private double coordinate_longitude = -200;
        private readonly Image coordinate_image = Image.FromFile(string.Format(@"{0}\resource\locate.png", Environment.CurrentDirectory));

        public bool IsShowTown;
        public bool IsShowMark;
        public bool IsShowMRT;
        private bool IsOnMarkStatus;
        public MapView()
        {
            InitializeComponent();
            Graphics g = CreateGraphics();
            bingMaps = new BingMapsLayer();
            this.bingMaps.RefreshPainting += new System.EventHandler(this.JustRefresh);
        }

        private void MapView_Load(object sender, EventArgs e)
        {
        }

        private void MapView_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            bingMaps.Draw(g);

            if (level >= 9)
            {
                if (IsShowTown)
                {
                    town.level = level;
                    town.offset_x = offset_x;
                    town.offset_y = offset_y;
                    town.Draw(g);
                }

                if (IsShowMark)
                {
                    mark.level = level;
                    mark.offset_x = offset_x;
                    mark.offset_y = offset_y;
                    mark.Draw(g);
                }

                if (IsShowMRT)
                {
                    mrt.level = level;
                    mrt.offset_x = offset_x;
                    mrt.offset_y = offset_y;
                    mrt.Draw(g);
                }
            }

            if (mouseMode)
            {
                g.DrawRectangle(pen, rubberBand);   //rubber band繪圖
            }

            if (coordinate_latitude != -200 && coordinate_longitude != -200)
            {
                // 座標繪圖
                int x, y;
                TileSystem.LatLongToPixelXY(coordinate_latitude, coordinate_longitude, level, out x, out y);
                g.DrawImage(coordinate_image, x + offset_x - 22, y + offset_y - 80);
            }
        }

        private void MapView_MouseMove(object sender, MouseEventArgs e)
        {
            if (LBDown) // 滑鼠左鍵按著
            {
                if (mouseMode)  // 使用rubber band
                {
                    rubberBand.Width = e.X - LBDown_x;
                    rubberBand.Height = e.Y - LBDown_y;
                    if (rubberBand.Width < 0)
                    {
                        rubberBand.X = e.X;
                        rubberBand.Width = -rubberBand.Width;
                    }
                    if (rubberBand.Height < 0)
                    {
                        rubberBand.Y = e.Y;
                        rubberBand.Height = -rubberBand.Height;
                    }
                }
                else            // 若無則是平移畫面
                {
                    bingMaps.offset_x += e.X - LBDown_x;
                    bingMaps.offset_y += e.Y - LBDown_y;
                    offset_x += e.X - LBDown_x;
                    offset_y += e.Y - LBDown_y;

                    bingMaps.pixel_x = e.X - offset_x;
                    bingMaps.pixel_y = e.Y - offset_y;

                    LBDown_x = e.X;
                    LBDown_y = e.Y;
                }

                this.Refresh();
            }
            else    // 沒有按著左鍵則是顯示mark資訊
            {
                bool IsOnMark = false;
                if (IsShowMark && level >= 9)
                {
                    foreach (LandmarkInfo info in mark.landmarkInfos)
                    {
                        if (e.X - offset_x >= info.x - 8 && e.X - offset_x <= info.x + 4 && e.Y - offset_y >= info.y - 18 && e.Y - offset_y <= info.y - 4)
                        {
                            if (!IsOnMarkStatus)
                            {
                                toolTip1.Show(info.info, this, e.X, e.Y);
                                IsOnMarkStatus = true;
                            }
                            IsOnMark = true;
                            break;
                        }
                    }

                    if (!IsOnMark)
                    {
                        toolTip1.Hide(this);
                        IsOnMarkStatus = false;
                    }
                }
            }

            LatLong latlong = new();
            TileSystem.PixelXYToLatLong(e.X - offset_x, e.Y - offset_y, level, out latlong.latitude, out latlong.longitude);
            MouseMovePoint.Invoke(latlong, null);
        }
         public struct LatLong
        {
            public double latitude;
            public double longitude;
        }
        private void MapView_MouseWheel(object sender, MouseEventArgs e)
        {
            TileSystem.PixelXYToLatLong(e.X - offset_x, e.Y - offset_y, level, out latitude, out longitude);
            if (e.Delta == 120 && level < 19)
            {
                level++;
                bingMaps.level++;
            }
            else if (e.Delta == -120 && level > 1)
            {
                level--;
                bingMaps.level--;
            }
            else
            {
                return;
            }

            this.SetPixelAndOffset(e.X, e.Y);

            this.Refresh();
        }

        private void MapView_MouseDown(object sender, MouseEventArgs e)
        {
            if (mouseMode)
            {
                rubberBand.X = e.X;
                rubberBand.Y = e.Y;
            }

            LBDown_x = e.X;
            LBDown_y = e.Y;

            LBDown = true;
        }

        private void MapView_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseMode)
            {
                this.ZoomOutInArea(rubberBand);

                rubberBand.X = 0;
                rubberBand.Y = 0;
                rubberBand.Width = 0;
                rubberBand.Height = 0;

                this.Refresh();
            }
            LBDown = false;
        }

        public void MapView_ZoomIn()
        {
            TileSystem.PixelXYToLatLong(256 - offset_x, 256 - offset_y, level, out latitude, out longitude);
            if (level < 19)
            {
                level++;
                bingMaps.level++;
            }
            else return;

            this.SetPixelAndOffset();

            this.Refresh();
        }

        public void MapView_ZoomOut()
        {
            TileSystem.PixelXYToLatLong(256 - offset_x, 256 - offset_y, level, out latitude, out longitude);
            if (level > 1)
            {
                level--;
                bingMaps.level--;
            }
            else return;

            this.SetPixelAndOffset();

            this.Refresh();
        }

        private void SetPixelAndOffset(int x = 255, int y = 255)
        {
            int pixel_x, pixel_y;
            TileSystem.LatLongToPixelXY(latitude, longitude, level, out pixel_x, out pixel_y);

            offset_x = -pixel_x + x;
            offset_y = -pixel_y + y;
            bingMaps.offset_x = -pixel_x + x;
            bingMaps.offset_y = -pixel_y + y;
            bingMaps.pixel_x = pixel_x;
            bingMaps.pixel_y = pixel_y;
        }

        public void MapView_ZoomToKaohsiung()
        {
            latitude = 22.83;
            longitude = 120.5;

            level = 9;
            bingMaps.level = 9;

            this.SetPixelAndOffset();

            this.Refresh();
        }

        private void ZoomOutInArea(Rectangle rect)
        {
            int pixel_x = rect.X + rect.Width / 2 - offset_x;
            int pixel_y = rect.Y + rect.Height / 2 - offset_y;

            TileSystem.PixelXYToLatLong(pixel_x, pixel_y, level, out latitude, out longitude);

            level++;
            bingMaps.level++;

            this.SetPixelAndOffset();

            this.Refresh();
        }

        public void SetCoordinate(double lati, double longi)
        {
            coordinate_latitude = lati;
            coordinate_longitude = longi;

            if (lati == -200 && longi == -200)
            {
                this.Refresh();
                return;
            }

            latitude = lati;
            longitude = longi;

            this.SetPixelAndOffset();

            this.Refresh();
        }

        public void ChangeBingMapLayer(string layer)
        {
            bingMaps.layer = layer;

            this.Refresh();
        }

        public void PrintDraw(Graphics g)
        {
            bingMaps.Draw(g);

            if (level >= 9)
            {
                if (IsShowTown)
                {
                    town.level = level;
                    town.offset_x = offset_x;
                    town.offset_y = offset_y;
                    town.Draw(g);
                }

                if (IsShowMark)
                {
                    mark.level = level;
                    mark.offset_x = offset_x;
                    mark.offset_y = offset_y;
                    mark.Draw(g);
                }

                if (IsShowMRT)
                {
                    mrt.level = level;
                    mrt.offset_x = offset_x;
                    mrt.offset_y = offset_y;
                    mrt.Draw(g);
                }
            }

            if (coordinate_latitude != -200 && coordinate_longitude != -200)
            {
                // 座標繪圖
                int x, y;
                TileSystem.LatLongToPixelXY(coordinate_latitude, coordinate_longitude, level, out x, out y);
                g.DrawImage(coordinate_image, x + offset_x - 22, y + offset_y - 80);
            }
        }

        public void MoveToPosition(double lati, double longi, int l)
        {
            latitude = lati;
            longitude = longi;
            level = l;
            bingMaps.level = l;

            this.SetPixelAndOffset();

            this.Refresh();
        }

        private void JustRefresh(object sender, EventArgs e)
        {
            InvokeJustRefresh re = delegate { this.Refresh(); };
            this.Invoke(re);
        }

        private delegate void InvokeJustRefresh();
    }
}