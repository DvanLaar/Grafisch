using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using template.Lights;
using template.Primitives;
using Template;

namespace template
{
    public class Raytracer
    {
        public static float EPS = 1e-8f;
        public int depthlimit = 16;

        public Camera camera;
        public Scene scene;

        public Texture skybox;

        //Needs to be made dependent on camera and FOV
        public Vector3 screencorner_topleft = new Vector3(-1f, -1f, 0f) + new Vector3(0, 0, 1f);
        public Vector3 screencorner_topright = new Vector3(1f, -1f, 0f) + new Vector3(0, 0, 1f);
        public Vector3 screencorner_bottomleft = new Vector3(-1f, 1f, 0f) + new Vector3(0, 0, 1f);

        public Surface screen;

        public Raytracer()
        {
            camera = new Camera();
            scene = new Scene();

            scene.AddPrimitive(new Sphere(new Vector3(0, 0, 6f), 1.5f, new Vector3(1f, 1f, 1f),0.5f));
            scene.AddPrimitive(new Sphere(new Vector3(3f, 0, 6f), 0.5f, new Vector3(0f, 1f, 0f),0.9f));
            scene.AddPrimitive(new Sphere(new Vector3(-3f, 0, 6f), 1, new Vector3(0f, 0f, 1f),0.1f));

            Texture floortexture = new Texture("Textures/floor.bmp");
            Texture skytexture = new Texture("Textures/sky.bmp");
            Texture pepetexture = new Texture("Textures/pepe.bmp");
            Texture jbtexture = new Texture("Textures/jb.png");
            skybox = new Texture("Textures/skybox.bmp");
            scene.AddPrimitive(new TexturedPlane(floortexture, Vector3.Normalize(Vector3.UnitX + Vector3.UnitZ), -Vector3.UnitY, 1.5f, new Vector3(1f, 1f, 1f),0.7f));
            //scene.AddPrimitive(new TexturedPlane(skytexture,Vector3.UnitX,-Vector3.UnitZ,8f,new Vector3(0.5f,0.5f,0.5f),1f,100f));


            scene.AddPrimitive(new TexturedTriangle(new Vector3(-1f, -0.6f, 3f) + new Vector3(-1.5f, 0f, -0.4f), new Vector3(+1f, -0.6f, 3f) + new Vector3(-1.5f, 0f, 0f), new Vector3(-1f, -2.6f, 3f) + new Vector3(-1.5f, 0f, -0.4f), jbtexture, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f),1f));
            scene.AddPrimitive(new TexturedTriangle(new Vector3(+1f, -0.6f, 3f) + new Vector3(-1.5f, 0f, 0f), new Vector3(+1f, -2.6f, 3f) + new Vector3(-1.5f, 0f, 0f), new Vector3(-1f, -2.6f, 3f) + new Vector3(-1.5f, 0f, -0.4f), jbtexture, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f),1f));

            scene.AddPrimitive(new TexturedTriangle(new Vector3(1f, 0f, -1f) , new Vector3(-1f, 0f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f), 1f));
            scene.AddPrimitive(new TexturedTriangle(new Vector3(-1f, 0f, -1f), new Vector3(-1f, -2f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f), 1f));

            //Slow, but awesome!
            //scene.AddPrimitive(new Mesh("Objects/teapot.ob",new Vector3(1f,0.8f,0.6f),new Vector3(-0.5f,1f,2f),1f,0.1f));

