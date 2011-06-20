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
using System.Collections.Generic;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using Game.Ui;
using Game.Ui.Controls;
using Core;
using Core.Net;
using Core.World;
using Core.Cortex;

namespace Game {

public class Menu :App
{
   internal Settings    Settings;
   internal World       World;
   internal Cortex      Cortex;

           Hourglass     mHourglass;
           AnimatedLabel mTicker;
           List<Button>  buttons;

   public Menu() 
   {
           bgimage = (new Surface("res/gamebg.png")).Convert(Video.Screen, true, false);

           Settings = Desktop.Settings;

           mServerDialog = new ServerDialog(this);

           mConfigWindow = new ConfigWindow(this);

           mErrorWindow = new ErrorWindow(this);

           buttons = new List<Button>();
           buttons.Add(new Button(this, 400, 16, MenuButtonFunction.Single));
           buttons.Add(new Button(this, 450, 16, MenuButtonFunction.JoinGame));
           buttons.Add(new Button(this, 500, 16, MenuButtonFunction.CreateGame));
           buttons.Add(new Button(this, 550, 16, MenuButtonFunction.Settings));
           buttons.Add(new Button(this, 600, 16, MenuButtonFunction.Quit));

           foreach(Button button in buttons)
           {
                   button.OnSlide = ButtonSlide;
                   button.OnClick = ButtonClick;
           }

           mHourglass = new Hourglass(this, 364, 60);

           mTicker = new AnimatedLabel(this, 412, 66);

           Desktop.Jukebox.Mode = Jukebox.MENU;
   }

   ~Menu()
   {
    /*
        if (World != null)
        {   World.Dispose();
            World = null;
        }

        if (Cortex != null)
        {   Cortex.Dispose();
            Cortex = null;
        }
   */
           Desktop.Jukebox.Drop();
           Desktop.Sampler.Drop();
   }

           ServerDialog  mServerDialog;
           ConfigWindow mConfigWindow;
           ErrorWindow  mErrorWindow;

   internal void LockMenu(Button current)
   {
           current.setFrozen(true);

        foreach(Button button in buttons)
        {
           if (button != current)
               button.Disable();
        }
   }

   internal void UnlockMenu()
   {
        foreach(Button button in buttons)
        {      
                button.Enable();
        }
   }

           bool ingameplay;

   internal int LockGameplay(object sender)
   {
       if (ingameplay)
           return 0;

           ingameplay = true;

           mErrorWindow.ResetEnabled(this, false);

           World.Config WorldCfg = new World.Config();
           WorldCfg.Share = Settings.Share;

           Cortex.Config CortexCfg = new Cortex.Config();
           CortexCfg.Share = Settings.Share;
           CortexCfg.Policy = Settings.Policy;
           CortexCfg.Nick = Settings.Nick;

       try
       {
           if (Settings.Share == GameShare.Internet)
           {
               if (Settings.Policy == GamePolicy.Guest)
               {
                   if (mServerDialog.Execute())
                   {
                       Settings.RemoteHost = mServerDialog.Host;

                       CortexCfg.Host = Settings.RemoteHost;
                   }
               }
           }

           if (Settings.Share == GameShare.Private)
           {
           }

               mHourglass.Show();

           if (Settings.Policy != GamePolicy.Guest)
               World = World.New(ref WorldCfg);

               Cortex = Cortex.New(ref CortexCfg);

           //if (Cortex.psProbe() != Cortex.Error.Okay)
           //    throw new Exception(string.Format("Unable to connect to host [{0}]", CortexCfg.Host));

           if (Cortex.psLogin(Settings.Nick) != Cortex.Error.Okay)
               throw new Exception(string.Format("Unable to connect to host [{0}]", CortexCfg.Host));

               Cortex.Base = mConfigWindow;

               mConfigWindow.ResetEnabled(this, true);

               return 0;
       }
       catch(Exception e)
       {
               Console.WriteLine("Error: {0}", e.ToString());
               mErrorWindow.Message = e.Message;
               mErrorWindow.ResetEnabled(this, true);
               return 1;
       }
       finally
       {
               mHourglass.Hide();
       }
   }

   internal int ExitGameplay(object sender)
   {
        if (!ingameplay)
            return 0;

               mHourglass.Show();

        if (Cortex != null)
        {   Cortex.Dispose();
            Cortex = null;
        }
 
        if (World != null)
        {   World.Dispose();
            World = null;
        }

           mConfigWindow.ResetEnabled(this, false);
           mServerDialog.ResetEnabled(this, false);
           mErrorWindow.ResetEnabled(this, false);
           UnlockMenu();
           mHourglass.Hide();
           ingameplay = false;
           return 0;
   }

