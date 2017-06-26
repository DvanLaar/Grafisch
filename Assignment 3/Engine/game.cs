using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using OpenTK.Input;
using System.Collections.Generic;

/// minimal OpenTK rendering framework for UU/INFOGR
/// Jacco Bikker, 2016
namespace rasterizer
{
    class Game
    {
        // constants:
        const float PI = 3.1415926535f;
        const bool USE_RENDER_TARGET = true;

        // member variables:
        public Surface screen; // background surface for printing etc.
        private Camera camera;
        private Stopwatch timer; // timer for measuring frame duration
        private ScreenQuad quad; // screen filling quad for post processing

        private SceneGraph scene;
        public SceneNode subScene;

        /// <summary>
        /// used shaders
        /// </summary>        
        private Shader shaderDefault, shaderNormal, shaderConstant, shaderPostProc, shaderPostBloomBlend, shaderSkyBox, shaderReflective;
        private FurShader shaderFur;
        private PostVigAndChromShader shaderVigAndChrom;
        private PostKernelShader shaderKernel;
        private Kernel kernel;

        /// <summary>
        /// used meshes
        /// </summary>
        private Mesh meshTeapot, meshFloor, meshCube, meshHeightMap;

        /// <summary>
        /// used textures
        /// </summary>
        private Texture textureWood, textureFur, textureBrickWall, textureTrump;
        private CubeTexture textureSkyBox;
        private Texture normalBrickWall, normalNormal, normalHeightMap;

        /// <summary>
        /// used render targets
        /// </summary>
        private RenderTarget targetMain, targetVigAndChrom, targetUnused, targetHDR, targetBloom;

        /// <summary>
        /// used models
        /// </summary>
        private Model modelTeapot, modelFloor, modelLightPos, modelHeightMap;

        /// <summary>
        /// Contains all positions of the point lights
        /// </summary>
        private static List<Vector3> lightPosition = new List<Vector3>(new Vector3[] {
            new Vector3(7f, 10f, 5f),
            new Vector3(-7f, 3f, 6f),
            new Vector3(-7f, 30f, 7f),
            new Vector3(15f, 30f, 7f)
        });

        /// <summary>
        /// Contains all intensities of the point lights
        /// </summary>
        private static List<Vector3> lightIntensity = new List<Vector3>(new Vector3[] {
            new Vector3(100f, 100f, 100f),
            new Vector3(0f, 100f, 0f),
            new Vector3(100f, 0f, 0f),
            new Vector3(0f, 0f, 1000f)
        });

        /// <summary>
        /// Is the index to the current movable light in the list 'lightPosition'.
        /// </summary>
        private static int lightIndex = 0;

        /// <summary>
        /// Static reference to the position of the camera, used in the rendering...
        /// </summary>
        public static Vector3 cameraPosition;

