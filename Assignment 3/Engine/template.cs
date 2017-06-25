﻿using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace rasterizer
{
    public class OpenTKApp : GameWindow
    {
        private int screenID;
        private Game game;
        private bool terminated = false;

        protected override void OnLoad(EventArgs e)
        {
            // called upon app init
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            Console.WriteLine("OpenGL version: " + GL.GetString(StringName.Version));

            ClientSize = new Size(640, 400);
            game = new Game();
            Sprite.target = game.screen = new Surface(Width, Height);
            screenID = game.screen.GenTexture();
            game.Init();
        }
        protected override void OnUnload(EventArgs e)
        {
            // called upon app close
            GL.DeleteTextures(1, ref screenID);
            // bypass wait for key on CTRL-F5:
            Environment.Exit(0);
        }
        protected override void OnResize(EventArgs e)
        {
            // called upon window resize
            GL.Viewport(0, 0, Width, Height);

            ClientSize = new Size(Width, Height);
            Sprite.target = game.screen = new Surface(Width, Height);
            game.ResizeWindow();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // called once per frame; app logic
            var keyboard = OpenTK.Input.Keyboard.GetState();
            if (keyboard[OpenTK.Input.Key.Escape]) Exit();
            game.processKeyboard(keyboard);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // called once per frame; render
            game.Tick();
            if (terminated)
            {
                Exit();
                return;
            }
            // set the state for rendering the quad
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);

            GL.Color3(1.0f, 1.0f, 1.0f);
            GL.BindTexture(TextureTarget.Texture2D, screenID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                            game.screen.width, game.screen.height, 0,
                            OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                            PixelType.UnsignedByte, game.screen.pixels
                            );
            // GL.Clear( ClearBufferMask.ColorBufferBit ); /* not needed */

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex2(-1.0f, -1.0f);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex2(1.0f, -1.0f);
            GL.TexCoord2(1.0f, 0.0f); GL.Vertex2(1.0f, 1.0f);
            GL.TexCoord2(0.0f, 0.0f); GL.Vertex2(-1.0f, 1.0f);
            GL.End();

            // prepare for generic OpenGL rendering
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.Texture2D);

            // do OpenGL rendering
            game.RenderGL();

            // swap buffers
            SwapBuffers();
        }

        /// <summary>
        /// entry point
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US"); // thanks Mathijs
            using (OpenTKApp app = new OpenTKApp()) { app.Run(30.0, 0.0); }
        }
    }
}