            //scene.AddLight(new Light(new Vector3(0, 0, 0), new Vector3(4f, 4f, 4f)));
            scene.AddLight(new PointLight(new Vector3(0,-3f,-0.8f), new Vector3(10f,10f,10f)));
            //scene.AddLight(new Light(new Vector3(5, -5, 0), new Vector3(0f, 0f, 10f)));
            scene.AddLight(new DirectionalLight(new Vector3(0,1,0),new Vector3(0.2f,0.2f,0.2f)));
        }

        public void Render(Surface screen)
        {
            this.screen = screen;
            //Initial part of Debug
            DrawInitialDebug(screen);


            //Cast rays 
            Ray ray;
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    ray = GenerateRay(x, y);
                    screen.Plot(x, y, V3toInt(CalculateColor(ray)));
                }
                //Console.WriteLine(x + " checked");
            }
        }

        private Vector3 CalculateColor(Ray ray)
        {
            if (ray.depth >= 16)
                return Vector3.Zero;

            Intersection intersection = scene.Intersect(ray);
            if (intersection != null)
            {
                float diffuse = intersection.primitive.material.diffuse;
                Vector3 diffusepart = new Vector3();
                Vector3 color = intersection.primitive.GetColor(ray, intersection);
                if (diffuse > EPS)
                {
                    foreach (Light light in scene.lights)
                    {
                        float intensityscale = light.GetIntensity(ray,intersection,scene);
                        diffusepart += color *light.intensity*intensityscale;
                    }
                }
                Vector3 specularpart = new Vector3();
                if (1-diffuse > EPS)
                {
                    Vector3 intersectionpoint = ray.origin + (intersection.value*ray.direction);
                    Vector3 normal = intersection.normal;
                    Ray secondaryRay = new Ray(intersectionpoint,ray.direction - 2*Vector3.Dot(ray.direction,normal)*normal,ray.depth+1);
                    specularpart = color * CalculateColor(secondaryRay);
                }
                return (diffuse*diffusepart) + ((1f-diffuse)*specularpart);

            } else
            { //The ray doesn't collide with any primitive, so return the color of the skybox
                Vector3 dirnorm = ray.direction;
                double tp = 2 * Math.PI;
                int texx = (int)(MathHelper.Clamp((Math.Atan2(dirnorm.Z , dirnorm.X)+Math.PI) / tp * (skybox.Width - 1),0,skybox.Width-1));
                int texy = (int)(Math.Acos(-dirnorm.Y) / Math.PI * (skybox.Height - 1));
                return skybox.Data[texx, texy];
            }
        }

        private int V3toInt(Vector3 v)
        {
            int r = (int)MathHelper.Clamp(v.X * 255f, 0, 255f);
            int g = (int)MathHelper.Clamp(v.Y * 255f, 0, 255f);
            int b = (int)MathHelper.Clamp(v.Z * 255f, 0, 255f);
            return (r << 16) + (g << 8) + b;
        }

        private void DrawInitialDebug(Surface screen)
        {
            //Camera
            int x = TXDebug(camera.Position.X);
            int y = TYDebug(camera.Position.Z);
            if ((x >= 512) && (x < 1024) && (y >= 0) && (y < 512))
                screen.Plot(x, y, 0xffffff);
            //Screenplane
            x = TXDebug(screencorner_topleft.X);
            y = TYDebug(screencorner_topleft.Z);
            int x2 = TXDebug(screencorner_topright.X);
            int y2 = TYDebug(screencorner_topright.Z);
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
            int x2 = TXDebug(ray.origin.X + (intersection.value * ray.direction.X));
            int y2 = TYDebug(ray.origin.Z + (intersection.value * ray.direction.Z));
            screen.Line(x1, y1, x2, y2, c);
            if (intersection.normal == Vector3.UnitY)
                return;
            float normalscale = 0.3f;
            x1 = TXDebug(ray.origin.X + (intersection.value * ray.direction.X) + (normalscale * intersection.normal.X));
            y1 = TYDebug(ray.origin.Z + (intersection.value * ray.direction.Z) + (normalscale * intersection.normal.Z));
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

        private Ray GenerateRay(int x, int y)
        {
            Vector3 origin = camera.Position;
            Vector3 direction = (screencorner_topleft +
                                (x / 512f) * (screencorner_topright - screencorner_topleft) +
                                (y / 512f) * (screencorner_bottomleft - screencorner_topleft)) - camera.Position;
            direction.Normalize();
            return new Ray(origin, direction);
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
    }

    public class Camera
    {
        public Vector3 Position = new Vector3(0, 0, 0);
        public Vector3 Direction = new Vector3(0, 0, 1);

        public Camera()
        {

        }
    }
}
