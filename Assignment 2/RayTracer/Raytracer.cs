﻿using OpenTK;
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
        private int SpeedUp = 8, AntiAliasing = 1;
        private DrawParameters drawParams;
        private readonly int nThreads = Environment.ProcessorCount;

        public Raytracer()
        {
            camera = new Camera(this, new Vector3(0f, 1f, 0f));
            skybox = new Texture("Textures/skybox2.bmp");
            // Make the skybox use HDR:
            for (int i = 0; i < skybox.Width; i++)
                for (int j = 0; j < skybox.Height; j++)
                    skybox.Data[i, j] *= 1.2f;

            // white ball
            scene.AddPrimitive(new Sphere(new Vector3(0, 1.5f, -6f), 1.5f, Utils.WHITE, .5f));
            // green ball
            scene.AddPrimitive(new Sphere(new Vector3(3f, 1.5f, -6f), 0.5f, new Vector3(0f, 1f, 0f), 0.9f));
            // blue ball
            scene.AddPrimitive(new Sphere(new Vector3(-3f, 1.5f, -6f), 1, new Vector3(0f, 0f, 1f), 0.1f));

            // scene.AddPrimitive(new Quad(new Vector3(-5f, 0f, -10f), new Vector3(10f, 0f, 0f), new Vector3(0f, 10f, 0f), Vector3.One * 0.5f, 0.5f));

            // normal is: Vector3.UnitY
            //Texture floortexture = new Texture("Textures/floor.bmp");
            //scene.AddPrimitive(new TexturedPlane(floortexture, new Vector3(.8f, 0f, -.6f), new Vector3(-.6f, 0f, -.8f), 0f, Utils.WHITE, 1f));

            if (jaccoPresent)
            {
                Texture jbtexture = new Texture("Textures/jb.png");
                Vector3 jb_bl = new Vector3(1.9f, 0f, -5.4f), jb_dirx = new Vector3(2f, 0f, 1f), jb_diry = new Vector3(0f, 2f, 0f);

                scene.AddPrimitive(new TexturedQuad(
                    jb_bl, jb_dirx, jb_diry, jbtexture,
                    new Vector2(0f, 1f), new Vector2(1f, 0f), new Vector2(0f, -1f),
                    Utils.WHITE, 1f
                ));
                scene.AddPrimitive(new TexturedSphere(
                    new Vector3(-3f, 2f, 3f),
                    1, Utils.WHITE, jbtexture, 1f));
            }

            // Texture pepetexture = new Texture("Textures/pepe.bmp");
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(1f, 0f, -1f), new Vector3(-1f, 0f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), Utils.WHITE, 1f));
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(-1f, 0f, -1f), new Vector3(-1f, -2f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), Utils.WHITE, 1f));

            // Slow, but awesome!
            // scene.AddPrimitive(new Mesh("Objects/decimated_teapot.obj", new Vector3(1f, 1f, 0f), new Vector3(-0.5f, 4f, -2f), .5f, 1f));

            //Ambient
            // scene.AddLight(new Light(Utils.WHITE * 0.1f));
            scene.AddLight(new DirectionalLight(new Vector3(-1f, -1f, -1f), Utils.WHITE));

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
            // DrawInitialDebug(screen);

            // When we talk about 4x anti-aliasing, we actually mean 2x2 rays instead of 1 per pixel.
            int AAsq = AntiAliasing * AntiAliasing;

            surface.Print("Anti-Aliasing: " + AAsq, 10, 512 + 6, 0xffffff);
            surface.Print("Speedup: " + this.SpeedUp, 10, 512 + 30, 0xffffff);

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
            Console.WriteLine("One render took " + timer.ElapsedMilliseconds + " ms");
        }

        public void DrawParallel(object data)
        {
            int startX = (int)data;
            // Cast rays
            for (int x = startX; x >= 0; x -= drawParams.deltaX)
            {
                for (int y = Camera.resolution; (y -= drawParams.SpeedUp) >= 0;)
                {
                    Vector3 raysum = Vector3.Zero;
                    for (int aax = drawParams.AntiAliasing; aax-- > 0;)
                    {
                        for (int aay = drawParams.AntiAliasing; aay-- > 0;)
                        {
                            Ray ray = camera.getDirection(x + drawParams.AAvals[aax], y + drawParams.AAvals[aay]);
                            raysum += CalculateColor(ray);
                        }
                    }

                    int color = Utils.GetRGBValue(drawParams.AAInvSq * raysum);
                    drawParams.surface.PlotRect(x, y, drawParams.SpeedUp, drawParams.SpeedUp, color);
                }
            }
        }

        private Vector3 CalculateColor(Ray ray, int recursionDepth = MAX_RECURSION)
        {
            if (recursionDepth-- <= 0) return Vector3.Zero;

            Intersection intersection = scene.Intersect(ray);
            if (intersection != null)
            {
                Vector3 color = intersection.primitive.GetColor(intersection);
                Vector3 diffusepart = Vector3.Zero, specularpart = Vector3.Zero;
                float diffuse = intersection.primitive.material.diffuse;
                if (diffuse > Utils.SMALL_EPS)
                {
                    foreach (Light light in scene.lights)
                        diffusepart += light.GetIntensity(ray, intersection, scene);
                }
                if (diffuse < 1 - Utils.SMALL_EPS)
                {
                    Vector3 normal = intersection.normal;
                    Ray secondaryRay = new Ray(intersection.location, ray.direction - 2 * Vector3.Dot(ray.direction, normal) * normal);
                    specularpart = CalculateColor(secondaryRay, recursionDepth);
                }
                return color * (diffuse * diffusepart + (1f - diffuse) * specularpart);
            }
            else
            {
                // The ray doesn't collide with any primitive, so return the color of the skybox
                Vector3 dir = ray.direction;
                int texx = Utils.scaleFloat((float)Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f, skybox.Height);
                // int texx = MathHelper.Clamp((int)((Math.Atan2(dir.Z, dir.X) / MathHelper.TwoPi + 0.5f) * (skybox.Width - 1)), 0, skybox.Width - 1);
                int texy = (int)(Utils.SafeAcos(dir.Y) / Math.PI * (skybox.Height - 1));
                // return new Vector3(1f * texx / skybox.Height, 1f * texy / skybox.Width, 0f);
                return skybox.Data[texx, texy];
            }
        }

        private void DrawInitialDebug(Surface screen)
        {
            //Camera
            int x = TXDebug(camera.Position.X);
            int y = TYDebug(camera.Position.Z);
            if ((x >= 512) && (x < 1024) && (y >= 0) && (y < 512))
                screen.Plot(x, y, 0xffffff);
            //Screenplane
            x = TXDebug(camera.cornerTL.X);
            y = TYDebug(camera.cornerTL.Z);
            int x2 = TXDebug(camera.cornerTR.X);
            int y2 = TYDebug(camera.cornerTR.Z);

            //Let's just say it is on screen to save on Lots of &'s
            //screen.Line(x, y, x2, y2, 0xffffff);

            //primitives
            foreach (Primitive prim in scene.primitives)
            {
                if (prim is Sphere)
                {
                    Sphere s = (Sphere)prim;
                    DrawCircle(screen, TXDebug(s.center.X), TYDebug(s.center.Z), s.radius, s.GetColorInt());
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