   internal int LockSettings(object sender)
   {
            return 0;
   }

   internal int ExitSettings(object sender)
   {
            return 0;
   }

   private void ButtonSlide(Button sender)
   {
        if (sender.MenuFunction != null)
        {
            mTicker.Text = sender.Caption;
        }
   }

   private void ButtonClick(Button sender)
   {
        if (sender.MenuFunction != null)
        {
            switch (sender.MenuFunction)
            {
              case  MenuButtonFunction.Single:
              {
                    Settings.Share = GameShare.Private;
                    Settings.Policy = GamePolicy.Host;
                    Desktop.Jobs.Schedule(null, LockGameplay);
              }
                    break;

              case  MenuButtonFunction.JoinGame:
              {
                    Settings.Share = GameShare.Internet;
                    Settings.Policy = GamePolicy.Guest;
                    Desktop.Jobs.Schedule(null, LockGameplay); 
              }
                    break;

              case  MenuButtonFunction.CreateGame:
              {
                    Settings.Share = GameShare.Internet;
                    Settings.Policy = GamePolicy.Host;
                    Desktop.Jobs.Schedule(null, LockGameplay); 
              }
                    break;

              case  MenuButtonFunction.Quit:
              {
                    Events.QuitApplication();
              }
                    break;
            }
        }

            LockMenu(sender);
   }

   public  override  void DrawGl(float dt)
   {
   }
}

class ServerDialog :Dialog
{
           Label         mServerLabel;
           EditBox       mServerEdit;

   public ServerDialog(Menu owner)
   :base(owner, 400, 88, 320, 48)
   {
           mServerLabel = new Label(this, 12, 16, 96, 24);
           mServerLabel.setFont("res/xscale.ttf", 0);
           mServerLabel.setColor(Color.FromArgb(255, 64, 192, 64));
           mServerLabel.setText("Host");

           mServerEdit = new EditBox(this, 96, 18, 216, 20);
           mServerEdit.setBordered(true);
           mServerEdit.OnCompleted = EditBoxCompleted;

           CloseOnComplete = false;
   }

   private void EditBoxCompleted(EditBox sender)
   {
       if (sender == mServerEdit)
       {
           done(sender.Text.Length > 0);
           mServerEdit.Defocus();
       }
   }

   public bool Execute()
   {
           bool ret;
           mServerLabel.setAnimated(true);
           mServerEdit.setText("");
           mServerEdit.setFrozen(false);
           ret = base.Execute();
           mServerLabel.setAnimated(false);
           mServerEdit.setFrozen(true);
           return ret;
   }

   public string Host
   {
           get {return mServerEdit.Text;}
   }
}

class ConfigWindow :Window, Cortex.iBase
{
           Menu          menu;

           Label         mNickLabel;
           EditBox       mNickEdit;

           Label         mTeamLabel;
           NickList      mNames;

           Play          mPlay;
           List<Button>  buttons;

   public ConfigWindow(Menu owner)
   :base(owner, 400, 136, 320, 284)
   {
           menu = owner;
           //bgimage = new Surface("res/dialog3.png");

           mNickLabel = new Label(this, 12, 16, 96, 24);
           mNickLabel.setFont("res/xscale.ttf", 0);
           mNickLabel.setColor(Color.FromArgb(255, 64, 192, 64));
           mNickLabel.setText("Name");
           mNickLabel.setAnimated(false);

           mNickEdit = new EditBox(this, 12, 40, 192, 16);
           mNickEdit.setText(Desktop.Settings.Nick);
           mNickEdit.OnCompleted = NickBoxCompleted;

           mTeamLabel = new Label(this, 12, 64, 96, 24);
           mTeamLabel.setFont("res/xscale.ttf", 0);
           mTeamLabel.setColor(Color.FromArgb(255, 64, 192, 64));
           mTeamLabel.setText("Team");
           mTeamLabel.setAnimated(false);

           mNames = new NickList(this, 12, 96);

           mPlay  = new Play();
   
           buttons = new List<Button>();
           buttons.Add(new Button(this, 224, 16, BarButtonFunction.Play));
           buttons.Add(new Button(this, 224, 48, BarButtonFunction.Logout));
           buttons.Add(new Button(this, 224, 80, BarButtonFunction.Cancel));

           foreach(Button button in buttons)
           {
                   button.OnClick = ButtonClick;
           }
   }

