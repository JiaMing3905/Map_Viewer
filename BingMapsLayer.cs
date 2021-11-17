using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;

namespace WinFormsApp1
{
    class BingMapsLayer
    {
        public event EventHandler RefreshPainting;  // 事件，為了使用MapView.refresh()

        private BackgroundWorker m_BackgroundWorker = new BackgroundWorker();   // 多執行序
        struct DrawCommand          // 建構下載以及放入Dictionary所需的資料
        {
            public string Url;
            public string quadKey;
        }

        private Queue<DrawCommand> m_DrawCommands = new Queue<DrawCommand>();

        // 存放圖片用的Dictionary
        private Dictionary<string, Image> m_Images = new Dictionary<string, Image>();

        // 以下是繪圖所需資訊
        public int level = 1;
        public int pixel_x;
        public int pixel_y;
        public int offset_x;
        public int offset_y;
        public string layer = "a";
        public BingMapsLayer()
        {
            // 最初的圖片先下載，其實也沒必要
            SuperWebClient wc = new SuperWebClient();
            m_Images.Add("a0", Image.FromStream(new MemoryStream(wc.DownloadData("https://ecn.t1.tiles.virtualearth.net/tiles/a0.jpeg?g=3649"))));
            m_Images.Add("a1", Image.FromStream(new MemoryStream(wc.DownloadData("https://ecn.t1.tiles.virtualearth.net/tiles/a1.jpeg?g=3649"))));
            m_Images.Add("a2", Image.FromStream(new MemoryStream(wc.DownloadData("https://ecn.t1.tiles.virtualearth.net/tiles/a2.jpeg?g=3649"))));
            m_Images.Add("a3", Image.FromStream(new MemoryStream(wc.DownloadData("https://ecn.t1.tiles.virtualearth.net/tiles/a3.jpeg?g=3649"))));
            
            // 新增背景下載任務
            m_BackgroundWorker.DoWork += M_BackgroundWorker_DoWork;
            m_BackgroundWorker.RunWorkerAsync();
        }
        private void M_BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)   // 若是沒有取消就會持續背景執行
            {
                if (m_DrawCommands.Count > 0)   // Queue若有任務排隊就執行
                {
                    DrawCommand cmd = m_DrawCommands.Dequeue(); // Dequeue
                    SuperWebClient wc = new SuperWebClient();
                    Image img = Image.FromStream(new MemoryStream(wc.DownloadData(cmd.Url)));   // 下載圖片

                    // 先檢查是否已經有圖片了(移動或是縮放速度太快會重複下載到同一張圖)，否則再次存取會出現例外狀況，使得此class失去作用
                    Image temp;
                    if(!m_Images.TryGetValue(layer + cmd.quadKey, out temp))
                        m_Images.Add(layer + cmd.quadKey, img);

                    // 事件，使MapView刷新畫面
                    RefreshPainting.Invoke(null, null);
                }
                else
                {
                    Thread.Sleep(100);  // 設定time sleep，不然迴圈會過度消耗資源
                }
            }
        }
        public void Draw(Graphics g)
        {
            // 將pixel轉成tile，再取得以此為中心的 5x5 tiles
            int tile_x, tile_y;
            TileSystem.PixelXYToTileXY(pixel_x, pixel_y, out tile_x, out tile_y);
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (tile_x + i < 0 || tile_x + i > (int)Math.Pow(2, level) - 1 || tile_y + j < 0 || tile_y + j > (int)Math.Pow(2, level) - 1)
                        continue; // 剔除邊界，不然會顯示錯誤的圖片

                    string quadKey = TileSystem.TileXYToQuadKey(tile_x + i, tile_y + j, level); // 取得此tile的quadKey

                    // 將此tile轉換成pixel座標
                    int x;
                    int y;
                    TileSystem.TileXYToPixelXY(tile_x + i, tile_y + j, out x, out y);

                    // 嘗試從Dictionary取得圖片，若無則下載
                    Image image;
                    if (m_Images.TryGetValue(layer + quadKey, out image))
                    {
                        Rectangle rect = new(x + offset_x, y + offset_y, 256, 256);
                        g.DrawImage(image, rect);
                    }
                    else
                    {
                        // 建立下載資訊的Structure
                        DrawCommand cmd = new DrawCommand();
                        cmd.Url = string.Format("https://ecn.t1.tiles.virtualearth.net/tiles/{0}{1}.jpeg?g=3649&shading=hill&stl=H", layer, quadKey);
                        cmd.quadKey = quadKey;

                        // 檢查Queue裡面是否有重複，若無就插入
                        // 如果不檢查就插入則會過度下載重複圖片，然後就卡死了
                        if (!m_DrawCommands.Contains(cmd)) m_DrawCommands.Enqueue(cmd);
                    }
                }
            }
        }
    }
}
