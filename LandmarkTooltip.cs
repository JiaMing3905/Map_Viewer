using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    class LandmarkInfo
    {
        public int x;
        public int y;
        public string info;
        public LandmarkInfo(int _x, int _y, string _i)
        {
            x = _x;
            y = _y;
            info = _i;
        }
    }
}