        // initialize
        public void Init()
        {

            // initialize camera
            camera = new Camera(new Vector3(0, 10, 15), Quaternion.FromAxisAngle(Vector3.UnitX, -0.5f));

            // initialize stopwatch
            timer = new Stopwatch();
            timer.Start();

            // create model shaders
            shaderDefault = Shader.Load("vs", "fs");
            shaderNormal = Shader.Load("vs_normal", "fs_normal");
            shaderConstant = Shader.Load("vs", "fs_const");
            shaderFur = new FurShader("../../shaders/vs_fur.glsl", "../../shaders/fs_fur.glsl");
            shaderSkyBox = Shader.Load("vs_skybox", "fs_skybox");
            shaderReflective = Shader.Load("vs", "fs_reflective");
            // create post processing shaders
            shaderPostProc = Shader.Load("vs_post", "fs_post");
            shaderKernel = new PostKernelShader("../../shaders/vs_post.glsl", "../../shaders/fs_kernel.glsl");
            shaderVigAndChrom = new PostVigAndChromShader("../../shaders/vs_post.glsl", "../../shaders/fs_vigchrom.glsl");
            shaderPostBloomBlend = Shader.Load("vs_post", "fs_bloomblend");

            // load meshes
            meshTeapot = new Mesh("../../assets/teapot.obj");
            meshFloor = new Mesh("../../assets/floor.obj");
            meshCube = new Mesh("../../assets/cube.obj");
            meshHeightMap = new HeightMap("../../assets/heightmap.png");

            // load textures
            textureFur = Texture.Load("fur.png");
            textureWood = Texture.Load("wood.jpg");
            textureTrump = Texture.Load("thetrump.png");
            textureBrickWall = Texture.Load("brickwall.jpg");
            textureSkyBox = new CubeTexture(
                "../../assets/sea_rt.JPG", "../../assets/sea_lf.JPG",
                "../../assets/sea_up.JPG", "../../assets/sea_dn.JPG",
                "../../assets/sea_bk.JPG", "../../assets/sea_ft.JPG"
            );
            // load normal maps
            normalNormal = Texture.Load("normal_normal.png");
            normalBrickWall = Texture.Load("brickwall_normal.jpg");
            normalHeightMap = Texture.Load("heightmap_normal.png");

            ResizeWindow();
            quad = new ScreenQuad();

            // create models
            modelTeapot = new Model(meshTeapot, textureWood, shaderNormal, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));
            modelFloor = new Model(meshFloor, textureBrickWall, shaderNormal, Matrix4.Identity);
            modelLightPos = new Model(meshCube, null, shaderConstant, Matrix4.Identity);
            modelHeightMap = new Model(meshHeightMap, textureTrump, shaderDefault, Matrix4.CreateScale(10f) * Matrix4.CreateTranslation(20f, 0f, 0f));
            Model teapot2 = new Model(meshTeapot, textureWood, shaderDefault, Matrix4.CreateScale(0.5f, 1f, 0.5f) * Matrix4.CreateRotationY(1.5f) * Matrix4.CreateTranslation(new Vector3(0, 24f, 0)));

            // set normal maps of specific models
            modelFloor.normalMap = normalBrickWall;
            modelTeapot.normalMap = normalBrickWall;
            modelLightPos.color = new Vector3(1f, 1f, .5f);
            modelHeightMap.normalMap = normalHeightMap;

            // create special models
            ReflectiveModel refl = new ReflectiveModel(meshTeapot, shaderReflective, Matrix4.CreateTranslation(0, 12f, 0), textureSkyBox);
            FurModel furmod = new FurModel(meshTeapot, textureBrickWall, textureFur, shaderDefault, shaderFur, Matrix4.CreateRotationX((float)Math.PI / 2f) * Matrix4.CreateTranslation(new Vector3(0, 36f, 0)));

            // set up scenegraph
            SceneNode mainNode = new SceneNode();

            subScene = new SceneNode();
            subScene.AddChildModel(refl);
            subScene.AddChildModel(furmod);
            subScene.AddChildModel(teapot2);

            mainNode.AddChildNode(subScene);

            mainNode.AddChildModel(modelFloor);
            mainNode.AddChildModel(modelTeapot);
            mainNode.AddChildModel(modelLightPos);
            mainNode.AddChildModel(modelHeightMap);
            scene = new SceneGraph(mainNode);

            // set kernel used for final post processing step
            kernel = Kernel.SmallGaussianBlur;
        }

        public void ResizeWindow()
        {
            // create the render target
            targetMain = new RenderTarget(screen.width, screen.height);
            targetVigAndChrom = new RenderTarget(screen.width, screen.height);
            targetUnused = new RenderTarget(screen.width, screen.height);

            targetHDR = new RenderTarget(screen.width, screen.height, 2);
            targetBloom = new RenderTarget(screen.width, screen.height);
        }

        private KeyboardState lastKeyboard;

