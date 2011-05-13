
using System;
using Core.Scene;
using Tao.OpenGl;

namespace graph1t
{
    public class Cube :Thing
    {
           float a;

       public Cube(Space space)
       :base(space, -1, TYPE_UNDEF)
       {
               mode = INT_SOLID;
               mesh = new Mesh("cube.mesh");
               mesh.material = new Material(Gl.GL_FRONT_AND_BACK);
               mesh.material.specular = new float[]{1.0f, 1.0f, 0.0f, 1.0f};
               //mesh.material.emission = new float[]{1.0f, 0.2f, 0.2f, 1.0f};
               mesh.material.ambient = new float[]{0.0f, 0.2f, 0.0f, 1.0f};
               mesh.material.diffuse = new float[]{1.0f, 0.0f, 0.0f, 1.0f};
               mesh.material.shininess = new float[]{5.0f};
               //mesh.texture = new Texture("scene/terrain.bmp");
               //mesh.texmap = new Map();
       }

       public  override bool  Step(float dt)
       {
               a += 3.6f;

           if (a >= 360f)
               a -= 360f;

                return true;
       }

       public  override void  DrawGl(int nDetail)
       {
               Gl.glLoadIdentity();
               //Gl.glTranslatef(0.0f, 0.0f, -1.0f);
               Gl.glRotatef(a, 1.0f, -1.0f, 0.5f);
               Gl.glScalef(0.05f, 0.05f, 0.05f);
               Gl.glColor3f(0.4f, 0.8f, 0.1f);
               mesh.Draw(nDetail);
/*
           Gl.glLoadIdentity();
           //Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
           Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, new float[]{0.0f, 0.0f, 1.0f, 1.0f});
           //Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SHININESS, new float[]{50.0f});
           Gl.glRotatef(rot, 0.5f, -1.0f, 1.0f);
           Gl.glScalef(0.5f, 0.5f, 0.5f);
           Gl.glColor4f(0.32f, 0.32f, 0.5f, 1.0f);
           mesh.Draw(nDetail);
*/

       }

    }


    public class Floor :Thing
    {
       public Floor(Space space)
       :base(space, -1, TYPE_UNDEF)
       {
              mesh = new Mesh();
              //mesh.texture = new Texture("map.png");
              //mesh.texture.Ref();
              //mesh.material = new Material(Gl.GL_FRONT_AND_BACK);
       }

       public  override bool  Step(float dt)
       {
               return true;
       }

       public  override void  DrawGl(int nDetail)
       {
               Gl.glLoadIdentity();
               Gl.glColor3f(0.0f, 0.1f, 1.0f); 
               
               //Gl.glBindTexture(Gl.GL_TEXTURE_2D, mesh.texture.GLNum);
               Gl.glBegin(Gl.GL_QUADS);
               Gl.glNormal3f(0.0f, 0.0f, +1.0f);
               Gl.glTexCoord2f(0.0f, 0.0f); Gl.glVertex3f(-1.0f, -1.0f,  -1.0f);
               Gl.glTexCoord2f(1.0f, 0.0f); Gl.glVertex3f( 1.0f, -1.0f,  -1.0f);
               Gl.glTexCoord2f(1.0f, 1.0f); Gl.glVertex3f( 1.0f,  1.0f,  -1.0f);
               Gl.glTexCoord2f(0.0f, 1.0f); Gl.glVertex3f(-1.0f,  1.0f,  -1.0f);
               Gl.glEnd();
       }

    }
}
