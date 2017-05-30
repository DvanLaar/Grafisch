using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RayTracer.Lights;
using RayTracer.Primitives;
using Template;
using OpenTK.Input;
using System.Threading;
using System.Diagnostics;

namespace RayTracer
{
    /**
     * OpenGL uses this standard coordinate system, so we use this as well:
     * https://learnopengl.com/img/getting-started/coordinate_systems_right_handed.png
     */
    public class Raytracer : Camera.SpeedUpListener
    {
        private struct DrawParameters
        {
            public int SpeedUp, AntiAliasing, deltaX;
            public Surface surface;
            public float[] AAvals;
            public float AAInvSq;
        };

        public const int MAX_RECURSION = 8;
        private const bool jaccoPresent = true;

        private Camera camera;
        private Scene scene = new Scene();
        private Texture skybox;
        private int SpeedUp = 4, AntiAliasing = 1;
        private DrawParameters drawParams;
        private readonly int nThreads = Environment.ProcessorCount;

        public Vector3 DebugXUnit, DebugYUnit;

        public Raytracer()
        {
            camera = new Camera(this, new Vector3(0f, 1f, 0f));
            skybox = new Texture("Textures/skybox2.bmp");

            // Make the skybox use HDR:
            for (int i = 0; i < skybox.Width; i++)
                for (int j = 0; j < skybox.Height; j++)
                    skybox.Data[i, j] *= 1.2f;

            // white ball
            scene.AddPrimitive(new Sphere(new Vector3(0, 1.5f, -6f), 1.5f, new Material(Vector3.One, true)));
            // green ball
            scene.AddPrimitive(new Sphere(new Vector3(3f, 1.5f, -6f), 0.5f, new Material(new Vector3(0f, 1f, 0f), true)));
            // blue ball
            scene.AddPrimitive(new Sphere(new Vector3(-3f, 1.5f, -6f), 1, new Material(new Vector3(0f, 0f, 1f), 25f)));

            scene.AddPrimitive(new Quad(new Vector3(-5f, 0f, -10f), new Vector3(10f, 0f, 0f), new Vector3(0f, 10f, 0f), new Material(Vector3.One * 0.5f, true)));

            // normal is: Vector3.UnitY
            Texture floortexture = new Texture("Textures/floor.bmp");
            scene.AddPrimitive(new TexturedPlane(floortexture, new Vector3(.8f, 0f, -.6f), new Vector3(-.6f, 0f, -.8f), 0f, new Material(Vector3.One)));

            if (jaccoPresent)
            {
                Texture jbtexture = new Texture("Textures/jb.png");
                Vector3 jb_bl = new Vector3(2.5f, 3f, -5.4f), jb_dirx = new Vector3(2f, 0f, 1f), jb_diry = new Vector3(0f, 2f, 0f);

                scene.AddPrimitive(new TexturedQuad(
                    jb_bl, jb_dirx, jb_diry, jbtexture,
                    new Material(Vector3.One)
                ));
                scene.AddPrimitive(new TexturedSphere(
                    new Vector3(-3f, 2f, 3f),
                    1, jbtexture, new Material(Vector3.One, 25f)
                ));
            }

            // Texture pepetexture = new Texture("Textures/pepe.bmp");
            // scene.AddPrimitive(new TexturedQuad(new Vector3(-1f, 0f, -1f), new Vector3(2f, 0, 0), new Vector3(0, 2f, 0), pepetexture, new Material(Vector3.One)));

            // Slow, but awesome!
            // scene.AddPrimitive(new Mesh("Objects/decimated_teapot.obj", new Vector3(-0.5f, 4f, -2f), 1f, new Material(new Vector3(1f, 1f, 0f))));

            // Ambient
            // scene.AddLight(new Light(Utils.WHITE * 0.1f));

            scene.AddLight(new PointLight(new Vector3(-3f, 0.5f, -3f), Vector3.One * 10f));
            scene.AddLight(new PointLight(new Vector3(3.3f, 4.7f, -4f), Vector3.One * 1f));
            scene.AddLight(new DirectionalLight(new Vector3(-1f, -5f, -2.5f), Vector3.One * .5f));

            //Triangle arealighttriangle = new Triangle(new Vector3(-1f, .5f, -2f), new Vector3(2f, .5f, -2f), new Vector3(2f, 1.5f, -2f), Utils.WHITE, 1f);
            //scene.AddLight(new AreaLight(arealighttriangle, arealighttriangle.material.color * 10f));
            //scene.AddPrimitive(arealighttriangle);

            // scene.AddLight(new Spotlight(new Vector3(-2f, 2f, 0f), new Vector3(0f, -1f, 0f), (float)Math.PI / 3f, Utils.BLUE * 10f));
            // scene.AddLight(new Spotlight(new Vector3(3f, 3f, -3f), new Vector3(1f, -1f, 0f), (float)Math.PI / 3f, Utils.RED * 10f));
        }

