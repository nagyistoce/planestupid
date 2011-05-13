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
using System.IO;
using SdlDotNet.Graphics;
using Tao.OpenGl;
using Core.Net;

namespace Core.Scene {

public class Mesh
{
           string src;

           vector[] vt; //vertex table
           byte[] dt; //display table

   public  Material material;
   public  Texture texture;
   public  Map  texmap;

           bool fault = false;

   const byte MP_NO = 0;

   const byte OP_TRI = 0x03;
   const byte OP_QUAD = 0x04;
   const byte OP_COLOR = 0x08;

   public  Mesh()
   {
   }

   public  Mesh(string src)
   {
           this.src = src;
           this.load();
   }

   ~Mesh()
   {
           this.free();
   }

   private bool load()
   {
       if (!File.Exists(src))
           return false;

           lock(this) {

                     FileStream F = null;
                     BinaryReader Rd;

           try
           {
                     F = new FileStream(src, FileMode.Open, FileAccess.Read);
                     Rd = new BinaryReader(F);

                byte magic  = Rd.ReadByte();
                byte mapper = Rd.ReadByte();
                int  nverts = Rd.ReadInt32();
                int  nfaces = Rd.ReadInt32();

                     vt = new vector[nverts];

                for (int x = 0; x != nverts; x++)
                {
                     vt[x] = new vector(
                             Rd.ReadSingle(),
                             Rd.ReadSingle(),
                             Rd.ReadSingle()
                     );
                }

                int  size = (int)F.Length - (int)F.Position;

                     dt = new byte[size];

                int  pos = 0;

                while((pos += F.Read(dt, pos, size - pos)) < size);

                     fault = false;
           }
           catch (Exception e)
           {
                  vt = null;
                  dt = null;
                  fault = true;
                  Console.WriteLine("Error loading mesh {0}: {1}", src, e.ToString());
                  return false;
           }
           finally
           {
                  F.Close();
           }
           /*lock*/ }
           return true;
   }

   private bool free()
   {
           lock(this) {
           /*lock*/ }
           return true;
   }

   private bool Apply(int nDetail)
   {
           if (fault)
               return false;

           int  r = 0;
           int  x;

           try
           {
                byte op;

                if (material != null)
                    material.Apply();

                if (texture != null)
                    texture.Apply();

                    Gl.glBegin(Gl.GL_TRIANGLES);

                while(r < dt.Length)
                {
                      switch(op = dt[r++])
                      {
                        case OP_TRI:
                        {
                             int n;

                             for (x = 0; x != 3; x++)
                             {
                                  Proto.getn(ref dt, ref r, out n);
                                  Gl.glVertex3f(vt[n].x, vt[n].y, vt[n].z);
                             }
                        }
                             break;
                      }
                }
           }
           catch(Exception e)
           {
                 Console.WriteLine("Error drawing mesh {0}: {1}", src, e.ToString());
                 fault = true;
                 return false;
           }
           finally
           {
                 Gl.glEnd();
           }

           return true;
   }

   public  bool Draw(int nDetail)
   {
           return Apply(nDetail);
   }

           bool bRef;

   public  Mesh Ref()
   {
           lock(this) {
           /*lock*/}

           return this;
   }

   public  void Deref()
   {
           lock(this) {
           /*unlock*/}
   }
}


public class Material
{
           int glFace;

   /* ambient RGBA reflectance */
   public float[] ambient; //=new byte[]{0.2, 0.2, 0.2, 1.0}

   /* diffuse RGBA reflectance */
   public float[] diffuse; //=new byte[]{0.8, 0.8, 0.8, 1.0}

   /* specular RGBA reflectance */
   public float[] specular; //=new byte[]{0.0, 0.0, 0.0, 1.0}

   /* emision RGBA */
   public float[] emission; //=new byte[]{0.0, 0.0, 0.0, 1.0}

   /* RGBA specular exponent*/
   public float[] shininess; //=new byte[]{0.0}

   public Material(int glFace)
   {
   }

   public  void Apply()
   {
       if (ambient != null)
           Gl.glMaterialfv(glFace, Gl.GL_AMBIENT, ambient);

       if (diffuse != null)
           Gl.glMaterialfv(glFace, Gl.GL_DIFFUSE, specular);

       if (specular != null)
           Gl.glMaterialfv(glFace, Gl.GL_SPECULAR, specular);

       if (emission != null)
           Gl.glMaterialfv(glFace, Gl.GL_EMISSION, specular);

       if (shininess != null)
           Gl.glMaterialfv(glFace, Gl.GL_SHININESS, shininess);
   }
}


public class Texture
{
           Surface tex;
           int    glnum;

   public Texture(string src)
   {
           tex = new Surface(src);
   }


   public  void Apply()
   {
           Gl.glBindTexture(Gl.GL_TEXTURE_2D, glnum);
   }

           bool bRef;

   public  Texture Ref()
   {
       if (!bRef)
       {
           Gl.glGenTextures(1, out glnum);
  
           Gl.glBindTexture(Gl.GL_TEXTURE_2D, glnum);

           Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, 3, tex.Width, tex.Height, 0, 
                           Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, tex.Pixels);

           Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
           Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);

           bRef = true;
       }

           return this;
   }

   public  void Deref()
   {
           bRef = false;
   }

   public  int GLNum
   {
          get {Ref(); return glnum;}
   }
}

public class Map
{
   public float[] repeat = new float[]{1.0f, 1.0f, 1.0f};

   public Map()
   {
   }
}

/*namespace Core.Scene*/ }
