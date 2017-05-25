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

namespace RayTracer
{
    /**
     * OpenGL uses this standard coordinate system, so we use this as well:
     * https://learnopengl.com/img/getting-started/coordinate_systems_right_handed.png
     */
    public class Raytracer : Camera.NoActionListener
    {
        public const int MAX_RECURSION = 2;

        private Camera camera;
        private Scene scene;
        private Texture skybox = null;
        private int SpeedUp = 16;

        public Raytracer()
        {
            camera = new Camera(this);
            scene = new Scene();
            skybox = new Texture("Textures/skybox2.bmp");

            // white ball
            scene.AddPrimitive(new Sphere(new Vector3(0, 1.5f, -6f), 1.5f, Utils.WHITE, 0.5f));
            // green ball
            scene.AddPrimitive(new Sphere(new Vector3(3f, 1.5f, -6f), 0.5f, new Vector3(0f, 1f, 0f), 0.9f));
            // blue ball
            scene.AddPrimitive(new Sphere(new Vector3(-3f, 1.5f, -6f), 1, new Vector3(0f, 0f, 1f), 0.1f));

            // normal is: Vector3.UnitY
            Texture floortexture = new Texture("Textures/floor.bmp");
            scene.AddPrimitive(new TexturedPlane(floortexture, Vector3.UnitX, -Vector3.UnitZ, 0f, Utils.WHITE, 0.7f));

            if (true)
            {
                Texture jbtexture = new Texture("Textures/jb.png");
                Vector3 jb_bl = new Vector3(1.9f, 2.6f, -5.4f), jb_dirx = new Vector3(2f, 0f, 1f), jb_diry = new Vector3(0f, 2f, 0f);
                scene.AddPrimitive(new TexturedTriangle(
                    jb_bl, jb_bl + jb_dirx, jb_bl + jb_diry, jbtexture,
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f),
                    Utils.WHITE, 1f));
                scene.AddPrimitive(new TexturedTriangle(
                    jb_bl + jb_dirx, jb_bl + jb_dirx + jb_diry, jb_bl + jb_diry, jbtexture,
                    new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f),
                    Utils.WHITE, 1f));
            }

            // Texture pepetexture = new Texture("Textures/pepe.bmp");
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(1f, 0f, -1f), new Vector3(-1f, 0f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), Utils.WHITE, 1f));
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(-1f, 0f, -1f), new Vector3(-1f, -2f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), Utils.WHITE, 1f));

            // Slow, but awesome!
            // scene.AddPrimitive(new Mesh("Objects/teapot.obj", new Vector3(1f, 0.8f, 0.6f), new Vector3(-0.5f, 1.5f, -2f), .5f, 1f));

            scene.AddLight(new Light(Utils.WHITE * 0.25f));
            scene.AddLight(new PointLight(new Vector3(1.5f, 4f, -4f), Utils.WHITE * 2.5f));
            // scene.AddLight(new Light(new Vector3(5, -5, 0), new Vector3(0f, 0f, 10f)));
            scene.AddLight(new DirectionalLight(new Vector3(4f, -1f, 0.25f), Utils.WHITE * 2.5f));
        }

        public void Render(Surface screen)
        {
            // Console.Write("render; ");

            // Initial part of Debug
            // DrawInitialDebug(screen);

            // Cast rays
            for (int x = 0; x < Camera.resolution; x += SpeedUp)
            {
                for (int y = 0; y < Camera.resolution; y += SpeedUp)
                {
                    Ray ray = camera.getDirection(x, y);
                    int color = Utils.GetRGBValue(CalculateColor(ray));
                    for (int xx = 0; xx < SpeedUp; xx++)
                    {
                        for (int yy = 0; yy < SpeedUp; yy++)
                        {
                            screen.Plot(x + xx, y + yy, color);
                        }
                    }
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
                Vector3 diffusepart = new Vector3(), specularpart = new Vector3();
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
            screen.Line(x, y, x2, y2, 0xffffff);

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

        private KeyboardState lastState;

        public void processKeyboard(KeyboardState keyboard)
        {
            this.camera.processKeyboard(keyboard);
            if (keyboard[Key.KeypadPlus] && !lastState[Key.KeypadPlus]) SpeedUp = Math.Min(SpeedUp * 2, 512);
            if (keyboard[Key.KeypadMinus] && !lastState[Key.KeypadMinus]) SpeedUp = Math.Max(SpeedUp / 2, 1);
            lastState = keyboard;
        }

        int Camera.NoActionListener.OnNoAction()
        {
            int ret = SpeedUp;
            SpeedUp = Math.Min(SpeedUp, 2);
            return ret;
        }

        void Camera.NoActionListener.RestoreOld(int value)
        {
            SpeedUp = value;
        }
    }
}
