using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Media;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace RocketHopper
{
    public partial class Display : Form
    {
        PlayerShip player;

        ArrayList baddies;
        ArrayList floatingPixels;

        const double gravity = 0.1;
        const int topSpace = 50;

        bool flash;
        int flashCount;

        int currDisplay;

        Random random;

        Thread graphicsThread;
        Thread keyStrokeThread;
        Thread gameOverThread;
        Thread songOverThread;

        Stopwatch upWatch;
        Stopwatch downWatch;
        Stopwatch rightWatch;
        Stopwatch leftWatch;
        Stopwatch fireWatch;
        Stopwatch bounceWatch;
        Stopwatch musicTimer;

        bool canMoveUp;
        bool canMoveDown;
        bool canMoveLeft;
        bool canMoveRight;
        bool canFire;
        bool canBounce;
        bool gameOver;
        bool playerWin;

        SoundPlayer killerMusic;
        int musicLength;
        int time;
        String songName;

        Thread baddieAdderThread;
        Thread baddieRemoverThread;

        Font courierNew40;
        Font courierNew20;

        Graphics g;

        int score;

        public Display()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            score = 0;

            player = new PlayerShip(200, 500, 40, gravity);

            courierNew40 = new Font("Courier New", 60, FontStyle.Bold);
            courierNew20 = new Font("Courier New", 20);

            baddies = new ArrayList();
            floatingPixels = new ArrayList();

            currDisplay = 0;

            canMoveUp = true;
            canMoveDown = true;
            canMoveLeft = true;
            canMoveRight = true;
            canFire = true;
            canBounce = true;

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
            bounceWatch = new Stopwatch();
            bounceWatch.Start();
            musicTimer = new Stopwatch();

            flashCount = 0;
            flash = false;

            random = new Random();

            baddieAdderThread = new Thread(baddieAdder);
            baddieRemoverThread = new Thread(baddieRemover);

            graphicsThread = new Thread(graphicsTick);
            graphicsThread.Start();

            keyStrokeThread = new Thread(legalKeyPressCheck);
            keyStrokeThread.Start();

            gameOverThread = new Thread(checkGameOver);
            songOverThread = new Thread(checkSongOver);

            xExpl = -1;
            yExpl = -1;
            stage = 0;

            musicLength = -1;
        }

        private void Display_Load(object sender, EventArgs e)
        {

        }

        private void Display_Paint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;

            if (currDisplay == 0)
            {
                g.DrawString("Rocket Hopper", courierNew40, Brushes.White, 60f, 100f);

                if (flash && flashCount < 5)
                {
                    g.DrawString("Press Space to Fire Rockets", courierNew20, Brushes.Gold, (int)((Width / 2.0) - 100), (int)(Height / 2.0));
                    flashCount++;
                }
                else if (flashCount >= 5)
                {
                    // begin game
                    baddieAdderThread.Start();
                    baddieRemoverThread.Start();
                    gameOverThread.Start();
                    // play song
                    int song = random.Next(0, 5);
                    switch (song)
                    {
                        case 0:
                            killerMusic = new SoundPlayer(Properties.Resources.allTheseThingsIveDone);
                            songName = "All These Things I've Done";
                            musicLength = 302;
                            break;
                        case 1:
                            killerMusic = new SoundPlayer(Properties.Resources.smileLikeYouMeanIt);
                            musicLength = 233;
                            songName = "Smile Like You Mean It";
                            break;
                        case 2:
                            killerMusic = new SoundPlayer(Properties.Resources.mrBrightside);
                            musicLength = 219;
                            songName = "Mr. Brightside";
                            break;
                        case 3:
                            killerMusic = new SoundPlayer(Properties.Resources.spaceman);
                            musicLength = 291;
                            songName = "Spaceman";
                            break;
                        case 4:
                            killerMusic = new SoundPlayer(Properties.Resources.human);
                            musicLength = 250;
                            songName = "Human";
                            break;
                        case 5:
                            killerMusic = new SoundPlayer(Properties.Resources.somebodyToldMe);
                            musicLength = 192;
                            songName = "Sombeody Told Me";
                            break;
                    }
                    songOverThread.Start();
                    killerMusic.Play();
                    musicTimer.Start();
                    currDisplay++;
                }
                else
                {
                    g.DrawString("Press Space to Fire Rockets", courierNew20, Brushes.White, (int)((Width / 2.0) - 100), (int)(Height / 2.0));
                    g.DrawString("Use Arrowkeys to Move", courierNew20, Brushes.White, (int)((Width / 2.0) - 100), (int)(Height / 2.0) + 30);
                }
            }
            else if (currDisplay == 1)
            {
                g.DrawString("Score: "+ score, courierNew20, Brushes.Gold, 800, 5);
                g.DrawString(songName, courierNew20, Brushes.SteelBlue, 5, Height - 100);

                time = (int)musicTimer.ElapsedMilliseconds;
                time = time / 1000;
                time = musicLength - time;
                g.DrawString(""+time, courierNew20, Brushes.White, 650, 5);

                int rocketCount = 0;
                Rocket currRock;
                while (rocketCount < player.Rockets.Count)
                {
                    currRock = (Rocket)player.Rockets[rocketCount];
                    g.DrawLine(Pens.MediumVioletRed, currRock.XPos, currRock.YPos, currRock.XPos, currRock.YPos + 10);
                    rocketCount++;
                }

                int thrustCount = 0;
                ThrusterPixel pixel;
                while (thrustCount < player.ThrusterPixels.Count)
                {
                    pixel = (ThrusterPixel)player.ThrusterPixels[thrustCount];
                    g.DrawRectangle(Pens.OrangeRed, (int)pixel.XPos - 1, (int)pixel.YPos - 1, 2, 2);
                    thrustCount++;
                }

                int badCount = 0;
                BadGuy bad;
                while (badCount < baddies.Count)
                {
                    bad = (BadGuy)baddies[badCount];
                    if (bad.Destroyed == true && bad.HalfShipPixels > 0 && bad.WhiteFlash)
                    {
                        g.DrawRectangle(Pens.White, bad.XPos - bad.HalfShipPixels, bad.YPos - bad.HalfShipPixels, bad.HalfShipPixels * 2, bad.HalfShipPixels * 2);
                        bad.WhiteFlash = false;
                        bad.HalfShipPixels = bad.HalfShipPixels - 2;
                    }
                    else if (bad.Destroyed == true && bad.HalfShipPixels > 0 && !(bad.WhiteFlash))
                    {
                        g.DrawRectangle(Pens.Red, bad.XPos - bad.HalfShipPixels, bad.YPos - bad.HalfShipPixels, bad.HalfShipPixels * 2, bad.HalfShipPixels * 2);
                        bad.WhiteFlash = true;
                        bad.HalfShipPixels = bad.HalfShipPixels - 2;
                    }
                    else
                    {
                        switch (bad.Level)
                        {
                            case 1:
                                g.DrawRectangle(Pens.LightGreen, bad.XPos - bad.HalfShipPixels, bad.YPos - bad.HalfShipPixels, bad.HalfShipPixels * 2, bad.HalfShipPixels * 2);
                                break;
                            case 2:
                                g.DrawRectangle(Pens.LimeGreen, bad.XPos - bad.HalfShipPixels, bad.YPos - bad.HalfShipPixels, bad.HalfShipPixels * 2, bad.HalfShipPixels * 2);
                                break;
                            case 3:
                                g.DrawRectangle(Pens.Green, bad.XPos - bad.HalfShipPixels, bad.YPos - bad.HalfShipPixels, bad.HalfShipPixels * 2, bad.HalfShipPixels * 2);
                                break;
                            case 4:
                                g.DrawRectangle(Pens.ForestGreen, bad.XPos - bad.HalfShipPixels, bad.YPos - bad.HalfShipPixels, bad.HalfShipPixels * 2, bad.HalfShipPixels * 2);
                                break;
                            case 5:
                                g.DrawRectangle(Pens.DarkGreen, bad.XPos - bad.HalfShipPixels, bad.YPos - bad.HalfShipPixels, bad.HalfShipPixels * 2, bad.HalfShipPixels * 2);
                                break;
                        }
                    }

                    int evilRocketCount = 0;
                    Rocket evil;
                    while (evilRocketCount < bad.EvilRockets.Count)
                    {
                        evil = (Rocket)bad.EvilRockets[evilRocketCount];
                        g.DrawEllipse(Pens.Red, evil.XPos - 2, evil.YPos - 2, 4, 4);
                        evilRocketCount++;
                    }
                    badCount++;
                }

                int tastyCount = 0;
                TastyPixel tasty;
                while (tastyCount < floatingPixels.Count)
                {
                    tasty = (TastyPixel)floatingPixels[tastyCount];
                    g.DrawRectangle(Pens.Gold, tasty.XPos - 2, tasty.YPos - 2, 4, 4);
                    tastyCount++;
                }

                if (!gameOver)
                {
                    g.DrawString(""+player.HalfShippixels, courierNew20, System.Drawing.Brushes.OrangeRed, 15, 5);
                    if (player.Hit)
                    {
                        g.DrawEllipse(Pens.OrangeRed, (int)player.XPosShip - player.HalfShippixels, (int)player.YPosShip - player.HalfShippixels, player.HalfShippixels * 2, player.HalfShippixels * 2);
                        player.Hit = false;
                    }
                    else
                    {
                        g.DrawEllipse(Pens.SteelBlue, (int)player.XPosShip - player.HalfShippixels, (int)player.YPosShip - player.HalfShippixels, player.HalfShippixels * 2, player.HalfShippixels * 2);
                    }
                    if (player.HalfShippixels > 50)
                    {
                        g.DrawRectangle(Pens.Gold, 65, 0, 500, 40);
                    }
                    else
                    {
                        g.DrawRectangle(Pens.OrangeRed, 65, 0, player.HalfShippixels * 10, 40);
                    }
                }
                else
                {
                    // explosion animation
                    if (xExpl == -1 && yExpl == -1)
                    {
                        explosionTracker();
                    }
                    else if (stage > 500)
                    {
                        g.FillEllipse(System.Drawing.Brushes.OrangeRed, xExpl - stage, yExpl - stage, stage * 2, stage * 2);
                        currDisplay++;
                    }
                    else
                    {
                        if (stage % 2 == 0)
                        {
                            g.FillEllipse(System.Drawing.Brushes.OrangeRed, xExpl - stage, yExpl - stage, stage * 2, stage * 2);
                        }
                        else
                        {
                            g.FillEllipse(System.Drawing.Brushes.Red, xExpl - stage, yExpl - stage, stage * 2, stage * 2);
                        }
                        explosionTracker();
                    }
                }
            }
            else if (currDisplay == 2)
            {
                g.DrawString("GAME OVER", courierNew40, System.Drawing.Brushes.Red, (int)((Width / 2.0) - 100), (int)(Height / 2.0));
                g.DrawString("Final Score: " + score, courierNew20, System.Drawing.Brushes.Red, (int)((Width / 2.0) - 100) - 200, (int)(Height / 2.0) + 100);
            }
            else if (currDisplay == 3)
            {
                g.DrawString("YOU WIN!", courierNew40, System.Drawing.Brushes.Yellow, (int)((Width / 2.0) - 100), (int)(Height / 2.0));
                g.DrawString("Final Score: " + score, courierNew20, System.Drawing.Brushes.Yellow, (int)((Width / 2.0) - 100) - 200, (int)(Height / 2.0) + 100);
            }
        }

        private void checkCollisions()
        {
            if (player.HitboxTop < topSpace)
            {
                player.YVelShip = -player.YVelShip;
                player.YPosShip = topSpace + player.HalfShippixels + 5;
            }
            if (player.HitboxBottom > 660)
            {
                player.YVelShip = -player.YVelShip;
                player.YPosShip = 660 - player.HalfShippixels - 5;
            }
            if (player.HitboxRight > 1000)
            {
                player.XVelShip = -player.XVelShip;
                player.XPosShip = 1000 - player.HalfShippixels - 5;
            }
            if (player.HitboxLeft < 0)
            {
                player.XVelShip = -player.XVelShip;
                player.XPosShip = player.HalfShippixels + 5;
            }

            int badCount = 0;
            BadGuy bad;
            while (badCount < baddies.Count)
            {
                bad = (BadGuy)baddies[badCount];

                double xDistance = bad.XPos - player.XPosShip;
                double yDistance = bad.YPos - player.YPosShip;

                if (Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance)) < ((6/5) * bad.HalfShipPixels) + player.HalfShippixels)
                {
                    if (bounceWatch.ElapsedMilliseconds > 50)
                    {
                        canBounce = true;
                    }
                    if (canBounce && player.YPosShip > bad.YPos && player.XPosShip > bad.XPos)
                    {
                        if (player.XPosShip > bad.HitboxRight && player.YPosShip < bad.HitboxBottom)
                        {
                            // reverse x
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else if (player.XPosShip < bad.HitboxRight && player.YPosShip > bad.HitboxBottom)
                        {
                            // reverse y
                            canBounce = false;
                            bounceWatch.Restart();
                            player.YVelShip = -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else {
                            // reverse both
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.YVelShip = -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                    }
                    else if (canBounce && player.YPosShip < bad.YPos && player.XPosShip > bad.XPos)
                    {
                        if  (player.XPosShip > bad.HitboxRight && player.YPosShip > bad.HitboxTop)
                        {
                            // reverse x
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else if (player.XPosShip < bad.HitboxRight && player.YPosShip < bad.HitboxTop) 
                        {
                            // reverse y
                            canBounce = false;
                            bounceWatch.Restart();
                            player.YVelShip = -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else
                        {
                            // reverse both
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.YVelShip = -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                    }
                    else if (canBounce && player.YPosShip > bad.YPos && player.XPosShip < bad.XPos)
                    {
                        if (player.XPosShip < bad.HitboxLeft && player.YPosShip < bad.HitboxBottom)
                        {
                            // reverse x
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else if (player.XPosShip > bad.HitboxLeft && player.YPosShip > bad.HitboxBottom)
                        {
                            // reverse y
                            canBounce = false;
                            bounceWatch.Restart();
                            player.YVelShip = -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else
                        {
                            // reverse both
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.YVelShip = -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                    }
                    else if (canBounce && player.YPosShip < bad.YPos && player.XPosShip < bad.XPos)
                    {
                        if (player.XPosShip < bad.HitboxLeft && player.YPosShip > bad.HitboxTop)
                        {
                            // reverse x
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else if (player.XPosShip > bad.HitboxLeft && player.YPosShip < bad.HitboxTop)
                        {
                            // reverse y
                            canBounce = false;
                            bounceWatch.Restart();
                            player.YVelShip =  -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                        else
                        {
                            // reverse both
                            canBounce = false;
                            bounceWatch.Restart();
                            player.XVelShip = -player.XVelShip;
                            player.YVelShip = -player.YVelShip;
                            player.HalfShippixels--;
                            player.Hit = true;
                        }
                    }
                }

                int rocketCount = 0;
                Rocket r;
                while (rocketCount < player.Rockets.Count)
                {
                    r = (Rocket)player.Rockets[rocketCount];

                    bool xOverlapRock = false;
                    bool yOverlapRock = false;

                    if (r.XPos > bad.HitboxLeft && r.XPos < bad.HitboxRight)
                    {
                        xOverlapRock = true;
                    }
                    if (r.YPos > bad.HitboxTop && r.YPos < bad.HitboxBottom)
                    {
                        yOverlapRock = true;
                    }
                    if (xOverlapRock && yOverlapRock)
                    {
                        r.Hit = true;
                        bad.destroy();
                        score = score + (60 - bad.HalfShipPixels);
                    }

                    rocketCount++;
                }

                rocketCount = 0;
                while (rocketCount < bad.EvilRockets.Count)
                {
                    r = (Rocket)bad.EvilRockets[rocketCount];

                    xDistance = r.XPos - player.XPosShip;
                    yDistance = r.YPos - player.YPosShip;

                    if (Math.Sqrt((xDistance * xDistance) + (yDistance * yDistance)) < player.HalfShippixels)
                    {
                        player.HalfShippixels = player.HalfShippixels - 5;
                        player.Hit = true;
                        r.Hit = true;
                    }

                    rocketCount++;
                }

                badCount++;
            }

            double pixXdist;
            double pixYdist;

            int pixCount = 0;
            TastyPixel pixCheck;

            while (pixCount < floatingPixels.Count)
            {
                pixCheck = (TastyPixel)floatingPixels[pixCount];
                pixXdist = pixCheck.XPos - player.XPosShip;
                pixYdist = pixCheck.YPos - player.YPosShip;

                if (Math.Sqrt((pixXdist * pixXdist) + (pixYdist * pixYdist)) < player.HalfShippixels + 5)
                {
                    floatingPixels.RemoveAt(pixCount);
                    player.HalfShippixels = player.HalfShippixels + 2;
                }
                else
                {
                    if (pixXdist > 0)
                    {
                        if (pixXdist < 200)
                        {
                            pixCheck.XPos = pixCheck.XPos - 5;
                        }
                        else
                        {
                            pixCheck.XPos--;
                        }
                    }
                    else if (pixXdist < 0)
                    {
                        if (pixXdist > -200)
                        {
                            pixCheck.XPos = pixCheck.XPos + 5;
                        }
                        else
                        {
                            pixCheck.XPos++;
                        }
                    }
                    if (pixYdist > 0)
                    {
                        if (pixYdist < 200)
                        {
                            pixCheck.YPos = pixCheck.YPos - 5;
                        }
                        else
                        {
                            pixCheck.YPos--;
                        }
                    }
                    else if (pixYdist < 0)
                    {
                        if (pixYdist > -200)
                        {
                            pixCheck.YPos = pixCheck.YPos + 5;
                        }
                        else
                        {
                            pixCheck.YPos++;
                        }
                    }
                    pixCount++;
                }
            }
        }

        private void graphicsTick()
        {
            while(true)
            {
                if (currDisplay == 1)
                {
                    checkCollisions();
                }

                this.Invalidate();

                Thread.Sleep(30);
            }
        }

        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            if (currDisplay == 0)
            {
                if (e.KeyCode == Keys.Space)
                {
                    flash = true;
                }
            }

            if (currDisplay == 1)
            {
                if (!gameOver)
                {
                    // UP
                    if (e.KeyCode == Keys.Up)
                    {
                        if (canMoveUp)
                        {
                            player.boost(0);
                            canMoveUp = false;
                            upWatch.Restart();
                        }
                    }

                    if (e.KeyCode == Keys.Down)
                    {
                        if (canMoveDown)
                        {
                            player.boost(2);
                            canMoveDown = false;
                            downWatch.Restart();
                        }
                    }

                    // LEFT
                    if (e.KeyCode == Keys.Left)
                    {
                        if (canMoveLeft)
                        {
                            player.boost(3);
                            canMoveLeft = false;
                            leftWatch.Restart();
                        }
                    }

                    // RIGHT
                    if (e.KeyCode == Keys.Right)
                    {
                        if (canMoveRight)
                        {
                            player.boost(1);
                            canMoveRight = false;
                            rightWatch.Restart();
                        }
                    }
                    // FIRE
                    if (e.KeyCode == Keys.Space)
                    {
                        if (canFire)
                        {
                            player.fireRocket();
                            canFire = false;
                            fireWatch.Restart();
                        }
                    }

                    if (e.KeyCode == Keys.R)
                    {
                        player.HalfShippixels = 40;
                    }
                }

                // ESCAPE
                if (e.KeyCode == Keys.Escape)
                {
                    Application.Exit();
                }
            }
            if (currDisplay == 2 || currDisplay == 3)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    Application.Exit();
                }
            }
        }

        private void legalKeyPressCheck()
        {
            while (true)
            {
                if (upWatch.ElapsedMilliseconds > 200)
                {
                    canMoveUp = true;
                }
                if (downWatch.ElapsedMilliseconds > 200)
                {
                    canMoveDown = true;
                }
                if (leftWatch.ElapsedMilliseconds > 200)
                {
                    canMoveLeft = true;
                }
                if (rightWatch.ElapsedMilliseconds > 200)
                {
                    canMoveRight = true;
                }
                if (fireWatch.ElapsedMilliseconds > 200)
                {
                    canFire = true;
                }
                Thread.Sleep(100);
            }
        }

        private void baddieAdder()
        {
            while (true)
            {
                int randomSize = random.Next(25, 50);
                int randomLevel = random.Next(1,5);
                switch (randomLevel)
                {
                    case 1: randomLevel = 1;
                        break;
                    case 2: randomLevel = random.Next(1, 2);
                        break;
                    case 3: randomLevel = random.Next(1, 3);
                        break;
                    case 4: randomLevel = random.Next(1, 4);
                        break;
                    case 5: randomLevel = random.Next(1, 5);
                        break;
                }

                int randomX = random.Next(randomSize, 950 - randomSize);
                if (!playerWin)
                {
                    baddies.Add(new BadGuy(randomX, topSpace - randomSize, randomSize, randomLevel));
                }

                double currentTime = (int)musicTimer.ElapsedMilliseconds / 1000;
                double timeRatio = currentTime / musicLength;
                
                if(timeRatio < 0.1)
                {
                    Thread.Sleep(2000);
                }
                else if (timeRatio < 0.3)
                {
                    Thread.Sleep(1500);
                }
                else if (timeRatio < 0.5)
                {
                    Thread.Sleep(1200);
                }
                else if (timeRatio < 0.75)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Thread.Sleep(900);
                }
            }
        }

        private void baddieRemover()
        {
            while (true)
            {
                int count = 0;
                bool done = false;
                BadGuy bad;
                while (!done && baddies.Count > 0)
                {
                    bad = (BadGuy)baddies[count];
                    if (bad.HalfShipPixels <= 0)
                    {
                        floatingPixels.Add(new TastyPixel(bad.XPos, bad.YPos));
                        baddies.RemoveAt(count);
                    }
                    else if (bad.YPos - bad.HalfShipPixels > 700)
                    {
                        bad.YPos = -2 * bad.HalfShipPixels;
                    }
                    else
                    {
                        count++;
                    }
                    if (count >= baddies.Count)
                    {
                        done = true;
                    }
                }

                Thread.Sleep(100);
            }
        }

        private void checkGameOver()
        {
            while (true)
            {
                if (player.HalfShippixels <= 0)
                {
                    gameOver = true;
                }
                Thread.Sleep(30);
            }
        }

        int xExpl;
        int yExpl;
        int stage;

        private void explosionTracker()
        {
            if (stage == 0)
            {
                xExpl = (int) player.XPosShip;
                yExpl = (int) player.YPosShip;
            }

            stage = stage + 5;
        }

        private void checkSongOver()
        {
            Thread.Sleep(musicLength * 1000);
            if (!gameOver)
            {
                playerWin = true;
                lock (baddies)
                {
                    foreach(BadGuy badguy in baddies)
                    {
                        badguy.destroy();
                    }
                }
                Thread.Sleep(500);
                gameOver = true;
                currDisplay = currDisplay + 2;
            }
        }
    }
}
