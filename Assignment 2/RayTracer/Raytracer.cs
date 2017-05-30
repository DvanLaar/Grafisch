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
    public class Raytracer
    {
        /// <summary>
        /// These values must stay constant during a draw.
        /// Therefore we copy these values to this object.
        /// </summary>
        private struct DrawParameters
        {
            public int SpeedUp, AntiAliasing, deltaX;
            public Surface surface;
            public float[] AAvals;
            public float AAInvSq;
        };

        // The cap on the recursion due to reflections.
        public const int MAX_RECURSION = 8;
        // Do we want jacco textures?
        private const bool jaccoPresent = true;

        private Camera camera;
        private Scene scene = new Scene();
        // Holds the texture of the skybox
        private Texture skybox;

        /// <summary>
        /// Big values of SpeedUp make the program faster,
        /// since then it won't calculate the color of all pixels, but creates blocks of size
        /// SpeedUp * SpeedUp which will have the same color.
        /// </summary>
        private int SpeedUp = 8;
        /// <summary>
        /// Anti-aliasing will create an easing-out effect for hard edges since it samples in between points of one pixel.
        /// </summary>
        private int AntiAliasing = 1;
        /// <summary>
        /// The parameters which need to keep constant during a render.
        /// </summary>
        private DrawParameters drawParams;
        /// <summary>
        /// The optimal number of threads which we will use in our multithreaded Render function.
        /// </summary>
        private readonly int nThreads = Environment.ProcessorCount;

        // Debug parameters
        public Surface screensurface;
        public Vector3 DebugXUnit, DebugYUnit;
        public Surface debugsurface;

        public Raytracer()
        {
            camera = new Camera(this, new Vector3(0f, 1f, 0f));
            skybox = new Texture("Textures/skybox2.bmp");

            // Make the skybox use HDR:
            for (int i = 0; i < skybox.Width; i++)
                for (int j = 0; j < skybox.Height; j++)
                    skybox.Data[i, j] *= 1.2f;

            // Now we will compose the scene with a couple of objects and a couple of light sources:

            // OBJECTS:

            // white ball
            scene.AddPrimitive(new Sphere(new Vector3(0, 1.5f, -6f), 1.5f, new Material(Vector3.One, .5f, 100f)));
            // green ball
            scene.AddPrimitive(new Sphere(new Vector3(3f, 1.5f, -6f), 0.5f, new Material(new Vector3(0f, 1f, 0f), .75f, 10f)));
            // blue ball
            scene.AddPrimitive(new Sphere(new Vector3(-3f, 1.5f, -6f), 1, new Material(new Vector3(0f, 0f, 1f), .5f, 25f)));

            scene.AddPrimitive(new Quad(new Vector3(-5f, 0f, -10f), new Vector3(10f, 0f, 0f), new Vector3(0f, 10f, 0f), new Material(Vector3.One * 0.5f, 1f)));

            // normal is: Vector3.UnitY
            Texture floortexture = new Texture("Textures/floor.bmp");
            scene.AddPrimitive(new TexturedPlane(floortexture, new Vector3(.8f, 0f, -.6f), new Vector3(-.6f, 0f, -.8f), 0f, new Material(Vector3.One)));

            if (jaccoPresent)
            {
                Texture jbtexture = new Texture("Textures/jb.png");

                scene.AddPrimitive(new TexturedQuad(
                    new Vector3(2.5f, 3f, -5.4f), new Vector3(2f, 0f, 1f), new Vector3(0f, 2f, 0f), jbtexture,
                    new Material(Vector3.One)
                ));
                scene.AddPrimitive(new TexturedSphere(
                    new Vector3(-3f, 2f, 3f),
                    1, jbtexture, new Material(Vector3.One, .5f, 25f)
                ));
            }

            // .obj file
            scene.AddPrimitive(new Mesh("Objects/little_test.obj", new Vector3(-4.5f, 1f, -4f), 1f, new Material(new Vector3(1f, 1f, 0f))));

            // LIGHT SOURCES:

            // Ambient
            scene.AddLight(new Light(Vector3.One * 0.1f));

            scene.AddLight(new PointLight(new Vector3(-3f, 2.5f, -3f), Vector3.One * 4f));
            scene.AddLight(new PointLight(new Vector3(3.3f, 4.7f, -4f), Vector3.One * 1f));
            scene.AddLight(new DirectionalLight(new Vector3(-1f, -5f, -2.5f), Vector3.One * .01f));

            Triangle arealighttriangle = new Triangle(new Vector3(-2.1f, 3f, 3f), new Vector3(-2f, 3f, 3f), new Vector3(-2f, 3f, 5f), new Material(Vector3.One));
            scene.AddLight(new AreaLight(arealighttriangle, arealighttriangle.material.diffuseColor * 5f));
            // scene.AddPrimitive(arealighttriangle);

            scene.AddLight(new Spotlight(new Vector3(-2f, 2f, 0f), new Vector3(0f, -1f, 0f), (float)Math.PI / 3f, Utils.BLUE * 10f));
            scene.AddLight(new Spotlight(new Vector3(3f, 3f, -3f), new Vector3(1f, -1f, 0f), (float)Math.PI / 3f, Utils.RED * 10f));
        }

        public void Render(Surface surface)
        {
            //Stopwatch timer = new Stopwatch();
            //timer.Start();

            // Initial part of Debug
            screensurface = surface;
            DrawInitialDebug(surface);

            // We divide the screen between threads by x-values.
            // In this fashion: [1, 2, 3, 1, 2, 3, 1, 2, 3, ..., 1, 2 ]
            // However we begin at the end (Camera.resolution - SpeedUp).
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
                // Precalculate these variables and put them in an array.
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
            // Wait for all OTHER threads until they are done.
            for (int i = 1; i < nThreads; i++)
                threads[i].Join();

            // The debug data is first drawn on a seperate surface, then drawn on the main surface so no debug data is drawn over the main raytracer image
            int[] debugdata = debugsurface.pixels;
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    surface.Plot(x + 512, y, debugdata[x + 512 * y]);
                }
            }

            // When we talk about 4x anti-aliasing, we actually mean 2x2 rays instead of 1 per pixel.
            surface.Print("Anti-Aliasing: " + (AntiAliasing * AntiAliasing), 522, 512 - 48, 0xffffff);
            surface.Print("Speedup: " + SpeedUp, 522, 512 - 24, 0xffffff);

            //timer.Stop();
            //Console.WriteLine("One render took " + timer.ElapsedMilliseconds + " ms");
        }

        public void DrawParallel(object data)
        {
            // Cast rays in every direction determined by every pixel
            for (int x = (int)data; x >= 0; x -= drawParams.deltaX)
            {
                for (int y = Camera.resolution; (y -= drawParams.SpeedUp) >= 0;)
                {
                    // Use anti-aliasing to get an average color over this block of SpeedUp x SpeedUp:
                    Vector3 raysum = Vector3.Zero;
                    for (int aax = drawParams.AntiAliasing; aax-- > 0;)
                    {
                        for (int aay = drawParams.AntiAliasing; aay-- > 0;)
                        {
                            // ask the camera for the direction of this interpolated pixel
                            Ray ray = camera.getDirection(x + drawParams.AAvals[aax], y + drawParams.AAvals[aay]);

                            // We are interested in a small subset of all pixels:
                            if (aax == 0 && aay == 0 && y <= 256 && 256 < y + SpeedUp && x % 10 == 0)
                            {
                                // Pass all the needed variables to draw the debug info
                                ray.debug = true;
                                ray.debugSurface = debugsurface;
                                ray.camerapos = camera.Position;
                                ray.debugxunit = DebugXUnit;
                                ray.debugyunit = DebugYUnit;
                            }

                            // Add the color to the sum.
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

        /// <summary>
        /// The core of the ray tracer.
        /// </summary>
        /// <param name="ray">The direction in which we look</param>
        /// <param name="recursionDepth">How far we can still go in recursion</param>
        /// <returns>The color associated with the ray</returns>
        private Vector3 CalculateColor(Ray ray, int recursionDepth = MAX_RECURSION)
        {
            if (recursionDepth-- <= 0) return Vector3.Zero; // recursion depth is reached

            // Check for an intersection:
            Intersection intersection = scene.Intersect(ray);

            if (ray.debug)
            {
                // Debug
                if (recursionDepth == MAX_RECURSION - 1)
                    DrawRayDebug(ray, intersection, 0xff0000); // primary ray
                else
                    DrawRayDebug(ray, intersection, 0x00ff00); // secondary, or higher ray
            }

            Vector3 dir = ray.direction;
            if (intersection == null)
            {
                // The ray doesn't collide with any primitive, so return the color of the skybox
                int texx = Utils.scaleFloat((float)Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f, skybox.Height);
                int texy = (int)(Utils.SafeAcos(dir.Y) / Math.PI * (skybox.Height - 1));
                return skybox.Data[texx, texy];
            }

            // Determine the diffuse and specular parts:
            Material mat = intersection.primitive.material;
            Vector3 ret = Vector3.Zero;

            if (mat.isSpecular)
            {
                // Calculate the reflection vector, and go one level deeper in the recursion
                Vector3 N = intersection.normal;
                // secondary ray, obtained by reflecting the current ray
                Ray ray2 = new Ray(intersection.location, dir - 2 * Vector3.Dot(dir, N) * N);

                // Pass on the same debug values:
                ray2.debug = ray.debug;
                ray2.debugSurface = ray.debugSurface;
                ray2.camerapos = ray.camerapos;
                ray2.debugxunit = ray.debugxunit;
                ray2.debugyunit = ray.debugyunit;

                // color at the intersection when looking in the reflection direction:
                Vector3 reflected = CalculateColor(ray2, recursionDepth);
                // multiply with the color of this material (in most cases this should be white for a realistic mirror)
                ret += mat.specularity * intersection.primitive.GetDiffuseColor(intersection) * reflected;
            }

            // Diffuse part:
            if (mat.isDiffuse)
            {
                foreach (Light light in scene.lights)
                {
                    // The color is the sum of all the contributions
                    ret += (1f - mat.specularity) * light.GetIntensity(ray, intersection, scene);
                }
            }
            return ret;
        }

        private void DrawInitialDebug(Surface screen)
        {
            //Create the surface to draw all the debugging on
            debugsurface = new Surface(512, 512);

            //Create a 2D basis based on the camera orientation to project the rays and spheres on
            DebugYUnit = camera.getDirection(256, 256).direction.Normalized();
            Vector3 upesq = camera.getDirection(256, 200).direction.Normalized();
            DebugXUnit = (Vector3.Cross(DebugYUnit, upesq)).Normalized();

            // draw primitive spheres
            foreach (Primitive prim in scene.primitives)
            {
                if (prim is Sphere)
                {
                    Sphere s = (Sphere)prim;
                    Vector3 nc = s.center - camera.Position;
                    float nx = Vector3.Dot(DebugXUnit, nc);
                    float ny = Vector3.Dot(DebugYUnit, nc);
                    Vector3 dif = nc - (nx * DebugXUnit + ny * DebugYUnit);
                    float distancesquared = dif.LengthSquared;
                    if (distancesquared > s.radius * s.radius)
                        continue; // outside the screen.

                    DrawCircle(debugsurface, TXDebug(nx), TYDebug(ny), (float)Math.Sqrt(s.radius * s.radius - distancesquared), Utils.GetRGBValue(s.material.diffuseColor));
                }
            }

            // Draw the camera as a point:
            debugsurface.Plot(TXDebug(0), TYDebug(0), 0xffffff);
        }

        /// <summary>
        /// Draws a ray on the debug screen.
        /// </summary>
        /// <param name="ray">The ray which contains all the debug options as well</param>
        /// <param name="intersection">The (optional) intersection which is the end to which we will draw a line</param>
        /// <param name="c">The color of the line</param>
        public static void DrawRayDebug(Ray ray, Intersection intersection, int c)
        {
            int x1 = TXDebug(Vector3.Dot(ray.debugxunit, ray.origin - ray.camerapos)), x2;
            int y1 = TYDebug(Vector3.Dot(ray.debugyunit, ray.origin - ray.camerapos)), y2;
            if (intersection != null)
            {
                x2 = TXDebug(Vector3.Dot(ray.debugxunit, intersection.location - ray.camerapos));
                y2 = TYDebug(Vector3.Dot(ray.debugyunit, intersection.location - ray.camerapos));
            }
            else
            {
                // Choose a big t value:
                x2 = TXDebug(Vector3.Dot(ray.debugxunit, (ray.origin + 100f * ray.direction) - ray.camerapos));
                y2 = TYDebug(Vector3.Dot(ray.debugyunit, (ray.origin + 100f * ray.direction) - ray.camerapos));
            }

            ray.debugSurface.Line(x1, y1, x2, y2, c);
        }

        // dimensions of the debug screen
        public static float minX = -5, maxX = 5, minY = -1, maxY = 9;

        private static int TXDebug(float x)
        {
            float worldWidth = maxX - minX;
            return (int)((x - minX) * (512 / worldWidth));
        }

        private static int TYDebug(float y)
        {
            float worldHeight = maxY - minY;
            return (int)((-y + maxY) * (512 / worldHeight));
        }

        /// Draw a circle on the debug screen.
        private void DrawCircle(Surface screen, int x, int y, float r, int c)
        {
            float xRange = maxX - minX;
            float yRange = maxY - minY;
            float stepsize = (float)Math.PI * 2f / 100f;
            int prevx = x + (int)(r * 512 / xRange);
            int prevy = y;
            // Approximate a circle by 100 points.
            for (int i = 1; i <= 100; i++)
            {
                int newx = x + (int)(r * 512 / xRange * Math.Cos(stepsize * i));
                int newy = y + (int)(r * 512 / yRange * Math.Sin(stepsize * i));
                screen.Line(prevx, prevy, newx, newy, c);
                prevx = newx;
                prevy = newy;
            }
        }

        public void processInput(KeyboardState keyboard, MouseDevice mouse)
        {
            // Pass on to the camera
            camera.processInput(keyboard, mouse);
        }

        public int GetSpeedUp()
        {
            return SpeedUp;
        }

        public void SetSpeedUp(int value)
        {
            SpeedUp = value;
        }

        public bool IncreaseSpeedUp()
        {
            if (SpeedUp >= 512) return false;
            SpeedUp = Math.Min(512, SpeedUp << 1);
            return true;
        }

        public bool DecreaseSpeedUp()
        {
            if (SpeedUp <= 1) return false;
            SpeedUp = Math.Max(1, SpeedUp >> 1);
            return true;
        }

        public bool IncreaseAntiAliasing()
        {
            AntiAliasing++;
            return true;
        }

        public bool DecreaseAntiAliasing()
        {
            if (AntiAliasing == 1) return false;
            AntiAliasing--;
            return true;
        }
    }
}
