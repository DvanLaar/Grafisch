using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using template_P3;
using System;
using System.Drawing;
using OpenTK.Input;

// minimal OpenTK rendering framework for UU/INFOGR
// Jacco Bikker, 2016

namespace Template_P3
{

    class Game
    {
        // member variables
        public Surface screen;                  // background surface for printing etc.
        Mesh mesh, floor;                       // a mesh to draw using OpenGL
        const float PI = 3.1415926535f;         // PI
        Camera camera;                          // teapot rotation angle
        Stopwatch timer;                        // timer for measuring frame duration
        Shader shader;                          // shader to use for rendering
        FurShader furshader;

        Shader postproc;                        // shader to use for post processing
        Texture wood;                           // texture to use for rendering
        Texture fur;
        RenderTarget target;                    // intermediate render target
        ScreenQuad quad;                        // screen filling quad for post processing
        bool useRenderTarget = true;

        SceneGraph scene;
        Model teapotmodel;

        public static Vector3 camerapos;

        // initialize
        public void Init()
        {
            // initialize camera
            camera = new Camera(new Vector3(0, -4, 15));
            // initialize stopwatch
            timer = new Stopwatch();
            timer.Reset();
            timer.Start();
            // create shaders
            shader = new Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
            furshader = new FurShader("../../shaders/vs_fur.glsl", "../../shaders/fs_fur.glsl");
            postproc = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl");
            // load teapot
            mesh = new Mesh("../../assets/teapot.obj");
            floor = new Mesh("../../assets/floor.obj");
            // load a texture
            fur = new Texture("../../assets/fur.png");
            wood = new Texture("../../assets/wood.jpg");
            // create the render target
            target = new RenderTarget(screen.width, screen.height);
            quad = new ScreenQuad();

            scene = new SceneGraph();

            SceneNode mainNode = new SceneNode();
            teapotmodel = new Model(mesh, wood, shader, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));
            Model floormodel = new Model(floor, wood, shader, Matrix4.Identity);

            // FurModel furry = new FurModel(mesh, wood, fur, shader, furshader, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));
            // mainNode.AddChildModel(furry);
            mainNode.AddChildModel(teapotmodel);
            mainNode.AddChildModel(floormodel);

            scene.mainNode = mainNode;
        }

        public void processKeyboard(KeyboardState keyboard)
        {
            // rotation.X : left/right
            // rotation.Y : up/down
            Vector2 rotation = Vector2.Zero;
            Vector3 translation = Vector3.Zero;

            if (keyboard[Key.Up]) rotation += Vector2.UnitY;
            if (keyboard[Key.Down]) rotation -= Vector2.UnitY;
            if (keyboard[Key.Left]) rotation += Vector2.UnitX;
            if (keyboard[Key.Right]) rotation -= Vector2.UnitX;

            if (keyboard[Key.S]) translation += Vector3.UnitZ;
            if (keyboard[Key.W]) translation -= Vector3.UnitZ;
            if (keyboard[Key.E]) translation += Vector3.UnitY;
            if (keyboard[Key.Q]) translation -= Vector3.UnitY;
            if (keyboard[Key.D]) translation += Vector3.UnitX;
            if (keyboard[Key.A]) translation -= Vector3.UnitX;

            camera.AddTransformation(rotation * 0.1f, translation);
        }

        // tick for background surface
        public void Tick()
        {
            screen.Clear(0);
            screen.Print("hello world", 2, 2, 0xffff00);
        }

        // tick for OpenGL rendering code
        public void RenderGL()
        {
            // measure frame duration
            float frameDuration = timer.ElapsedMilliseconds;
            timer.Reset();
            timer.Start();

            // prepare matrix for vertex shader
            Matrix4 transform = camera.Matrix;

            camerapos = camera.Position;


            // update rotation
            //camera.RotateYaw(0.001f * frameDuration);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            //GL.DepthMask(false);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (useRenderTarget)
            {
                // enable render target
                target.Bind();
                // render scene to render target
                scene.Render(transform);
                // render quad
                target.Unbind();

                quad.Render(postproc, target.GetTextureID());
            }
            else
            {
                // render scene directly to the screen
                scene.Render(transform);
            }
        }
    }

} // namespace Template_P3