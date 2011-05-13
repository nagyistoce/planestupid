
using System;

namespace Core.Phy {

public class Body
{
   protected Space  space;
   protected vector vl;
   protected vector va;

   public interface Mesh
   {
   }

           Mesh   mesh;

   public static int MX_LINEAR_VELOCITY = 0x10;
   public static int MX_ANGULAR_VELOCITY = 0x20;
   public static int MX_POSITION = 0x40;
   public static int MX_ROTATION = 0x80;

   public interface Monitor
   {
           void  Sync(Body sender, int mxflags);
   }

           int     mxflags;
           Monitor monitor;

   public interface Ai
   {
   }

           Ai     ai;

           vector pos;
           vector rot;

           float  mass;

           int    mode;
           bool   passive;
           bool   enabled;

   public  Body(Space space)
   {
           passive = true;
           enabled = true;
   }

   public  void  AttachMesh(Mesh mesh)
   {
   }

   public  void  AttachMonitor(Monitor monitor)
   {
   }

   public  void  AttachAi(Ai ai)
   {
   }

   public  void  Move(vector pos)
   {
           this.pos = pos;
           this.mxflags |= MX_POSITION;
   }

   public  void  Rotate(vector rot)
   {
           this.rot = rot;
           this.mxflags |= MX_ROTATION;
   }

   public  virtual void  Step(float dt)
   {
       if (enabled)
       {   
       }

       if (mxflags != 0)
       {
           if (monitor != null)
               monitor.Sync(this, mxflags);

           mxflags  = 0;
       }
   }
}

/*namespace Core.Phy*/ }
