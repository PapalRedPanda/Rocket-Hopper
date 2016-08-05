using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocketHopper
{
    class Rocket
    {
        int xPos;
        int yPos;
        int size;
        int direction;

        bool hit;

        public Rocket(int x, int y)
        {
            xPos = x;
            yPos = y;
            size = 1;
            hit = false;
            direction = 0; //up
        }

        public int Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public int XPos
        {
            get
            {
                return xPos;
            }
            set
            {
                xPos = value;
            }
        }

        public int YPos
        {
            get
            {
                return yPos;
            }
            set
            {
                yPos = value;
            }
        }

        public bool Hit
        {
            get
            {
                return hit;
            }
            set
            {
                hit = value;
            }
        }
    }
}
