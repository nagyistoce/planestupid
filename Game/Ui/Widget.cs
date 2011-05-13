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
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Sprites;

namespace Game.Ui {

public interface IFocusable
{
   void GainedFocus();
   void LostFocus();
}

public class Widget :Sprite
{
             Widget  parent;

   public Widget(Widget parent) :base()
   {
           this.parent = parent;

       if (this.parent != null)
           this.parent.AddWidget(this);

           Desktop.WidgetWantEvents(this);
   }

   ~Widget()
   {
          Dispose();
   }

   public Widget(Widget parent, int x, int y, Surface sf) :this(parent)
   {
          Reset(x, y, sf);
   }


   public Widget(Widget parent, int x, int y, int width, int height, bool alpha) :this(parent)
   {
           Reset(x, y, new Surface(width, height, 32, true));

           Surface.Transparent = true;
       if (Surface.AlphaBlending = alpha)
           Surface.Alpha = 255;
   }

   public Widget(Widget parent, int x, int y, string resource, bool alpha) :this(parent)
   {
           Reset(x, y, new Surface(resource));
           Surface.Transparent = true;
       if (Surface.AlphaBlending = alpha)
           Surface.Alpha = 255;
   }

   public  void    Reset(int x, int y, Surface sf)
   {
           base.X = x;
           base.Y = y;

           Surface = sf.Convert(Desktop.Surface, true, true);
   } 

   public  virtual void AddWidget(Widget w)
   {
   }

   public  virtual void RemWidget(Widget w)
   {
   }

   protected bool enabled = true;

   public  virtual bool GetEnabled()
   {
           bool pe;

       if (parent != null)
           pe = parent.GetEnabled();
           else
           pe = true;

           return pe && enabled;
   }

   public  virtual void ResetEnabled(Widget authority, bool value)
   {
           enabled = value;
   }

   public  virtual bool Sync(float dt)
   {
           return  false;
   }

   public  virtual void Draw(Surface dst)
   {
           dst.Blit(Surface);
   }

           bool disposed;

   public  virtual void Dispose()
   {
       if (disposed)
           return;

           Desktop.WidgetNoEvents(this);

       if (parent != null)
           parent.RemWidget(this);

           base.Dispose();
           disposed = true;
   }

   public  Rectangle Window
   {
           get {
                     Rectangle Win = Rectangle;

                if (parent != null)
                    Win.Offset(parent.Window.Location);

                    return Win;              
           }
   }

   public  bool Active
   {
           get {return GetEnabled() && Visible;}
   }
}

/*namespace Game.Ui*/ }
