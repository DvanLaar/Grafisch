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
        Shader postbloomblend;
        PostVigAndChromShader vigandchromshader;
        PostKernelShader kernelshader;
        Kernel kernel;

        Texture wood;                           // texture to use for rendering
        Texture fur;

        RenderTarget target;                    // intermediate render target
        RenderTarget target2;
        RenderTarget target3;
        RenderTarget targethdr;
        RenderTarget bloomtarget;

        ScreenQuad quad;                        // screen filling quad for post processing
        bool useRenderTarget = true;

        SceneGraph scene;
        Model teapotmodel;

        public static Vector3 camerapos;

        // initialize
        public void Init()
        {
            // initialize camera
            camera = new Camera(new Vector3(0, 3, 15));
            // initialize stopwatch
            timer = new Stopwatch();
            timer.Start();
            // create shaders
            shader = new Shader("../../shaders/vs.glsl", "../../shaders/fs.glsl");
            furshader = new FurShader("../../shaders/vs_fur.glsl", "../../shaders/fs_fur.glsl");
            postproc = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_post.glsl");
            kernelshader = new PostKernelShader("../../shaders/vs_post.glsl", "../../shaders/fs_kernel.glsl");
            vigandchromshader = new PostVigAndChromShader("../../shaders/vs_post.glsl", "../../shaders/fs_vigchrom.glsl");
            postbloomblend = new Shader("../../shaders/vs_post.glsl", "../../shaders/fs_bloomblend.glsl");
            // load teapot
            mesh = new Mesh("../../assets/teapot.obj");
            floor = new Mesh("../../assets/floor.obj");
            // load a texture
            fur = new Texture("../../assets/fur.png");
            wood = new Texture("../../assets/wood.jpg");

            Resize();

            quad = new ScreenQuad();
            scene = new SceneGraph();

            SceneNode mainNode = new SceneNode();
            teapotmodel = new Model(mesh, wood, shader, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));

            Model floormodel = new Model(floor, wood, shader, Matrix4.Identity);
            floormodel.materialcolor = new Vector3(100f, 1f, 1f);
            // FurModel furry = new FurModel(mesh, wood, fur, shader, furshader, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));
            // mainNode.AddChildModel(furry);

            mainNode.AddChildModel(teapotmodel);
            mainNode.AddChildModel(floormodel);

            scene.mainNode = mainNode;

            kernel = Kernel.SmallGaussianBlur;
        }

        public void Resize()
        {
            // create the render target
            target = new RenderTarget(screen.width, screen.height);
            target2 = new RenderTarget(screen.width, screen.height);
            target3 = new RenderTarget(screen.width, screen.height);

            targethdr = new RenderTarget(screen.width, screen.height, 2);
            bloomtarget = new RenderTarget(screen.width, screen.height);
        }

        public void processKeyboard(KeyboardState keyboard)
        {
            // measure frame duration
            timer.Stop();
            float frameDuration = timer.ElapsedMilliseconds;
            timer.Restart();

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

            camera.AddTransformation(0.004f * frameDuration * rotation, 0.03f * frameDuration * translation);
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
            // prepare matrix for vertex shader
            Matrix4 transform = camera.Matrix;

            camerapos = -camera.Position;


            // update rotation
            //camera.RotateYaw(0.001f * frameDuration);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            //GL.DepthMask(false);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (useRenderTarget)
            {

                //Bind the HDR target
                targethdr.Bind();
                //Let the GPU now when want to render to 2 textures
                GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                //Render to the textures
                scene.Render(transform);
                //Unbind the HDR target
                GL.DrawBuffers(1, new DrawBuffersEnum[1] { DrawBuffersEnum.ColorAttachment0});
                targethdr.Unbind();

                bloomtarget.Bind();
                quad.KernelRender(kernelshader, targethdr.GetTextureID(1), 640f, 400f, Kernel.Uniform(19, 19, 19));
                bloomtarget.Unbind();

                //Merge bloomtarget and targethdr[0]
                target.Bind();
                quad.BloomBlendRender(postbloomblend, targethdr.GetTextureID(0), bloomtarget.GetTextureID());
                target.Unbind();

                target2.Bind();
                quad.VigAndChromRender(vigandchromshader, target.GetTextureID(),2.3f,new Vector2(0.51f,0.5f), 0.0125f * new Vector3(1f, 0f, -1f) );
                target2.Unbind();

                //quad.Render(postproc, target.GetTextureID());
                quad.KernelRender(kernelshader, target2.GetTextureID(), 640f, 400f, kernel);
            }
            else
            {
                // render scene directly to the screen
                scene.Render(transform);
            }
        }
    }

} // namespace Template_P3