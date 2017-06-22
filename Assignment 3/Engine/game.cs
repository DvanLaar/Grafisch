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
        const float PI = 3.1415926535f;
        const bool useRenderTarget = true;

        // member variables
        public Surface screen; // background surface for printing etc.
        public Camera camera;
        public Stopwatch timer; // timer for measuring frame duration
        public ScreenQuad quad; // screen filling quad for post processing
        public SceneGraph scene;

        // used shaders:
        public Shader shaderDefault, shaderNormal, shaderFur, shaderPostProc, shaderPostBloomBlend, shaderSkybox, shaderReflective;
        public PostVigAndChromShader shaderVigAndChrom;
        public PostKernelShader shaderKernel;
        public Kernel kernel;

        // used meshes:
        public Mesh meshTeapot, meshFloor, meshCube;

        // used textures:
        public Texture textureWood, textureFur, textureWall;
        public CubeTexture textureSkybox;

        // used render targets:
        public RenderTarget targetMain, targetVigAndChrom, targetUnused, targetHDR, targetBloom;
        
        // used models:
        public Model modelTeapot, modelFloor;

        public static Vector3 cameraPosition;

        // initialize
        public void Init()
        {
            // initialize camera
            camera = new Camera(new Vector3(0, 3, 15));
            // initialize stopwatch
            timer = new Stopwatch();
            timer.Start();
            // create shaders
            shaderDefault = Shader.Load("vs", "fs");
            shaderNormal = Shader.Load("vs", "fs_normal");
            shaderFur = new FurShader("../../shaders/vs_fur.glsl", "../../shaders/fs_fur.glsl");
            shaderPostProc = Shader.Load("vs_post", "fs_post");
            shaderKernel = new PostKernelShader("../../shaders/vs_post.glsl", "../../shaders/fs_kernel.glsl");
            shaderVigAndChrom = new PostVigAndChromShader("../../shaders/vs_post.glsl", "../../shaders/fs_vigchrom.glsl");
            shaderPostBloomBlend = Shader.Load("vs_post", "fs_bloomblend");
            shaderSkybox = Shader.Load("vs_skybox", "fs_skybox");
            shaderReflective = Shader.Load("vs","fs_reflective");
            // load teapot
            meshTeapot = new Mesh("../../assets/teapot.obj");
            meshFloor = new Mesh("../../assets/floor.obj");
            meshCube = new Mesh("../../assets/cube.obj");
            // load a texture
            textureFur = Texture.Load("fur.png");
            textureWood = Texture.Load("wood.jpg");
            textureWall = Texture.Load("brick_normal_map.png");
            textureSkybox = new CubeTexture("../../assets/sea_rt.JPG",
                                     "../../assets/sea_lf.JPG",
                                     "../../assets/sea_up.JPG",
                                     "../../assets/sea_dn.JPG",
                                     "../../assets/sea_bk.JPG",
                                     "../../assets/sea_ft.JPG");

            Resize();
            quad = new ScreenQuad();
            scene = new SceneGraph();

            SceneNode mainNode = new SceneNode();
            modelTeapot = new Model(meshTeapot, textureWood, shaderDefault, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));

            modelFloor = new Model(meshFloor, textureWood, shaderNormal, Matrix4.Identity);
            modelFloor.MaterialColor = new Vector3(1f, 1f, 1f);
            modelFloor.NormalMap = textureWall;


            ReflectiveModel refl = new ReflectiveModel(meshTeapot,shaderReflective,Matrix4.CreateTranslation(0,10f,0),textureSkybox);
            mainNode.AddChildModel(modelTeapot);
            mainNode.AddChildModel(modelFloor);
            mainNode.AddChildModel(refl);

            scene.mainNode = mainNode;

            kernel = Kernel.SmallGaussianBlur;
        }

        public void Resize()
        {
            // create the render target
            targetMain = new RenderTarget(screen.width, screen.height);
            targetVigAndChrom = new RenderTarget(screen.width, screen.height);
            targetUnused = new RenderTarget(screen.width, screen.height);

            targetHDR = new RenderTarget(screen.width, screen.height, 2);
            targetBloom = new RenderTarget(screen.width, screen.height);
        }

        public void processKeyboard(KeyboardState keyboard)
        {
            // measure frame duration
            timer.Stop();
            float frameDuration = timer.ElapsedMilliseconds;
            timer.Restart();

            if (keyboard[Key.ShiftLeft] || keyboard[Key.ShiftRight])
                frameDuration *= 10f;

            if (keyboard[Key.Enter])
            {
                this.modelFloor.modeltransform *= Matrix4.CreateRotationX(0.1f);
            }

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

            cameraPosition = -camera.Position;


            // update rotation
            //camera.RotateYaw(0.001f * frameDuration);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            //GL.DepthMask(false);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (useRenderTarget)
            {

                //Bind the HDR target
                targetHDR.Bind();
                //Let the GPU now when want to render to 2 textures
                GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
                GL.Clear(ClearBufferMask.DepthBufferBit);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                //First skybox
                meshCube.SkyboxRender(shaderSkybox, Matrix4.Identity, transform, textureSkybox);
                //Second the rest of scene
                scene.Render(transform);
                //Unbind the HDR target
                GL.DrawBuffers(1, new DrawBuffersEnum[1] { DrawBuffersEnum.ColorAttachment0 });
                targetHDR.Unbind();

                targetBloom.Bind();
                quad.KernelRender(shaderKernel, targetHDR.GetTextureID(1), 640f, 400f, Kernel.Uniform(19, 19, 19));
                targetBloom.Unbind();

                //Merge bloomtarget and targethdr[0]
                targetMain.Bind();
                quad.BloomBlendRender(shaderPostBloomBlend, targetHDR.GetTextureID(0), targetBloom.GetTextureID());
                targetMain.Unbind();

                targetVigAndChrom.Bind();
                quad.VigAndChromRender(shaderVigAndChrom, targetMain.GetTextureID(), 2.3f, new Vector2(0.51f, 0.5f), 0.0125f * new Vector3(1f, 0f, -1f));
                targetVigAndChrom.Unbind();

                //quad.Render(postproc, target.GetTextureID());
                quad.KernelRender(shaderKernel, targetVigAndChrom.GetTextureID(), 640f, 400f, kernel);
            }
            else
            {
                // render scene directly to the screen
                scene.Render(transform);
            }
        }
    }

} // namespace Template_P3