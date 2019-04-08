﻿using System;
using Android.App;
using Android.Content;
using Android.OS;
using Duality;
using Duality.Backend;
using Jazz2.Game;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform.Android;
using ContentResolver = Jazz2.Game.ContentResolver;
using Environment = System.Environment;
using INativeWindow = Duality.Backend.INativeWindow;
using Vector2 = Duality.Vector2;

namespace Jazz2.Android
{
    public partial class InnerView : AndroidGameView
    {
        private App current;
        private int viewportWidth, viewportHeight;

        private readonly Vibrator vibrator;

        public InnerView(Context context) : base(context)
        {
            vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Initialize core
            // ToDo: Create Android-specific AssemblyLoader
            DualityApp.Init(DualityApp.ExecutionContext.Game, null, null);

            ContentResolver.Current.Init();
            
            viewportWidth = Width;
            viewportHeight = Height;

            DualityApp.WindowSize = new Point2(viewportWidth, viewportHeight);
            INativeWindow window = DualityApp.OpenWindow(new WindowOptions());

            ContentResolver.Current.InitPostWindow();

            // Initialize input
            FocusableInTouchMode = true;
            RequestFocus();

            InitializeInput();

            // Initialize the game
            current = new App(window);
            current.ShowMainMenu();

            // Run the render loop
            Run(60);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            DualityApp.Terminate();
        }

        protected override void CreateFrameBuffer()
        {
            ContextRenderingApi = GLVersion.ES3;

            // The default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
            try {
                base.CreateFrameBuffer();
                return;
            } catch (Exception ex) {
                App.Log("GLView.CreateFrameBuffer() threw an exception: " + ex);
            }

            // This is a graphics setting that sets everything to the lowest mode possible so
            // the device returns a reliable graphics setting.
            try {
                GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

                base.CreateFrameBuffer();
                return;
            } catch (Exception ex) {
                App.Log("GLView.CreateFrameBuffer() threw an exception: " + ex);
            }

            throw new BackendException("Cannot initialize OpenGL ES 3.0 device");
        }

        protected override void OnResize(EventArgs e)
        {
            viewportHeight = Height;
            viewportWidth = Width;

            DualityApp.WindowSize = new Point2(viewportWidth, viewportHeight);

            MakeCurrent();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //base.OnUpdateFrame(e);

            if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated) {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) {
                    (Context as Activity).FinishAndRemoveTask();
                } else {
                    (Context as Activity).Finish();
                }
                Environment.Exit(0);
                return;
            }

            DualityApp.Update();
            
#if ENABLE_TOUCH
            ControlScheme.UpdateTouchActions();
#endif
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //base.OnRenderFrame(e);

            DualityApp.Render(null, new Rect(viewportWidth, viewportHeight), new Vector2(viewportWidth, viewportHeight));

            SwapBuffers();
        }

        protected override void OnContextLost(EventArgs e)
        {
            base.OnContextLost(e);

            // Reinitialize core
            DualityApp.Terminate();
            DualityApp.Init(DualityApp.ExecutionContext.Game, null, null);

            ContentResolver.Current.Init();

            viewportWidth = Width;
            viewportHeight = Height;

            DualityApp.WindowSize = new Point2(viewportWidth, viewportHeight);
            INativeWindow window = DualityApp.OpenWindow(new WindowOptions());

            ContentResolver.Current.InitPostWindow();

            // Reinitialize input
            virtualButtons = null;

            InitializeInput();

            // Reinitialize the game
            current = new App(window);
            current.ShowMainMenu();
        }
    }
}