using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using template_P3;
using System;
using System.Drawing;
using OpenTK.Input;
using System.Collections.Generic;

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
        public Mesh meshTeapot, meshFloor, meshCube, meshHeightMap;

        // used textures:
        public Texture textureWood, textureFur, textureBrickWall, textureTrump;
        public CubeTexture textureSkybox;
        public Texture normalBrickWall, normalNormal, normalHeightMap;

        // used render targets:
        public RenderTarget targetMain, targetVigAndChrom, targetUnused, targetHDR, targetBloom;

        // used models:
        public Model modelTeapot, modelFloor, modelLightPos, modelHeightMap;

        private static List<Vector3> lightPosition = new List<Vector3>(new Vector3[] {
            new Vector3(7f, 1f, 5f),
            new Vector3(-7f, 3f, 6f)
        });

        /// <summary>
        /// Is the index to the current movable light in the list 'lightPosition'.
        /// </summary>
        private static int lightIndex = 0;

        public static Vector3 cameraPosition;

        public SceneNode subNode1;

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
            shaderNormal = Shader.Load("vs_normal", "fs_normal");
            shaderFur = new FurShader("../../shaders/vs_fur.glsl", "../../shaders/fs_fur.glsl");
            shaderPostProc = Shader.Load("vs_post", "fs_post");
            shaderKernel = new PostKernelShader("../../shaders/vs_post.glsl", "../../shaders/fs_kernel.glsl");
            shaderVigAndChrom = new PostVigAndChromShader("../../shaders/vs_post.glsl", "../../shaders/fs_vigchrom.glsl");
            shaderPostBloomBlend = Shader.Load("vs_post", "fs_bloomblend");
            shaderSkybox = Shader.Load("vs_skybox", "fs_skybox");
            shaderReflective = Shader.Load("vs", "fs_reflective");

            // load teapot
            meshTeapot = new Mesh("../../assets/teapot.obj");
            meshFloor = new Mesh("../../assets/floor.obj");
            meshCube = new Mesh("../../assets/cube.obj");
            meshHeightMap = new HeightMap("../../assets/heightmap.png");

            // load a texture
            textureFur = Texture.Load("fur.png");
            textureWood = Texture.Load("wood.jpg");
            textureTrump = Texture.Load("thetrump.png");
            textureBrickWall = Texture.Load("brickwall.jpg");
            textureSkybox = new CubeTexture(
                "../../assets/sea_rt.JPG", "../../assets/sea_lf.JPG",
                "../../assets/sea_up.JPG", "../../assets/sea_dn.JPG",
                "../../assets/sea_bk.JPG", "../../assets/sea_ft.JPG"
            );

            normalNormal = Texture.Load("normal_normal.png");
            normalBrickWall = Texture.Load("brickwall_normal.jpg");
            normalHeightMap = Texture.Load("heightmap_normal.png");

            Resize();
            quad = new ScreenQuad();

            modelTeapot = new Model(meshTeapot, textureWood, shaderNormal, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));
            modelFloor = new Model(meshFloor, textureBrickWall, shaderNormal, Matrix4.Identity);
            modelLightPos = new Model(meshCube, null, shaderDefault, Matrix4.Identity);
            modelHeightMap = new Model(meshHeightMap, textureTrump, shaderDefault, Matrix4.CreateScale(10f) * Matrix4.CreateTranslation(20f, 0f, 0f));

            // modelFloor.MaterialColor = new Vector3(1f, 1f, 1f);
            modelFloor.NormalMap = normalBrickWall;
            modelTeapot.NormalMap = normalBrickWall;
            modelHeightMap.NormalMap = normalHeightMap;

            ReflectiveModel refl = new ReflectiveModel(meshTeapot, shaderReflective, Matrix4.CreateTranslation(0, 20f, 0), textureSkybox);

            FurModel furmod = new FurModel(meshTeapot, textureBrickWall, textureFur, shaderDefault, shaderFur, Matrix4.CreateRotationX((float)Math.PI / 2f) * Matrix4.CreateTranslation(new Vector3(0, 40f, 0)));
            Model teapot2 = new Model(meshTeapot, textureWood, shaderDefault, Matrix4.CreateRotationY(1.5f) * Matrix4.CreateTranslation(new Vector3(0, 60f, 0)));

            SceneNode mainNode = new SceneNode();

            subNode1 = new SceneNode();
            subNode1.AddChildModel(refl);
            subNode1.AddChildModel(furmod);
            subNode1.AddChildModel(teapot2);

            mainNode.AddChildNode(subNode1);

            mainNode.AddChildModel(modelFloor);
            mainNode.AddChildModel(modelTeapot);
            mainNode.AddChildModel(modelLightPos);
            mainNode.AddChildModel(modelHeightMap);
            scene = new SceneGraph(mainNode);

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

        private KeyboardState lKeyboard;

        public void processKeyboard(KeyboardState keyboard)
        {
            // measure frame duration
            timer.Stop();
            float frameDuration = timer.ElapsedMilliseconds;
            timer.Restart();

            if (keyboard[Key.ShiftLeft] || keyboard[Key.ShiftRight])
                frameDuration *= 10f;


            bool f1 = keyboard[Key.BackSlash] && !lKeyboard[Key.BackSlash];
            bool f2 = keyboard[Key.Enter] && !lKeyboard[Key.Enter];
            if (f1 || f2) {
                if (f1) MeshLoader.divideByDet = !MeshLoader.divideByDet;
                if (f2) MeshLoader.averageTangents = !MeshLoader.averageTangents;
                Console.WriteLine("AVERAGE TANGENTS: " + MeshLoader.averageTangents);
                Console.WriteLine("DIVIDE BY DET: " + MeshLoader.divideByDet);
                modelTeapot.mesh = new Mesh("../../assets/teapot.obj");
                // modelTeapot = new Model(meshTeapot, textureWood, shaderNormal, Matrix4.CreateTranslation(new Vector3(0, 0.1f, 0)));
                // modelTeapot.NormalMap = normalBrickWall;
            }

            if (keyboard[Key.O]) modelFloor.shader = modelHeightMap.shader = modelTeapot.shader = shaderDefault;
            if (keyboard[Key.P]) modelFloor.shader = modelHeightMap.shader = modelTeapot.shader = shaderNormal;

            if (keyboard[Key.Insert] && !lKeyboard[Key.Insert])
            {
                lightIndex = lightPosition.Count;
                lightPosition.Add(Vector3.Zero);
            }
            if (keyboard[Key.Delete] && !lKeyboard[Key.Delete] && lightPosition.Count > 0)
            {
                lightPosition.RemoveAt(lightIndex);
                if (lightIndex == lightPosition.Count) lightIndex = 0;
            }

            if (keyboard[Key.Comma] && !lKeyboard[Key.Comma])
                lightIndex = (lightIndex == 0 ? lightPosition.Count : lightIndex) - 1;
            if (keyboard[Key.Period] && !lKeyboard[Key.Period])
                if (++lightIndex == lightPosition.Count) lightIndex = 0;

            float speed = 0.0075f;
            Vector3 boxTranslation = -1e9f * Vector3.UnitZ;
            if (lightPosition.Count > 0)
            {
                if (keyboard[Key.L]) lightPosition[lightIndex] += speed * frameDuration * Vector3.UnitX;
                if (keyboard[Key.H]) lightPosition[lightIndex] -= speed * frameDuration * Vector3.UnitX;
                if (keyboard[Key.N]) lightPosition[lightIndex] += speed * frameDuration * Vector3.UnitY;
                if (keyboard[Key.M]) lightPosition[lightIndex] -= speed * frameDuration * Vector3.UnitY;
                if (keyboard[Key.J]) lightPosition[lightIndex] += speed * frameDuration * Vector3.UnitZ;
                if (keyboard[Key.K]) lightPosition[lightIndex] -= speed * frameDuration * Vector3.UnitZ;
                boxTranslation = lightPosition[lightIndex];
            }
            modelLightPos.meshToModel = Matrix4.CreateTranslation(boxTranslation);

            // rotation.X : left/right
            // rotation.Y : up/down
            Vector2 rotation = Vector2.Zero;
            Vector3 translation = Vector3.Zero;

            // rotate the camera:
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

            // apply the transformation
            camera.AddTransformation(0.004f * frameDuration * rotation, 0.03f * frameDuration * translation);

            // rotate the subnode:
            rotation = Vector2.Zero;
            translation = Vector3.Zero;

            if (keyboard[Key.Number1]) translation += Vector3.UnitY;
            if (keyboard[Key.Number2]) translation -= Vector3.UnitY;
            if (keyboard[Key.Number3]) rotation += Vector2.UnitX;
            if (keyboard[Key.Number4]) rotation -= Vector2.UnitX;

            subNode1.Transform = Matrix4.CreateRotationX(0.03f * rotation.X) * Matrix4.CreateTranslation(0.004f * frameDuration * translation) * subNode1.Transform;



            lKeyboard = keyboard;
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

            cameraPosition = camera.Position;
            // Console.WriteLine(cameraPosition);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            // GL.DepthMask(false);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (useRenderTarget)
            {
                // Bind the HDR target
                targetHDR.Bind();
                // Let the GPU now when want to render to 2 textures
                GL.DrawBuffers(2, new DrawBuffersEnum[2] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

                // First skybox
                meshCube.SkyboxRender(shaderSkybox, Matrix4.Identity, transform, textureSkybox);
                // Second the rest of scene
                scene.Render(transform);
                // Unbind the HDR target
                GL.DrawBuffers(1, new DrawBuffersEnum[1] { DrawBuffersEnum.ColorAttachment0 });
                targetHDR.Unbind();

                targetBloom.Bind();
                quad.KernelRender(shaderKernel, targetHDR.GetTextureID(1), 640f, 400f, Kernel.Uniform(19, 19, 19));
                targetBloom.Unbind();

                // Merge bloomtarget and targethdr[0]
                targetMain.Bind();
                quad.BloomBlendRender(shaderPostBloomBlend, targetHDR.GetTextureID(0), targetBloom.GetTextureID());
                targetMain.Unbind();

                targetVigAndChrom.Bind();
                quad.VigAndChromRender(shaderVigAndChrom, targetMain.GetTextureID(), 2.3f, new Vector2(0.51f, 0.5f), 0.0125f * new Vector3(1f, 0f, -1f));
                targetVigAndChrom.Unbind();

                quad.KernelRender(shaderKernel, targetVigAndChrom.GetTextureID(), 640f, 400f, kernel);
            }
            else
            {
                // render scene directly to the screen
                scene.Render(transform);
            }
        }

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
    }
} // namespace Template_P3
