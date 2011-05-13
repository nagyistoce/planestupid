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

namespace Game.Ui.Controls {

public class Label :Widget
{
           string text = "";
           int    maxlen = 256;

           int    fontsize;
           string fontname;
           SdlDotNet.Graphics.Font font;

           int    dy;
           Color  color;

           float  anit = 0.0f;

   const   int MODE_ANIMATED = 1;
           int mode;

   public Label(Widget parent, int x, int y, int width, int height)
   :base(parent, x, y, width, height, true)
   {
           setFont("res/arcade.ttf", 0);
           color = Color.FromArgb(0, 1, 1, 1);
           mode = 0;
   }

   private bool resync = true;

   public void Resize(int width, int height)
   {
          /*
          */
   }

   public  override bool Sync(float dt)
   {
       if ((mode & MODE_ANIMATED) != 0)
       {
           if (anit <= 1.0f)
           {
               Surface.Alpha = (byte)(255.0f - (192.0f * anit));
               resync |= true;
           }   else
           if ((anit > 1.0f) && (anit <= 2.0f))
           {
               Surface.Alpha = (byte)(-128.0f + (192.0f * anit));
               resync |= true;
           }
           if (anit > 4.0f)
           {
               anit = 0.0f;
           }

           anit += dt * 4;

       }   else
       {
           anit = 0.0f;
           Surface.Alpha = (byte)255;
           resync |= true;
       }

           return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {   Surface.Fill(Color.FromArgb(0, 0, 0, 0));
           Surface.Blit(font.Render(text, color));
           Surface.Update();
           resync = false;
       }
 
           dst.Blit(this);
   }

   public  void setText(string Text)
   {
           text = Text;
           resync = true;
   }

   public  void setColor(Color Color)
   {
           color = Color;
           resync = true;
   }

   public  void setFont(string FontName, int FontSize)
   {
           fontname = FontName;
           fontsize = FontSize;

       if (fontsize == 0)
       {
           fontsize = Height - 4;
       }

           font = new SdlDotNet.Graphics.Font(fontname, fontsize);

           dy = 0;

           resync = true;
   }

   public  void setAnimated(bool value)
   {
       if (value)
           mode |= MODE_ANIMATED;
           else
           mode &= ~MODE_ANIMATED;
   }
}

/*namespace Game.Ui.Controls*/}