   private void ButtonClick(Button sender)
   {
        if (sender.BarFunction != null)
        {
            switch (sender.BarFunction)
            {
              case  BarButtonFunction.Play:
                    Desktop.Jobs.Schedule(null, Play);
                    break;

              case  BarButtonFunction.Logout:
                    break;

              case  BarButtonFunction.Cancel:
                    Desktop.Jobs.Schedule(null, Leave);
                    break;
            }
        }
   }

   private  void NickBoxCompleted(EditBox sender)
   {
            sender.Defocus();
       if  (Cortex.psLogin(sender.Text) == Cortex.Error.Okay)
            menu.Settings.Nick = Cortex.Nick;

            sender.Text = menu.Settings.Nick;
   }

   private  int Play(object sender)
   {
       if  (mNickEdit.Text != menu.Settings.Nick)
            if  (Cortex.psLogin(mNickEdit.Text) == Cortex.Error.Okay)
                 menu.Settings.Nick = Cortex.Nick;

            Cortex.psPrepare();
            Cortex.Dynamics = mPlay;
            return 0;
   }

   private  int Leave(object sender)
   {
            menu.ExitGameplay(null);
            return 0;
   }

   private  void LockGameScene()
   {
            ResetEnabled(null, false);

            Desktop.AppSelect(mPlay);
            mPlay.Prepare();
            mPlay.Run();
            mPlay.OnGameOver = ExitGameScene;
            //Cortex.psReady();
   }

   private  void ExitGameScene()
   {
            mPlay.End();
            mNames.Clear();
            Desktop.AppSelect(menu);
            menu.ResetEnabled(this, true);
            Leave(null);
   }

   public  void adDropped(Cortex sender)
   {
            Console.WriteLine("Dropping connection to server.");
            Desktop.Jobs.Schedule(null, Leave);
   }

   public  void ubRehashed(Cortex sender, NameList users)
   {
           mNames.Suspend();
           mNames.Clear();

           foreach (User u in users)
           {
                    mNames.Add(u.getSID(), u.getNick(), u.getGID());

                if ((u.flags & Proto.F_SELF) != 0)
                    mNickEdit.Text = u.nick;
           }

           mNames.Resume();
   }

   public  void ubModified(Cortex sender, User user)
   {
           mNames.UpdateNick(user.getSID(), user.getNick());
   }

   public  void ubJoined(Cortex sender, User user)
   {
           mNames.Add(user.getSID(), user.getNick(), user.getGID());
           Console.WriteLine("Joined: {0}", user.getNick());
   }

   public  void ubLeft(Cortex sender, User user)
   {
           mNames.Drop(user.getSID());
           Console.WriteLine("Left: {0}", user.getNick());
   }

   public  void gxLoad(Cortex sender)
   {
           LockGameScene();
   }

   public  void gxReady(Cortex sender)
   {
   }

   public  void gxOver(Cortex sender)
   {
           ExitGameScene();
   }

   public override void ResetEnabled (Widget authority, bool value)
   {
           base.ResetEnabled (authority, value);

       if (!value)
       {
           mNames.Clear();
       }
   }
}

class SettingsWindow
{
}

class ErrorWindow :Window
{
           Menu          menu;
           Label         mCaptionLabel;
           Label         mMessageLabel;

           Button        mButton;

   public ErrorWindow(Menu owner)
   :base(owner, 400, 136, 320, 64)
   {
           menu = owner;
           bgimage = new Surface("res/dialog2.png");

           mCaptionLabel = new Label(this, 12, 8, 96, 24);
           mCaptionLabel.setFont("res/xscale.ttf", 0);
           mCaptionLabel.setColor(Color.FromArgb(255, 255, 64, 64));
           mCaptionLabel.setText("Error");
           mCaptionLabel.setAnimated(true);

           mMessageLabel = new Label(this, 12, 36, 296, 18);
           mMessageLabel.setFont("res/xscale.ttf", 0);
           mMessageLabel.setColor(Color.FromArgb(255, 255, 255, 255));
           mMessageLabel.setText("<insert message here | imagine the worst | be creative>");
           mMessageLabel.setAnimated(false);

           mButton = new Button(this, 236, 0, BarButtonFunction.Cancel);
           mButton.OnClick = ButtonClick;
   }

   private void ButtonClick(Button sender)
   {
           Desktop.Jobs.Schedule(null, menu.ExitGameplay);
   }

   public  string Message
   {
           get {return null;}
           set {mMessageLabel.setText(value);}
   }
}
/*namespace Game*/ }
