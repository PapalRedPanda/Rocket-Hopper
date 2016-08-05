using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketHopper
{
    class TastyPixel
    {
        int xPos;
        int yPos;

        public TastyPixel(int x, int y)
        {
            xPos = x;
            yPos = y;
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
    }
}
