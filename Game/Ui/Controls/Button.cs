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
using System.Drawing;
using SdlDotNet.Core;
using SdlDotNet.Input;
using SdlDotNet.Audio;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;

namespace Game.Ui.Controls {

public enum  MenuButtonFunction
{
       Single,
       JoinGame,
       CreateGame,
       Settings,
       Quit
}

public enum  BarButtonFunction
{
       Cancel,
       Logout,
       Play
}

public class Button :Widget
{
   public  string Caption {get; set;}

           const  int  M_MOUSE_SLIDE = 1;
           const  int  M_MOUSE_CLICK = 2;
           const  int  M_FROZEN = 8;
           const  int _M_LOOK_ = M_MOUSE_SLIDE | M_MOUSE_CLICK;

           const  int  M_ENA = 0x10;
           const  int  M_DSA = 0x20;

           int    mode;

           int    alpha;

   protected Surface[] images;

           Sound  sndslide;
           Sound  sndclick;

   public  Button(Widget parent, int x, int y, int width, int height) 
   :base(parent, x, y, width, height, true)
   {
           mode  |= M_ENA;
           images = new Surface[4];
           alpha  = 127;
   }

   public  MenuButtonFunction MenuFunction {get; set;}

   public  Button(Widget parent, int x, int y, MenuButtonFunction f)
   :this(parent, x, y, 48, 48)
   {
           string[] files;

           switch(f)
           {
             case MenuButtonFunction.Single:
                  files = new string[]{
                          "res/bsing.png", 
                          "res/bsing-glow.png", 
                          "res/bsing-down.png", 
                          "res/bsing-down.png"
                  };
                  Caption = "Single";
                  break;

             case MenuButtonFunction.JoinGame:
                  files = new string[]{
                          "res/bjoin.png", 
                          "res/bjoin-glow.png", 
                          "res/bjoin-down.png", 
                          "res/bjoin-down.png"
                  };
                  Caption = "Join Game";
                  break;

             case MenuButtonFunction.CreateGame:
                  files = new string[]{
                          "res/bcrea.png", 
                          "res/bcrea-glow.png", 
                          "res/bcrea-down.png", 
                          "res/bcrea-down.png"
                  };
                  Caption = "Create Game";
                  break;

             case MenuButtonFunction.Settings:
                  files = new string[]{
                          "res/bsett.png", 
                          "res/bsett-glow.png", 
                          "res/bsett-down.png", 
                          "res/bsett-down.png"
                  };
                  Caption = "Settings";
                  break;

             case MenuButtonFunction.Quit:
                  files = new string[]{
                          "res/bquit.png", 
                          "res/bquit-glow.png", 
                          "res/bquit-down.png", 
                          "res/bquit-down.png"
                  };
                  Caption = "Quit";
                  break;

             default:
                  throw new Exception("WTF?!");
           }

           for (x = 0; x != 4; x++)
           {
                images[x] = new Surface(files[x]);
           }

                sndslide = Desktop.Sampler.Load("slide", "res/slide.wav");
                sndclick = Desktop.Sampler.Load("click", "res/click.wav");

                MenuFunction = f;
   }

   public  BarButtonFunction BarFunction {get; set;}

   public  Button(Widget parent, int x, int y, BarButtonFunction f)
   :this(parent, x, y, 96, 32)
   {
           string[] files;

           switch(f)
           {
             case BarButtonFunction.Cancel:
                  files = new string[]{
                          "res/bbcancel.png", 
                          "res/bbcancel-glow.png", 
                  };
                  Caption = "Single";
                  break;

             case BarButtonFunction.Logout:
                  files = new string[]{
                          "res/bblogout.png", 
                          "res/bblogout-glow.png", 
                  };
                  Caption = "Network";
                  break;

             case BarButtonFunction.Play:
                  files = new string[]{
                          "res/bbplay.png", 
                          "res/bbplay-glow.png", 
                  };
                  Caption = "Settings";
                  break;

             default:
                  throw new Exception("WTF?!");
           }

                images[0] = new Surface(files[0]);
                images[1] = new Surface(files[1]);
                images[2] = images[1];
                images[3] = images[1];

                sndslide = Desktop.Sampler.Load("slide", "res/slide.wav");
                sndclick = Desktop.Sampler.Load("click", "res/click.wav");

                BarFunction = f;
   }


   protected bool resync = true;

   public  override void Update(MouseMotionEventArgs e)
   {
       if ((mode & M_FROZEN) != 0)
           return;

       if (!Active)
           return;

       if (Window.Contains(e.Position))
       {
           if ((mode & M_MOUSE_SLIDE) == 0)
           { 
                mode |= M_MOUSE_SLIDE;

                if (slide != null)
                    slide(this);

                if (sndslide != null)
                    sndslide.Play();

                resync = true;
           }
       }   else
       {
           if ((mode & M_MOUSE_SLIDE) != 0)
           {
                mode ^= M_MOUSE_SLIDE;
                resync = true;
           }
       }
   }

   public  override void Update(MouseButtonEventArgs e)
   {
       if ((mode & M_FROZEN) != 0)
           return;

       if (!Active)
           return;

       if (e.Button != MouseButton.PrimaryButton)
           return;

       if (e.ButtonPressed)
       {
           if (Window.Contains(e.Position))
           {
                mode |= M_MOUSE_CLICK;

                if (click != null)
                    click(this);

                if (sndclick != null)
                    sndclick.Play();

                    resync = true;
           }
       }   else
       {
           if ((mode & M_MOUSE_CLICK) != 0)
           {
                mode &= ~M_MOUSE_CLICK;
               resync = true;
           }
       }
   }

   public  override bool Sync(float dt)
   {
       if ((mode & M_ENA) != 0)
       {
           if (alpha < 255)
               alpha += (byte)(dt * 255.0f);
               else
               mode ^= M_ENA;

           if (alpha > 255)
               alpha = 255;

               Surface.Alpha = (byte)alpha;
               resync = true;
       }

       if ((mode & M_DSA) != 0)
       {
           if (alpha > 127)
               alpha -= (byte)(dt * 255.0f);
               else
               mode ^= M_DSA;

           if (alpha < 127)
               alpha = 127;

               Surface.Alpha = (byte)alpha;
               resync = true;
       }

           return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {
           Surface.Fill(Color.FromArgb(0, 0, 0, 0));

           if ((mode & M_FROZEN) == 0)
                Surface.Blit(images[mode & _M_LOOK_]);
                else
                Surface.Blit(images[2]);

           Surface.Update();
           resync = false;
       }
 
           dst.Blit(this);
   }

   public  void Enable()
   {
           mode = 0;
           mode |= M_ENA;
           enabled = true;
   }

   public  void Disable()
   {
           mode &= ~M_ENA;
           mode |= M_DSA;
           enabled = false;
   }

   public  void setFrozen(bool value)
   {
           mode &= ~M_FROZEN;

       if (value)
           mode |= M_FROZEN;

           resync |= true;
   }

   public  delegate void FOnSlide(Button sender);

           FOnSlide slide;

   public  FOnSlide OnSlide
   {
           get  {return slide;}
           set  {slide = value;}
   }

   public  delegate void FOnClick(Button sender);

           FOnClick click;

   public  FOnClick OnClick
   {
           get  {return click;}
           set  {click = value;}
   }
}

/*namespace Game.Ui.Controls*/ }
