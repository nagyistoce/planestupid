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
using System.Threading;

namespace Game.Ui {

public class Scheduler
{
           Thread thread;
            Queue<Job> queue;

   public Scheduler()
   {
           queue = new Queue<Job>();

           thread = new Thread(new ThreadStart(_perform));
           thread.IsBackground = true;
           thread.Start();
   }

   ~Scheduler()
   {
   }

   private bool dequeue(out Job job)
   {
           bool ret = false;

           lock(this)
           {
                try
                {
                       Monitor.Wait(this);
                       job = queue.Dequeue();
                       ret = job != null;
                }
                catch (SynchronizationLockException e)
                {
                       job = null;
                }
                catch (ThreadInterruptedException e)
                {
                       job = null;
                }

                       Monitor.Pulse(this);
           }

                       return ret;
   }

   private void _perform()
   {
       try
       {
                  Job job;

           while (true)
           {
              if (dequeue(out job))
              {
                  job.execute();
              }
           }
      }
      catch(ThreadAbortException e)
      {
            Console.WriteLine("[Scheduler] Brutally killed...");
      }
      catch(Exception e)
      {
            Console.WriteLine("[Scheduler] Died.");
      }
      finally
      {
      }
   }

   public  Job Schedule(Job job)
   {
           lock(this)
           {
                queue.Enqueue(job);
                Monitor.Pulse(this);
           }

           return job;
   }

   public  Job Schedule(Widget sender, FJob function)
   {
           return Schedule(new Job(sender, function));
   }
}

public delegate int FJob(Widget sender);

public class Job
{
           Widget sender;
           FJob   function;

           object sync;
           int    rc;

  public Job(Widget sender, FJob function)
  {
           this.sender = sender;
           this.function = function;
  }

  internal void execute()
  {
       if (function != null)
           rc = function(sender);

       if (sync != null)
           Monitor.Pulse(sync);
  }

  public static int WRE_BASE = 0;
  public static int WRE_BUSY = WRE_BASE - 1;
  public static int WRE_FAIL = WRE_BASE - 2;

  public int Waitfor(object sync)
  {
       if (sync != null)
           return WRE_BUSY;

           this.sync = sync;

           lock(sync)
           {
                try
                {
                       Monitor.Wait(sync);
                       return rc;
                }
                catch (Exception e)
                {
                       return WRE_FAIL;
                }
           }
  }
}

/*namespace Game.Ui*/}
