/*
PlaneStupid
Copyright (c) 2011 spaceape

This software is provided 'as-is', without any express or implied
warranty. In no event will the authors be held liable for any damages
arising from the use of this software.

Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not
    claim that you wrote the original software. If you use this software
    in a product, an acknowledgment in the product documentation would be
    appreciated but is not required.

    2. Altered source versions must be plainly marked as such, and must not be
    misrepresented as being the original software.

    3. This notice may not be removed or altered from any source
    distribution.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using SdlDotNet.Core;
using SdlDotNet.Input;
using SdlDotNet.Audio;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;
using SdlDotNet.Graphics.Primitives;
using Core.Cortex;
using Core.Scene;
using Core.Net;
using Game.Mods;

namespace Game.Ui.Controls {

public interface iLandingSpot
{
       bool PtIsInside(Point pt);
       void Show(ref bool show);
       void Hide(ref bool show);
}

public class Trap :Widget, Thing.Ai
{
   const float HW = (float)(Desktop.width / 2);
   const float HH = (float)(Desktop.height / 2);

           Vehicle vehicle;
           iLandingSpot landingspot;

           Surface sfOff;
           Surface sfRing;

   const int MODE_NO = 0;
   const int MODE_VISIBLE = 1;
   const int MODE_OFF = 2;
   const int MODE_OVER = 4;
   const int MODE_DOWN = 8;

           int mode = MODE_NO;

           bool hasRouteInfo;
           bool hasStatusInfo;

   public  Trap(Widget parent, Vehicle vehicle, iLandingSpot ls)
   :base(parent, 0, 0, 64, 64, true)
   {
           this.vehicle = vehicle;
           this.vehicle.ai = this;

           this.landingspot = ls;

           this.sfOff = new Surface("res/trap-off.png");
           this.sfRing = new Surface("res/trap-ring.png");

           AlphaBlending = true;
           Alpha = 192;
   }

   ~Trap()
   {
           this.vehicle.ai = null;
   }

   public  void Step(Thing body, float dt)
   {
           int  x = (int)(HW + body.Pos.x * HW) - (Width / 2);
           int  y = (int)(HH + body.Pos.y * HW);

       if (x != this.X || y != this.Y)
       {
           this.X = x;

           if (y > Desktop.Height)
           {   mode |= MODE_OFF;
               this.Y = Desktop.Height - Height;
           }   else
           {
               this.Y = y - Height / 2;
               mode &= ~MODE_OFF;
           }
       }

           mode |= MODE_VISIBLE;
   }

           PointF ptprev;
           PointF ptcurrent;
           float  trtimer;
           int    bltimer;
           bool   show;
           bool   hrilock;

   public  override void Update(MouseButtonEventArgs e)
   {
       if ((mode & MODE_OFF) != 0)
           return;

       if (e.Button != MouseButton.PrimaryButton)
           return;

           int lmode = mode;

       if (e.ButtonPressed)
       {
           if (Window.Contains(e.Position))
           {
               Point Pos = new Point(X + Width / 2, Y + Height / 2);
               SdlDotNet.Input.Mouse.MousePosition = Pos;

               vehicle.Route = new Vehicle.Path();
               ptprev = DesktopToSpace(Pos);
               ptcurrent = DesktopToSpace(Pos);
               trtimer = 0.0f;
               hrilock = true;
               mode |= MODE_DOWN;
           }
       }   else
       {
           if ((mode & MODE_DOWN) != 0)
           {
               if (landingspot.PtIsInside(e.Position))
               {   vehicle.Flags[Proto.Vehicle.SI_MODS] |= Proto.Vehicle.MOD_OWNED;
                   hasStatusInfo = true;
               }   else
               {
                   vehicle.Flags[Proto.Vehicle.SI_MODS] = (byte)(vehicle.Flags[Proto.Vehicle.SI_MODS] & ~Proto.Vehicle.MOD_OWNED);
                   hasStatusInfo = true;
               }

               hasRouteInfo = vehicle.Route.Count > 0;
               landingspot.Hide(ref show);
          }

               mode &= ~MODE_DOWN;
       }
   }

   public  override void Update(MouseMotionEventArgs e)
   {
       if ((mode & MODE_OFF) != 0)
           return;

       if (Window.Contains(e.Position))
       {
           if ((mode & MODE_OVER) == 0)
           { 
                mode |= MODE_OVER;

              //if (sndslide != null)
              //    sndslide.Play();
           }
       }   else
       {
           if ((mode & MODE_OVER) != 0)
           {
                mode ^= MODE_OVER;
           }
       }

       if ((mode & MODE_DOWN) != 0)
       {
           ptcurrent = DesktopToSpace(e.Position);

           if (landingspot.PtIsInside(e.Position))
               landingspot.Show(ref show);
               else
               landingspot.Hide(ref show);
       }
   }

   public  override bool Sync(float dt)
   {
          bltimer += 1;

      if (bltimer > 255)
          bltimer = 0;

      if ((mode & MODE_DOWN) != 0)
      {
           if (trtimer >= 0.05f)
           {
               if (vehicle.Route == null)
                   vehicle.Route = new Vehicle.Path();

               PointF pts = ptprev;

               float  dx = ptcurrent.X - pts.X;
               float  dy = ptcurrent.Y - pts.Y;

               double r = Math.Sqrt(Math.Pow(dx, 2.0) + Math.Pow(dy, 2.0));
               double dr = Proto.dtRsTime * vehicle.Velocity.z;
               double x = 0.0;

               float  rx = (float)(dx * dr / r);
               float  ry = (float)(dy * dr / r);

               while (x < r)
               {
                  if (vehicle.Route.Count > 512)
                      break;
                    
                      vehicle.Route.Enqueue(pts);
                      pts.X += rx;
                      pts.Y += ry;
                      x += dr;

                  if (hrilock)
                  {
                      hasRouteInfo = true;
                      hrilock = false;
                  }
               }

               ptprev = pts;
               ptcurrent = pts;
               trtimer -= 0.05f;
           }

               trtimer += dt;
      }    else
      {
           
      }

           return true;
   }

   public  override void Draw(Surface dst)
   {
           Surface.Fill(Color.FromArgb(0, 0, 0, 0));

           if ((mode & MODE_VISIBLE) != 0)
           {
               if ((mode & MODE_OFF) != 0)
               {
                    dst.Blit(sfOff, Rectangle);
               }    else
               {
                    if ((mode & MODE_OVER) != 0)
                    {
                        dst.Blit(sfRing, Rectangle);
                    }
                    
                    int mods = vehicle.Flags[Proto.Vehicle.SI_MODS];

                    if (mods > 0)
                    {
                        if ((mods & Proto.Vehicle.MOD_LANDING) == 0)
                        {
                            Circle c = new Circle(32, 32, 24);
                            Surface.Draw(c, Color.LightBlue, false, false);

                            if ((mods & Proto.Vehicle.MOD_OWNED) != 0)
                            {
                                Box box = new Box(32, 16, 35, 19);
                                Surface.Draw(box, Color.Red, false, true);
                            }

                            if ((mods & Proto.Vehicle.MOD_PROXI) != 0)
                            {
                                Circle w = new Circle(32, 32, (short)(24 + (bltimer & 7)));
                                Surface.Draw(w, Color.Red, false, false);
                            }
                        }   else
                        {
                                Circle c = new Circle(32, 32, 27);
                                Surface.Draw(c, Color.BlueViolet, false, false);
                        }
                   }
               }
           }

           Surface.Update();
           dst.Blit(this);
   }

   public  bool HasRouteInfo()
   {
           return hasRouteInfo;
   }

   public  PointF[] GetRouteInfo()
   {
           hasRouteInfo = false;
           return vehicle.Route.ToArray();
   }

   public  bool HasStatusInfo()
   {
           return hasStatusInfo;
   }

   public  byte[] GetStatusInfo()
   {
           hasStatusInfo = false;
           return vehicle.Flags;
   }

   public  int Id
   {
           get {return vehicle.Id;}
   }

   public  Vehicle Vehicle
   {
           get {return vehicle;}
   }

   public  static PointF DesktopToSpace(Point p)
   {
           PointF ret = new PointF(-1.0f + (float)p.X / HW, 
                                   -1.0f + (float)(HW - HH + p.Y ) / HW
           );
           return ret;
   }

   public  static Point SpaceToDesktop(PointF p)
   {
           Point  ret = new Point((int)(HW + p.X * HW),
                                  (int)(HH + p.Y * HW)
                  );
           return ret;
   }
}

public class LandingSpot :Widget, iLandingSpot
{
           Rectangle hotspot;

   protected int showcnt;

   public LandingSpot(Widget parent, int x, int y, int width, int height)
   :base(parent, x, y, width, height, true)
   {
           hotspot = new Rectangle(x + 1, y + 1, 48, 48);
   }

   public  bool PtIsInside(Point pt)
   {
           return hotspot.Contains(pt);
   }

   public  void Show(ref bool show)
   {
       if (show)
           return;

       if (!show)
       {
           show = true;
           showcnt++;
       }
   }

   public  void Hide(ref bool show)
   {
       if (!show)
           return;

       if (show)
       {
           show = false;
           showcnt--;
       }
   }
}

public class Strip :LandingSpot
{
           Surface pointer;

   const int MODE_HIGHLIGHT = 1;

           int mode;

   public Strip(Widget parent, int x, int y)
   :base(parent, x, y, 128, 48)
   {
           pointer = new Surface("res/strip-pointer.png");
   }

           int  dx;

   public  override bool Sync(float dt)
   {
       int lmode = mode;

       if (showcnt > 0)
           mode |= MODE_HIGHLIGHT;
           else
           mode &= ~MODE_HIGHLIGHT;

       if ((mode & MODE_HIGHLIGHT) != 0)
       {
           dx += 4;
           
           if (dx > 48)
               dx = 0; 
       }   else
       {
           dx = 0;
       }

           return true;
   }

   public  override void Draw(Surface dst)
   {
           if ((mode & MODE_HIGHLIGHT) != 0)
           {
                int x = dx;
  
                while (x >= 0)
                {
                       dst.Blit(pointer, new Point(X + x, Y + 0));
                       x -= pointer.Width / 2;
                }
            }

            dst.Blit(this);
    }
}

public class Helipad :Widget
{
   public Helipad(Widget parent, int x, int y)
   :base(parent, x, y, 64, 64, true)
   {
   }
}

/* namespace Game.Ui.Controls */ }
