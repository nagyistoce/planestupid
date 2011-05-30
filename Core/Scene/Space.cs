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
using SdlDotNet;
using SdlDotNet.Graphics;
using Core.Net;

namespace Core.Scene {

/* Space
   collects meshes, materials, textures and bodies
*/

public class Space
{
   public  SortedDictionary<int, Thing> things;
           SortedDictionary<int, Body> bots;

           Size  pxField;
           Rectangle pxViewport;

   public  Point Offset = new Point(0, 0);
   public  Color ClearColor = Color.FromArgb(0);

   public  Surface Surface {get; set;}
   public  Surface Background {get; set;}

   public Space()
   {
           things = new SortedDictionary<int, Thing>();
   }

   public  Space(Size field, Rectangle viewport)
   :this()
   {
           pxField = field;
           pxViewport = viewport;

           Surface = new Surface(pxField.Width, pxField.Height, 32, true);
   }

   public  void  Step(float dt)
   {
           lock(things) {

           foreach(KeyValuePair<int, Thing> Ent in things)
           {
                   Ent.Value.Step(dt);
           }

           /*unlock*/ }
   }

   public  void  Step(float dt, ref List<Thing> Set)
   {
           lock(things) {

           foreach(KeyValuePair<int, Thing> Ent in things)
           {
               if (Ent.Value.Step(dt))
               {
                   Set.Add(Ent.Value);
               }
           }

           /*unlock*/ }
   }

   public  void  Snap(float dt)
   {
           lock(things) {

           foreach(KeyValuePair<int, Thing> Ent in things)
           {
                   Ent.Value.Snap(dt);
           }

           /*unlock*/ }
   }

   public  void  Draw(Surface target, int nDetail)
   {
           lock(things) {

           if (Background != null)
               Surface.Blit(Background);
               else
               Surface.Fill(ClearColor);

                foreach(KeyValuePair<int, Thing> Ent in things)
                {
                        Ent.Value.Draw(Surface, nDetail);
                }

                Surface.Update();

                target.Blit(Surface, Offset, Viewport);
    
           /*unlock*/ }
   }

   public  void  DrawGl(int nDetail)
   {
           lock(things) {

           foreach(KeyValuePair<int, Thing> Ent in things)
           {
                   Ent.Value.DrawGl(nDetail);
           }

           /*unlock*/ }
   }

   public  virtual bool  Serialize(ref byte[] data, ref int s)
   {
           int  re = s;

           lock(things) {

           foreach(KeyValuePair<int, Thing> Ent in things)
           {
               if (!Proto.putn(ref data, ref s, Ent.Key))
               {   s = re;
                   return false;
               }

               if (!Proto.putn(ref data, ref s, Ent.Value.Type))
               {   s = re;
                   return false;
               }

               if (!Ent.Value.Serialize(ref data, ref s))
               {   s = re;
                   return false;
               }
           }

           /*unlock*/ }

           return true;
   }

   public Size Field
   {
          get {return pxField;}
   }

   public Rectangle Viewport
   {
          get {return pxViewport;}
   }

   public  PointF DesktopToSpace(Point p)
   {
           PointF ret = new PointF(-1.0f + (float)p.X / (pxField.Width / 2), 
                                   -1.0f + (float)p.Y / (pxField.Height / 2)
           );
           return ret;
   }

   public  Point SpaceToDesktop(float px, float py)
   {
           Point  ret = new Point((int) ((pxField.Width + px * pxField.Width) / 2.0f),
                                  (int) ((pxField.Height + py * pxField.Width) / 2.0f)
                  );
           return ret;
   }

   public  Point SpaceToDesktop(PointF p)
   {
           return SpaceToDesktop(p.X, p.Y);
   }
}

/*namespace Core.Scene*/ }