        public void processKeyboard(KeyboardState keyboard)
        {
            // measure frame duration
            timer.Stop();
            float frameDuration = timer.ElapsedMilliseconds;
            timer.Restart();

            // speedup
            if (keyboard[Key.ShiftLeft] || keyboard[Key.ShiftRight])
                frameDuration *= 10f;

            // enable/disable normalmapping
            if (keyboard[Key.O]) modelFloor.shader = modelHeightMap.shader = modelTeapot.shader = shaderDefault;
            if (keyboard[Key.P]) modelFloor.shader = modelHeightMap.shader = modelTeapot.shader = shaderNormal;

            // switch final post processing kernel
            if (keyboard[Key.Z]) kernel = Kernel.SmallGaussianBlur;
            if (keyboard[Key.X]) kernel = Kernel.SobelHorizontal;

            if (keyboard[Key.Tab] && !lastKeyboard[Key.Tab])
            {
                // add a new light to the scene
                lightIndex = lightPosition.Count;
                lightPosition.Add(Vector3.Zero);
                lightIntensity.Add(new Vector3(100, 100, 100));
            }
            if (keyboard[Key.Delete] && !lastKeyboard[Key.Delete] && lightPosition.Count > 0)
            {
                // remove a light from the scene
                lightPosition.RemoveAt(lightIndex);
                if (lightIndex == lightPosition.Count) lightIndex = 0;
            }

            // move to the next light
            if (keyboard[Key.Comma] && !lastKeyboard[Key.Comma])
                lightIndex = (lightIndex == 0 ? lightPosition.Count : lightIndex) - 1;
            // move to the previous light
            if (keyboard[Key.Period] && !lastKeyboard[Key.Period])
                if (++lightIndex == lightPosition.Count) lightIndex = 0;

            float speed = 0.0075f;
            Vector3 boxTranslation = -1e9f * Vector3.UnitZ;
            if (lightPosition.Count > 0)
            {
                // move lightposition of the currently selected light.
                if (keyboard[Key.L]) lightPosition[lightIndex] += speed * frameDuration * Vector3.UnitX;
                if (keyboard[Key.H]) lightPosition[lightIndex] -= speed * frameDuration * Vector3.UnitX;
                if (keyboard[Key.N]) lightPosition[lightIndex] += speed * frameDuration * Vector3.UnitY;
                if (keyboard[Key.M]) lightPosition[lightIndex] -= speed * frameDuration * Vector3.UnitY;
                if (keyboard[Key.J]) lightPosition[lightIndex] += speed * frameDuration * Vector3.UnitZ;
                if (keyboard[Key.K]) lightPosition[lightIndex] -= speed * frameDuration * Vector3.UnitZ;
                boxTranslation = lightPosition[lightIndex];
            }
            modelLightPos.meshToModel = Matrix4.CreateScale(0.5f) * Matrix4.CreateTranslation(boxTranslation);

            // camera movements
            // rotation.X : left/right
            // rotation.Y : up/down
            Vector2 rotation = Vector2.Zero;
            Vector3 translation = Vector3.Zero;

            // rotate the camera:
            if (keyboard[Key.Up]) rotation += Vector2.UnitY;
            if (keyboard[Key.Down]) rotation -= Vector2.UnitY;
            if (keyboard[Key.Left]) rotation += Vector2.UnitX;
            if (keyboard[Key.Right]) rotation -= Vector2.UnitX;

            // translate the camera:
            if (keyboard[Key.S]) translation += Vector3.UnitZ;
            if (keyboard[Key.W]) translation -= Vector3.UnitZ;
            if (keyboard[Key.E]) translation += Vector3.UnitY;
            if (keyboard[Key.Q]) translation -= Vector3.UnitY;
            if (keyboard[Key.D]) translation += Vector3.UnitX;
            if (keyboard[Key.A]) translation -= Vector3.UnitX;

            // apply the transformation:
            camera.AddTransformation(0.002f * frameDuration * rotation, 0.03f * frameDuration * translation);

            // subnode movements to show working scenegraph
            rotation = Vector2.Zero;
            translation = Vector3.Zero;

            // translate the subscene
            if (keyboard[Key.Number1]) translation += Vector3.UnitY;
            if (keyboard[Key.Number2]) translation -= Vector3.UnitY;
            // rotate the subscene
            if (keyboard[Key.Number3]) rotation += Vector2.UnitX;
            if (keyboard[Key.Number4]) rotation -= Vector2.UnitX;

            // apply the transformation
            subScene.Transformation = Matrix4.CreateTranslation(0.004f * frameDuration * translation) * Matrix4.CreateRotationX(0.05f * rotation.X) * subScene.Transformation;

            // set this keyboard as the last, so we can record changes in key presses.
            lastKeyboard = keyboard;
        }

