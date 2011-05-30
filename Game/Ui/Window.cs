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

namespace Game.Ui {

public class Window :Widget
{
           const  int  M_ENA = 0x10;
           const  int  M_DSA = 0x20;

           int    mode;

           int    alpha;

   protected Color bgcolor;
   protected Surface bgimage;
   protected List<Widget> widgets;

   public  Window(Widget parent, int x, int y, int width, int height, bool transparent) 
   :base(parent, x, y, width, height, transparent)
   {
           bgcolor = Color.FromArgb(0x7f, 0, 0, 0);
           bgimage = null;

           widgets = new List<Widget>();

           mode = 0;
           alpha  = 0;
           enabled = false;
   }

   public  Window(Widget parent, int x, int y, int width, int height) 
   :this(parent, x, y, width, height, true)
   {
   }

   ~Window()
   {
           Dispose();
   }

   public  override void AddWidget(Widget w)
   {
           lock(widgets)
           {
                widgets.Add(w);
           }
   }

   public  override void RemWidget(Widget w)
   {
           lock(widgets)
           {
                widgets.Remove(w);
                resync |= true;
           }
   }

   public  override void ResetEnabled(Widget authority, bool value)
   {
           lock(widgets)
           {
                foreach(Widget w in widgets)
                {
                       w.ResetEnabled(this, value);
                }
           }

           mode &= ~(M_ENA | M_DSA);

       if (value)
           mode |= M_ENA;
           else
           mode |= M_DSA;

           enabled = value;
   }

   protected bool resync = true;

   public  override bool Sync(float dt)
   {
       if ((mode & M_ENA) != 0)
       {
           if (dt > 0.1f)
               dt = 0.1f;

           if (alpha < 255)
               alpha += (byte)(dt * 512.0f);
               else
               mode ^= M_ENA;

           if (alpha > 255)
               alpha = 255;

               Surface.Alpha = (byte)alpha;
               resync |= true;
       }

       if ((mode & M_DSA) != 0)
       {
           if (alpha > 0)
               alpha -= (byte)(dt * 1024.0f);
               else
               mode ^= M_DSA;

           if (alpha < 0)
               alpha = 0;

               Surface.Alpha = (byte)alpha;
               resync |= true;
       }

       lock(widgets) 
       {
              foreach(Widget w in widgets)
              {
                      resync |= w.Sync(dt);
              }
       }

           return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (alpha == 0)
           return;

       if (resync)
       {
           if (bgimage == null)
           {
               Surface.Fill(bgcolor);
           }   else
           {
               Surface.Fill(Color.FromArgb(0, 0, 0, 0));
               Surface.Blit(bgimage);
           }

           lock(widgets)
           {
               foreach(Widget w in widgets)
               {
                   w.Draw(Surface);
               }
           }

           Surface.Update();
           resync = false;
       }

           dst.Blit(this);
   }

           bool disposed;

   public  override void Dispose()
   {
       if (disposed)
           return;

       lock(this) {
            try
            {
                lock(widgets)
                {
                     foreach(Widget w in widgets)
                     {
                             w.Dispose();
                     }
                }

                base.Dispose();
            }
            catch(Exception e)
            {
            }

            disposed = true;
       }
   }
 }

/*namespace Game.Ui*/ }
