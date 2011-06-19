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

namespace Game.Ui.Controls {

public class Message
{
           User  sender;
           string message;

           const float timeout = 10.0f;
           float time;

   public Message(User sender, string message)
   {
           this.sender = sender;
           this.message = message;
           this.time = timeout;
   }

   public void Sync(float dt)
   {
       if (time > 0.0f)
       {
           time -= dt;
       }
   }

   public void Draw(Point pos, SdlDotNet.Graphics.Font font, Surface dst)
   {
          byte  alpha = (time > 1.0f) ? (byte)255 : (byte)(255 * time);
          string head = string.Format("<{0}> ", sender.nick);

          Color head_color = Color.FromArgb(alpha, 0, 255, 0);
          dst.Blit(font.Render(head, head_color), pos);

          pos.Offset(font.SizeText(head).Width, 0);
          Color message_color = Color.FromArgb(alpha, 240, 240, 255);
          dst.Blit(font.Render(message, message_color), pos);
   }

   public bool Expired()
   {
          return time <= 0.0f;
   }
}

public class HudChat :Widget
{
           Box box;

           string text = "";
           int    maxlen = 128;

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

           List<Message> messages;
           List<Message> drops;

   public  HudChat(Widget parent, int x, int y)
   :base(parent, x, y, 320, 12, true)
   {
           setFont("res/andalemo.ttf", 10);

           messages = new List<Message>();
           drops = new List<Message>();
   }

   public  void setFont(string FontName, int FontSize)
   {
           fontname = FontName;
           fontsize = FontSize;

           font = new SdlDotNet.Graphics.Font(fontname, fontsize);

           sublen = Width / font.SizeText("A").Width - 1;

           resync |= true;
   }

   public  void setText(string Text)
   {
           text = Text;

       if (text.Length >= maxlen)
           text = text.Substring(0, maxlen);

       if (text.Length <= sublen)
           subtext = text;
           else
           subtext = text.Substring(text.Length - sublen, sublen);

           resync = true;
   }

   public  override void Update(KeyboardEventArgs e)
   {
       if (e.Type != EventTypes.KeyDown)
           return;

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
               if ((mode ^= MODE_FOCUSED) == 0)
               {
                   if (text.Length != 0)
                       if (completed != null)
                           completed(text);
               }

                  setText("");
                  break; 

             default:
               if (e.KeyboardCharacter.Length == 1)
                   setText(text + e.KeyboardCharacter);
                  break;
           }
   }

           bool resync;

   public  override bool Sync(float dt)
   {
       if ((mode & MODE_FOCUSED) != 0)
       {
           if (cursort >= 1.0f)
           {
               if (cursor.Length == 0)
                   cursor = "_";
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

           drops.Clear();

           foreach (Message msg in messages)
           {
               msg.Sync(dt);

               if (msg.Expired())
                   drops.Add(msg);
           }

           foreach (Message msg in drops)
           {
               messages.Remove(msg);
           }

           return resync;
   }

   public  override void Draw(Surface dst)
   {
           Point p = Position;

       if ((mode & MODE_FOCUSED) != 0)
       {
           dst.Draw(new Box((short)(p.X - 2), (short)(p.Y - 1), (short)(p.X + Width), (short)(p.Y + 13)), Color.FromArgb(96, 4, 8, 32), true, true);
           dst.Blit(font.Render(subtext + cursor, Color.White), p);
       }

           foreach (Message m in messages)
           {
                    p.Offset(0, -16);

                    m.Draw(p, font, dst);
           }
   }

   public  void  Notify(User sender, string message)
   {
           messages.Insert(0, new Message(sender, message));
   }

   public  delegate void FOnCompleted(string message);

           FOnCompleted completed;

   public  FOnCompleted OnCompleted
   {
           get  {return completed;}
           set  {completed = value;}
   }

   public  string Text
   {
           get  {return text;}
   }
}

/* namespace Game.Ui.Controls */ }
