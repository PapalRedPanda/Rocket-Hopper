using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

namespace RocketHopper
{
    class BadGuy
    {
        int xPos;
        int yPos;
        int halfShipPixels;
        int level;
        int currShot;

        ArrayList evilRockets;

        Stopwatch shotTimer1;
        Stopwatch shotTimer2;

        bool shotAvailable;

        int direction;

        Thread movement;
        Thread rocketThread;

        bool destroyed;
        bool whiteFlash;

        public BadGuy(int x, int y, int startSize, int startLevel)
        {
            xPos = x;
            yPos = y;
            halfShipPixels = startSize;

            evilRockets = new ArrayList();

            shotTimer1 = new Stopwatch();
            shotTimer1.Start();
            shotTimer2 = new Stopwatch();
            shotTimer2.Start();

            currShot = 0;

            //level = startLevel;
            level = startLevel;

            movement = new Thread(move);
            movement.Start();
            rocketThread = new Thread(updateRockets);
            rocketThread.Start();

            destroyed = false;
        }

        public ArrayList EvilRockets
        {
            get { return evilRockets; }
        }

        public int HitboxTop
        {
            get { return yPos - halfShipPixels; }
        }
        public int HitboxRight
        {
            get { return xPos + halfShipPixels; }
        }
        public int HitboxBottom
        {
            get { return yPos + halfShipPixels; }
        }
        public int HitboxLeft
        {
            get { return xPos - halfShipPixels; }
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

        public int HalfShipPixels
        {
            get { return halfShipPixels; }
            set { halfShipPixels = value; }
        }

        public void destroy()
        {
            destroyed = true;
        }

        public bool Destroyed
        {
            get { return destroyed; }
        }

        public bool WhiteFlash
        {
            get { return whiteFlash; }
            set { whiteFlash = value; }
        }

        public int Level
        {
            get { return level; } 
            set { level = value; }
        }

        private void move() //also shoot
        {
            while (!destroyed)
            {
                if (shotTimer1.ElapsedMilliseconds > 2000 && shotTimer2.ElapsedMilliseconds > 500)
                {
                    fireRocket();
                    shotTimer2.Restart();
                    currShot++;
                    if (currShot == level)
                    {
                        shotTimer1.Restart();
                        currShot = 0;
                    }
                }

                yPos = yPos + 2;
                Thread.Sleep(30);
            }
        }

        public void fireRocket()
        {
            Rocket newRocket1 = new Rocket(xPos, HitboxTop);
            newRocket1.Direction = 0;
            Rocket newRocket2 = new Rocket(HitboxRight, yPos);
            newRocket2.Direction = 1;
            Rocket newRocket3 = new Rocket(xPos, HitboxBottom);
            newRocket3.Direction = 2;
            Rocket newRocket4 = new Rocket(HitboxLeft, yPos);
            newRocket4.Direction = 3;

            evilRockets.Add(newRocket1);
            evilRockets.Add(newRocket2);
            evilRockets.Add(newRocket3);
            evilRockets.Add(newRocket4);
        }

        private void updateRockets()
        {
            while (true)
            {
                if (evilRockets.Count > 0)
                {
                    int count = 0;
                    bool done = false;
                    Rocket r;
                    while (!done && evilRockets.Count > 0)
                    {
                        r = (Rocket)evilRockets[count];
                        if (r.Hit == true)
                        {
                            evilRockets.RemoveAt(count);
                        }
                        else if (r.YPos < 0 || r.YPos > 700 || r.XPos < 0 || r.XPos > 950)
                        {
                            evilRockets.RemoveAt(count);
                        }
                        else
                        {
                            switch (r.Direction)
                            {
                                case 0: r.YPos = r.YPos - 5;
                                    break;
                                case 1: r.XPos = r.XPos + 5;
                                    break;
                                case 2: r.YPos = r.YPos + 5;
                                    break;
                                case 3: r.XPos = r.XPos - 5;
                                    break;
                            }
                            count++;
                        }
                        if (count >= evilRockets.Count)
                        {
                            done = true;
                        }
                    }
                }
                Thread.Sleep(30);
            }

        }
    }
}
