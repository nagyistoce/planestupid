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
using SdlDotNet.Graphics.Primitives;

namespace Game.Ui.Controls {

public class EditBox :Widget, IFocusable
{
           Box box;

           string text = "";
           int    maxlen = 256;

           int    fontsize;
           string fontname;
           SdlDotNet.Graphics.Font font;

           int    sublen;
           string subtext;
           float  cursort;
           string cursor = "";


   const   int MODE_FOCUSED = 1;
   const   int MODE_FROZEN = 8;
   const   int MODE_BORDERED = 0x10;
           int mode;

   public EditBox(Widget parent, int x, int y, int width, int height)
   :base(parent, x, y, width, height, true)
   {
           box = new Box((short)0, (short)0, 
                (short)(width - 1), (short)(height - 1));

           setFont("res/andalemo.ttf", Height - 4);
   }

   ~EditBox()
   {
           Defocus();
   }

   private void update_subtext()
   {
      if  (text.Length <= sublen)
           subtext = text;
           else
           subtext = text.Substring(text.Length - sublen, sublen);
   }

   private bool resync = true;

   public void Resize(int width, int height)
   {
          /*
           box = new Box((short)x, (short)y, 
                (short)(x + width), (short)(x + height));
           base.Width = width;
           base.Height = height;
         */
   }

   public  override void Update(KeyboardEventArgs e)
   {
       if (!Active)
           return;

       if ((mode & MODE_FROZEN) != 0)
           return;

       if (e.Type != EventTypes.KeyDown)
           return;

       if ((mode & MODE_FOCUSED) != 0)
       {
           switch(e.Key)
           {
             case Key.Delete:
                  setText("");
                  break;

             case Key.Backspace:
               if(text.Length > 0)
                  setText(text.Substring(0, text.Length - 1));
                  break;

             case Key.Space:
                  setText(text + " ");
                  break;

             case Key.Return:
               if (OnCompleted != null)
                  OnCompleted(this);
                  break; 

             default:
               if (e.KeyboardCharacter.Length == 1)
                   setText(text + e.KeyboardCharacter);
                  break;
           }

           //Console.WriteLine("Key {0}", e.Scancode);
       }
   }

   public  override void Update(MouseMotionEventArgs e)
   {
       if (!Active)
           return;
   }

   public  override void Update(MouseButtonEventArgs e)
   {
           if (!Active)
               return;

           if (Window.Contains(e.Position))
           {
               if (e.Button == MouseButton.PrimaryButton)
                    Focus();
           }   else
           {
               Defocus();
           }
   }

   public  void GainedFocus()
   {
           mode |= MODE_FOCUSED;
           resync |= true;
   }

   public  void LostFocus()
   {
           mode &= ~MODE_FOCUSED;
           resync |= true;
   }

   public  override bool Sync(float dt)
   {
       if ((mode & MODE_FOCUSED) != 0)
       {
           if (cursort >= 1.0f)
           {
               if (cursor.Length == 0)
                   cursor = "|";
                   else
                   cursor = "";

                   cursort = 0.0f;
                   resync |= true;
           }

           cursort += dt;
      }    else
      {
           cursor = "";
           cursort = 0.0f;
      }

           return resync;
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {
               Surface.Fill(Color.FromArgb(0, 0, 0, 0));

           if ((mode & MODE_FOCUSED) != 0)
           {
               if ((mode & MODE_FROZEN) == 0)
               {
                    Surface.Fill(Color.FromArgb(127, 96, 96, 96));
                    Surface.Draw(box, Color.FromArgb(255, 0, 0, 255), true);
               }
           }

           if ((mode & MODE_BORDERED) != 0)
           {
               Surface.Draw(box, Color.FromArgb(255, 0, 0, 255), true);
           }

           Surface.Blit(font.Render(subtext + cursor, Color.FromArgb(255, 255, 255, 255)));
           Surface.Update();
           resync = false;
       }
 
           dst.Blit(this);
   }

   public  void setText(string Text)
   {
           text = Text;
           update_subtext();

           resync = true;
   }

   public  void setFont(string FontName, int FontSize)
   {
           fontname = FontName;
           fontsize = FontSize;

           font = new SdlDotNet.Graphics.Font(fontname, fontsize);

           sublen = Width / font.SizeText("A").Width - 1;

           resync |= true;
   }

   public  void setBordered(bool value)
   {
           mode &= ~MODE_BORDERED;

       if (value)
           mode |= MODE_BORDERED;

           resync |= true;
   }

   public  void setFrozen(bool value)
   {
           mode &= ~MODE_FROZEN;

       if (value)
           mode |= MODE_FROZEN;

           resync |= true;
   }

   public  delegate void FOnCompleted(EditBox sender);

           FOnCompleted completed;

   public  FOnCompleted OnCompleted
   {
           get  {return completed;}
           set  {completed = value;}
   }

   public  string Text
   {
           get  {return text;}
           set  {setText(value);}
   }

   public  void Focus()
   {
           Desktop.WidgetWantFocus(this);
   }

   public  void Defocus()
   {
       if ((mode & MODE_FOCUSED) != 0)
           Desktop.WidgetWantFocus(null);
   }
}

/*namespace Game.Ui.Controls*/ }