        public void Render(Surface surface)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            // Console.Write("render; ");

            // Initial part of Debug
            DrawInitialDebug(surface);

            // When we talk about 4x anti-aliasing, we actually mean 2x2 rays instead of 1 per pixel.
            surface.Print("Anti-Aliasing: " + (AntiAliasing * AntiAliasing), 522, 512 - 48, 0xffffff);
            surface.Print("Speedup: " + SpeedUp, 522, 512 - 24, 0xffffff);

            int[] startX = new int[nThreads];
            for (int i = 0; i < nThreads; i++)
                startX[i] = Camera.resolution - (i + 1) * SpeedUp;

            // Update draw params:
            drawParams.SpeedUp = SpeedUp;
            drawParams.AntiAliasing = AntiAliasing;
            drawParams.deltaX = nThreads * SpeedUp;
            drawParams.AAInvSq = 1f / (AntiAliasing * AntiAliasing);
            drawParams.AAvals = new float[AntiAliasing];
            for (int i = 0; i < AntiAliasing; i++)
            {
                drawParams.AAvals[i] = SpeedUp * 0.5f * (1f + 2 * i) / AntiAliasing;
            }
            drawParams.surface = surface;

            Thread[] threads = new Thread[nThreads];
            for (int i = 1; i < nThreads; i++)
            {
                threads[i] = new Thread(DrawParallel);
                threads[i].Start(startX[i]);
            }
            DrawParallel(startX[0]);
            for (int i = 1; i < nThreads; i++)
                threads[i].Join();

            timer.Stop();
            //Console.WriteLine("One render took " + timer.ElapsedMilliseconds + " ms");
        }

        public void DrawParallel(object data)
        {
            // Cast rays in every direction determined by every pixel
            for (int x = (int)data; x >= 0; x -= drawParams.deltaX)
            {
                for (int y = Camera.resolution; (y -= drawParams.SpeedUp) >= 0;)
                {
                    Vector3 raysum = Vector3.Zero;
                    for (int aax = drawParams.AntiAliasing; aax-- > 0;)
                    {
                        for (int aay = drawParams.AntiAliasing; aay-- > 0;)
                        {
                            // ask the camera for the direction of this interpolated pixel
                            Ray ray = camera.getDirection(x + drawParams.AAvals[aax], y + drawParams.AAvals[aay]);
                            raysum += CalculateColor(ray);
                        }
                    }

                    // calculate the average color and cast it to an RGB int value
                    int color = Utils.GetRGBValue(drawParams.AAInvSq * raysum);
                    // plot this color on a square of size SpeedUp to reduce time
                    drawParams.surface.PlotRect(x, y, drawParams.SpeedUp, drawParams.SpeedUp, color);
                }
            }
        }

        private Vector3 CalculateColor(Ray ray, int recursionDepth = MAX_RECURSION)
        {
            Vector3 dir = ray.direction;
            if (recursionDepth-- <= 0) return Vector3.Zero;

            Intersection intersection = scene.Intersect(ray);
            if (intersection == null)
            {
                // The ray doesn't collide with any primitive, so return the color of the skybox
                int texx = Utils.scaleFloat((float)Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f, skybox.Height);
                int texy = (int)(Utils.SafeAcos(dir.Y) / Math.PI * (skybox.Height - 1));
                return skybox.Data[texx, texy];
            }

            // This is the real color of the object:
            if (intersection.primitive.material.isMirror)
            {
                // Calculate the reflection vector, and go one level deeper in the recursion
                Vector3 N = intersection.normal;
                // secondary ray, obtained by reflecting the current ray
                Ray ray2 = new Ray(intersection.location, dir - 2 * Vector3.Dot(dir, N) * N);
                // color at the intersection when looking in the reflection direction:
                Vector3 reflected = CalculateColor(ray2, recursionDepth);
                // multiply with the color of this material (in most cases this should be white for a realistic mirror)
                return intersection.primitive.GetDiffuseColor(intersection) * reflected;
            }
            
            Vector3 color = Vector3.Zero;
            foreach (Light light in scene.lights)
            {
                // The color is the sum of all the contributions
                color += light.GetIntensity(ray, intersection, scene);
            }
            return color;
        }

