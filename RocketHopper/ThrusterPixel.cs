using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketHopper
{
    class ThrusterPixel
    {
        int xPos;
        int yPos;
        int initX;
        int initY;
        int direction; // 0 UP 1 RIGHT 2 DOWN 3 LEFT

        public ThrusterPixel(int x, int y, int dir)
        {
            initX = x;
            initY = y;
            xPos = x;
            yPos = y;
            direction = dir;
        }

        public int XPos
        {
            get { return xPos; }
            set { xPos = value; }
        }
        public int YPos
        {
            get { return yPos; }
            set { yPos = value; }
        }
        public int InitX
        {
            get { return initX; }
        }
        public int InitY
        {
            get { return initY; }
        }
        public int Direction
        {
            get { return direction; }
        }
    }
}
