using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Media;
using System.Threading.Tasks;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace RocketHopper
{
    class PlayerShip
    {
        double xPosShip;
        double yPosShip;
        double xVelShip;
        double yVelShip;

        SoundPlayer rocketLaunch;
        SoundPlayer shipHit;

        int halfShipPixels;
        int ignitionStack;
        double gravityForce;

        bool hit;

        ArrayList rockets;
        ArrayList thrusterPixels;

        Stopwatch upWatch;
        Stopwatch downWatch;
        Stopwatch rightWatch;
        Stopwatch leftWatch;
        Stopwatch fireWatch;

        Thread movementThread;
        Thread rocketThread;
        Thread thrusterPixelsThread;

        public PlayerShip(int x, int y, int size, double gravity)
        {
            xPosShip = x;
            yPosShip = y;
            ignitionStack = 0;
            halfShipPixels = size;
            gravityForce = gravity;

            rockets = new ArrayList();
            thrusterPixels = new ArrayList();

            movementThread = new Thread(updateMovement);
            movementThread.Start();
            rocketThread = new Thread(updateRockets);
            rocketThread.Start();
            thrusterPixelsThread = new Thread(updateThrusterPixels);
            thrusterPixelsThread.Start();

            upWatch = new Stopwatch();
            upWatch.Start();
            downWatch = new Stopwatch();
            downWatch.Start();
            leftWatch = new Stopwatch();
            leftWatch.Start();
            rightWatch = new Stopwatch();
            rightWatch.Start();
            fireWatch = new Stopwatch();
            fireWatch.Start();
            hit = false;

            //rocketLaunch = new SoundPlayer(Properties.Resources.RocketFire);
            //shipHit = new SoundPlayer(Properties.Resources.ShipHit);
        }

        private void updateMovement()
        {
            while (true)
            {
                xPosShip = xPosShip + xVelShip;
                yPosShip = yPosShip + yVelShip;

                yVelShip = yVelShip + gravityForce;

                xVelShip = xVelShip * (0.975);
                yVelShip = yVelShip * (0.975);
                

                Thread.Sleep(30);
            }
        }

        public void flipVelocity(bool x)
        {
            switch (x)
            {
                case true:
                    xVelShip = -xVelShip;
                    break;
                case false:
                    yVelShip = -yVelShip;
                    break;
            }
        }

        public void boost(int direction) // 0 UP, 1 RIGHT, 2 DOWN, 3 LEFT
        {
            switch (direction)
            {
                case 0:
                    yVelShip = yVelShip - 8;
                    break;
                case 1:
                    xVelShip = xVelShip + 8;
                    break;
                case 2:
                    yVelShip = yVelShip + 8;
                    break;
                case 3:
                    xVelShip = xVelShip - 8;
                    break;
            }
            fireThrusterPixel(direction);
        }

        public void fireRocket()
        {
            Rocket newRocket1 = new Rocket((int)(xPosShip - halfShipPixels), (int)yPosShip);
            Rocket newRocket2 = new Rocket((int)(xPosShip + halfShipPixels), (int)yPosShip);
            rockets.Add(newRocket1);
            rockets.Add(newRocket2);

            //rocketLaunch.Play();

            yVelShip = yVelShip + 4;

            ignitionStack++;

            halfShipPixels--;
        }

        private void updateRockets()
        {
            while (true)
            {
                if (rockets.Count > 0)
                {
                    int count = 0;
                    bool done = false;
                    Rocket r;
                    while (!done && rockets.Count > 0)
                    {
                        r = (Rocket)rockets[count];
                        if (r.Hit == true)
                        {
                            rockets.RemoveAt(count);
                        }
                        else if (r.YPos < 0)
                        {
                            rockets.RemoveAt(count);
                        }
                        else
                        {
                            r.YPos = r.YPos - 10;
                            count++;
                        }
                        if (count >= rockets.Count)
                        {
                            done = true;
                        }
                    }
                }
                Thread.Sleep(30);
            }
            
        }

        private void fireThrusterPixel(int direction)
        {
            ThrusterPixel newPixel;

            if (direction == 0)
            {
                newPixel = new ThrusterPixel((int)xPosShip, (int)(yPosShip + halfShipPixels), direction);
            }
            else if (direction == 1)
            {
                newPixel = new ThrusterPixel((int)(xPosShip - halfShipPixels), (int)yPosShip, direction);
            }
            else if (direction == 2)
            {
                newPixel = new ThrusterPixel((int)xPosShip, (int)(yPosShip - halfShipPixels), direction);
            }
            else
            {
                newPixel = new ThrusterPixel((int)(xPosShip + halfShipPixels), (int)yPosShip, direction);
            }

            thrusterPixels.Add(newPixel);
        }

        private void updateThrusterPixels()
        {
            while (true)
            {
                if (thrusterPixels.Count > 0)
                {
                    int count = 0;
                    bool done = false;
                    ThrusterPixel pixel;

                    while (!done && thrusterPixels.Count > 0)
                    {
                        pixel = (ThrusterPixel)thrusterPixels[count];

                        if (pixel.Direction == 0) // ship up, pixel down
                        {
                            if (pixel.YPos - pixel.InitY > 50)
                            {
                                thrusterPixels.RemoveAt(count);
                            }
                            else
                            {
                                pixel.YPos = pixel.YPos + 2;
                                count++;
                            }
                        }
                        if (pixel.Direction == 1) // ship right, pixel left
                        {
                            if (pixel.InitX - pixel.XPos > 50)
                            {
                                thrusterPixels.RemoveAt(count);
                            }
                            else
                            {
                                pixel.XPos = pixel.XPos - 3;
                                count++;
                            }
                        }
                        if (pixel.Direction == 2) // ship down, pixel up
                        {
                            if (pixel.InitY - pixel.YPos > 50)
                            {
                                thrusterPixels.RemoveAt(count);
                            }
                            else
                            {
                                pixel.YPos = pixel.YPos - 2;
                                count++;
                            }
                        }
                        if (pixel.Direction == 3) // ship left, pixel right
                        {
                            if (pixel.XPos - pixel.InitX > 50)
                            {
                                thrusterPixels.RemoveAt(count);
                            }
                            else
                            {
                                pixel.XPos = pixel.XPos + 3;
                                count++;
                            }
                        }
                        if (count >= thrusterPixels.Count)
                        {
                            done = true;
                        }
                    }
                }
                Thread.Sleep(30);
            }
        }

        public ArrayList Rockets
        {
            get { return rockets; }
        }

        public ArrayList ThrusterPixels
        {
            get { return thrusterPixels; }
        }

        public double XVelShip
        {
            get { return xVelShip; }
            set { xVelShip = value; }
        }

        public double YVelShip
        {
            get { return yVelShip; }
            set { yVelShip = value; }
        }

        public double XPosShip
        {
            get { return xPosShip; }
            set { xPosShip = value; }
        }

        public double YPosShip
        {
            get { return yPosShip; }
            set { yPosShip = value; }
        } 

        public int HalfShippixels
        {
            get { return halfShipPixels; }
            set { halfShipPixels = value; }
        }

        public int HitboxTop
        {
            get { return (int) (yPosShip - halfShipPixels); }
        }

        public int HitboxRight
        {
            get { return (int) (xPosShip + halfShipPixels); }
        }

        public int HitboxBottom
        {
            get { return (int) (yPosShip + halfShipPixels); }
        }

        public int HitboxLeft
        {
            get { return (int) (xPosShip - halfShipPixels); }
        }

        public bool Hit
        {
            get { return hit; }
            set
            {
                hit = value;
            }
        }
    }
}
