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
using SdlDotNet.Core;
using SdlDotNet.Graphics;

namespace Game.Ui.Controls {

public class AnimatedLabel :Widget
{
           SdlDotNet.Graphics.Font font;
           TextMachine textmachine;
           Surface     textsurface;

   public AnimatedLabel(Widget parent, int x, int y) 
   :base(parent, x, y, 200, 24, true)
   {
           font = new Font("res/arcade.ttf", 20);
           textmachine = new TextMachine("Ohai!", 2);
   }

           bool resync = true;

   public  override bool Sync(float dt)
   {
           return  resync = textmachine.Sync();
   }

   public  override void Draw(Surface dst)
   {
       if (resync)
       {
           textsurface = font.Render(textmachine.Read(), System.Drawing.Color.White, true);
           Surface.Fill(System.Drawing.Color.FromArgb(0x00000001));
           Surface.Blit(textsurface);
           Surface.Update();
           resync = false;
       }

           dst.Blit(this);
   }

   public string Text
   {
          get {return textmachine.Message;}
          set {textmachine.Message = value;}
   }
}


class TextMachine //shitty name, i know
{
           string text;
           List<char> bits;
           int    steps;

           bool   done;
           int    step;
           int    limit;

   public TextMachine(string atext, int asteps)
   {
           text = null;
           bits = new List<char>();
           steps = asteps;

           done = false;
           Reset(atext);
   }

   public void Reset(string atext)
   {
           text = atext;
           step = steps;
           done = false;

           limit = (bits.Count < text.Length) ? bits.Count : text.Length;
   }

   public string Read()
   {
          string ret = "";

          foreach(char c in bits)
          {
             ret += c;
          }

          return ret;
   }

   public bool Sync()
   {
       if (done)
           return false;

           done = --step == 0;

       if (bits.Count < text.Length)
       {
           bits.Add(text[bits.Count]);

           if (limit > bits.Count)
               limit = bits.Count;

               step  = steps;
       }

       if (bits.Count > text.Length)
       {
           bits.RemoveAt(bits.Count - 1);

           if (limit > text.Length)
               limit = text.Length;

               step  = steps;
       }

       if  (done)
       {           
            for (int x = 0; x != limit; x++)
            {
                 bits[x] = text[x];
            }
       }    else
       {
            for (int x = 0; x != limit; x++)
            {
                 if (bits[x] < 'z')
                     bits[x]++;
            }
       }

           return true;
   }

   public string Message
   {
          get {return text;}
          set {Reset(value);}
   }
}

/*namespace Game.Ui.Controls*/ }