        /// <summary>
        /// tick for background surface
        /// </summary>
        public void Tick()
        {
            screen.Clear(0);
            screen.Print("hello world", 2, 2, 0xffff00);
        }

        /// <summary>
        /// tick for OpenGL rendering code
        /// </summary>
        public void RenderGL()
        {
            // prepare matrix for vertex shader
            Matrix4 transform = camera.Matrix;
            cameraPosition = camera.Position;

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            // GL.DepthMask(false);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (USE_RENDER_TARGET)
            {
                // Bind the HDR target
                targetHDR.Bind();
                // Let the GPU know when we want to render to two textures
                GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

                // First, the skybox
                meshCube.RenderSkyBox(shaderSkyBox, Matrix4.Identity, transform, textureSkyBox);
                // Second, the rest of scene
                scene.Render(transform);
                // Now unbind the HDR target
                GL.DrawBuffers(1, new DrawBuffersEnum[1] { DrawBuffersEnum.ColorAttachment0 });
                targetHDR.Unbind();

                // Blur the HDR
                targetBloom.Bind();
                quad.RenderKernel(shaderKernel, targetHDR.GetTextureID(1), 640f, 400f, Kernel.Uniform(19, 19, 19));
                targetBloom.Unbind();

                // Merge bloomtarget and "normal" scene
                targetMain.Bind();
                quad.RenderBloomBlend(shaderPostBloomBlend, targetHDR.GetTextureID(0), targetBloom.GetTextureID());
                targetMain.Unbind();

                // Apply some vignetting and chromatic aberation :).
                targetVigAndChrom.Bind();
                quad.RenderVigAndChrom(shaderVigAndChrom, targetMain.GetTextureID(), 2.3f, new Vector2(0.51f, 0.5f), 0.0125f * new Vector3(1f, 0f, -1f));
                targetVigAndChrom.Unbind();

                // Render to screen with final postprocessing kernel
                quad.RenderKernel(shaderKernel, targetVigAndChrom.GetTextureID(), 640f, 400f, kernel);
            }
            else
            {
                // Render the scene directly to the screen and make it look not so nice as with the code from above 8).
                scene.Render(transform);
            }
        }

        /// <summary>
        /// Returns the raw data of all the light position vectors.
        /// </summary>
        /// <returns>A float array with for every 3 floats, the X, Y and Z coordinate of one light point.</returns>
        public static float[] GetLightPositions()
        {
            int nlights = lightPosition.Count;
            float[] values = new float[3 * nlights];
            for (int i = nlights; i-- > 0;)
            {
                values[3 * i + 0] = lightPosition[i].X;
                values[3 * i + 1] = lightPosition[i].Y;
                values[3 * i + 2] = lightPosition[i].Z;
            }
            return values;
        }

        /// <summary>
        /// Returns the raw data of all the light Intensity vectors.
        /// </summary>
        /// <returns>A float array with for every 3 floats, the R, G and B intensity of one light point.</returns>
        public static float[] GetLightIntensity()
        {
            int nlights = lightIntensity.Count;
            float[] values = new float[3 * nlights];
            for (int i = nlights; i-- > 0;)
            {
                values[3 * i + 0] = lightIntensity[i].X;
                values[3 * i + 1] = lightIntensity[i].Y;
                values[3 * i + 2] = lightIntensity[i].Z;
            }
            return values;
        }
    }
} // namespace Template_P3