        private void DrawInitialDebug(Surface screen)
        {
            //Camera
            screen.Plot(TXDebug(0),TYDebug(0),0xffffff);


            DebugYUnit = camera.getDirection(256,256).direction.Normalized();
            Vector3 upesq = camera.getDirection(256, 200).direction.Normalized();
            DebugXUnit = (Vector3.Cross(DebugYUnit,upesq)).Normalized();

            //Screenplane
            //x = TXDebug(camera.cornerTL.X);
            //y = TYDebug(camera.cornerTL.Z);
            //int x2 = TXDebug(camera.cornerTR.X);
            //int y2 = TYDebug(camera.cornerTR.Z);

            //Let's just say it is on screen to save on Lots of &'s
            //screen.Line(x, y, x2, y2, 0xffffff);

            //primitives
            foreach (Primitive prim in scene.primitives)
            {
                if (prim is Sphere)
                {
                    Sphere s = (Sphere)prim;
                    Vector3 nc = s.center - camera.Position;
                    float nx = Vector3.Dot(DebugXUnit, nc);
                    float ny = Vector3.Dot(DebugYUnit, nc);
                    Vector3 dif = nc - (nx*DebugXUnit + ny* DebugYUnit);
                    float distancesquared = dif.LengthSquared;
                    if (distancesquared > s.radius * s.radius)
                        continue;

                    DrawCircle(screen, TXDebug(nx), TYDebug(ny), (float)Math.Sqrt(s.radius * s.radius - distancesquared), Utils.GetRGBValue(s.material.diffuse));
                }
            }
        }

        private void DrawRayDebug(Surface screen, Ray ray, Intersection intersection, int c, int nc)
        {
            int x1 = TXDebug(ray.origin.X);
            int y1 = TYDebug(ray.origin.Z);
            int x2 = TXDebug(intersection.location.X);
            int y2 = TYDebug(intersection.location.Z);
            screen.Line(x1, y1, x2, y2, c);

            Vector3 to = intersection.location + .3f * intersection.normal;
            x1 = TXDebug(to.X);
            y1 = TYDebug(to.Z);
            screen.Line(x1, y1, x2, y2, nc);
        }

        public float minX = -5, maxX = 5, minY = -1, maxY = 9;

        private int TXDebug(float x)
        {
            float worldWidth = maxX - minX;
            return 512 + (int)((x - minX) * (512 / worldWidth));
        }

        private int TYDebug(float y)
        {
            float worldHeight = maxY - minY;
            return (int)((-y + maxY) * (512 / worldHeight));
        }

        private void DrawCircle(Surface screen, int x, int y, float r, int c)
        {
            float xRange = maxX - minX;
            float yRange = maxY - minY;
            float stepsize = (float)Math.PI * 2f / 100f;
            int prevx = x + (int)(r * 512 / xRange);
            int prevy = y;
            for (int i = 1; i <= 100; i++)
            {
                int newx = x + (int)(r * 512 / xRange * Math.Cos(stepsize * i));
                int newy = y + (int)(r * 512 / yRange * Math.Sin(stepsize * i));
                screen.Line(prevx, prevy, newx, newy, c);
                prevx = newx;
                prevy = newy;
            }
        }

        public void processKeyboard(KeyboardState keyboard)
        {
            camera.processKeyboard(keyboard);
        }

        int Camera.SpeedUpListener.GetSpeedUp()
        {
            return SpeedUp;
        }

        void Camera.SpeedUpListener.SetSpeedUp(int value)
        {
            SpeedUp = value;
        }

        bool Camera.SpeedUpListener.IncreaseSpeedUp()
        {
            if (SpeedUp >= 512) return false;
            SpeedUp = Math.Min(512, SpeedUp << 1);
            Console.WriteLine("SpeedUp = " + SpeedUp);
            return true;
        }

        bool Camera.SpeedUpListener.DecreaseSpeedUp()
        {
            if (SpeedUp <= 1) return false;
            SpeedUp = Math.Max(1, SpeedUp >> 1);
            Console.WriteLine("SpeedUp = " + SpeedUp);
            return true;
        }

        bool Camera.SpeedUpListener.IncreaseAntiAliasing()
        {
            AntiAliasing++;
            return true;
        }

        bool Camera.SpeedUpListener.DecreaseAntiAliasing()
        {
            if (AntiAliasing == 1) return false;
            AntiAliasing--;
            return true;
        }
    }
}
