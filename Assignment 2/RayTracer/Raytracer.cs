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
using OpenTK.Graphics.OpenGL;

namespace RayTracer
{
    /**
     * OpenGL uses this standard coordinate system, so we use this as well:
     * https://learnopengl.com/img/getting-started/coordinate_systems_right_handed.png
     */
    public class Raytracer
    {
        public const int MAX_RECURSION = 16;
        public const float EPS = 1e-8f;
        public const int WIDTH = 512 << 0;

        private Camera camera;
        private Scene scene;
        private Texture skybox;
        private Surface screen;

        public Raytracer()
        {
            camera = new Camera();
            scene = new Scene();
            skybox = new Texture("Textures/skybox.bmp");

            // white ball
            scene.AddPrimitive(new Sphere(new Vector3(0, 1.5f, -6f), 1.5f, new Vector3(1f, 1f, 1f), 0.5f));
            // green ball
            scene.AddPrimitive(new Sphere(new Vector3(3f, 1.5f, -6f), 0.5f, new Vector3(0f, 1f, 0f), 0.9f));
            // blue ball
            scene.AddPrimitive(new Sphere(new Vector3(-3f, 1.5f, -6f), 1, new Vector3(0f, 0f, 1f), 0.1f));

            Texture floortexture = new Texture("Textures/floor.bmp");

            // normal is: Vector3.UnitY
            scene.AddPrimitive(new TexturedPlane(floortexture, Vector3.UnitX, Vector3.UnitZ, 0f, new Vector3(1f, 1f, 1f), 0.7f));
            //scene.AddPrimitive(new TexturedPlane(skytexture,Vector3.UnitX,-Vector3.UnitZ,8f,new Vector3(0.5f,0.5f,0.5f),1f,100f));
            
            // Texture jbtexture = new Texture("Textures/jb.png");
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(-1f, -0.6f, 3f) + new Vector3(-1.5f, 0f, -0.4f), new Vector3(+1f, -0.6f, 3f) + new Vector3(-1.5f, 0f, 0f), new Vector3(-1f, -2.6f, 3f) + new Vector3(-1.5f, 0f, -0.4f), jbtexture, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f), 1f));
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(+1f, -0.6f, 3f) + new Vector3(-1.5f, 0f, 0f), new Vector3(+1f, -2.6f, 3f) + new Vector3(-1.5f, 0f, 0f), new Vector3(-1f, -2.6f, 3f) + new Vector3(-1.5f, 0f, -0.4f), jbtexture, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f), 1f));

            // Texture pepetexture = new Texture("Textures/pepe.bmp");
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(1f, 0f, -1f), new Vector3(-1f, 0f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f), 1f));
            // scene.AddPrimitive(new TexturedTriangle(new Vector3(-1f, 0f, -1f), new Vector3(-1f, -2f, -1f), new Vector3(1f, -2f, -1f), pepetexture, new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector3(1f, 1f, 1f), 1f));

            //Slow, but awesome!
            //scene.AddPrimitive(new Mesh("Objects/teapot.ob",new Vector3(1f,0.8f,0.6f),new Vector3(-0.5f,1f,2f),1f,0.1f));

            // scene.AddLight(new Light(new Vector3(0, 0, 0), new Vector3(4f, 4f, 4f)));
            scene.AddLight(new PointLight(new Vector3(.5f, 1f, 2f), new Vector3(10f, 10f, 10f), new Vector3(0f, 10f, 0f)));
            // scene.AddLight(new Light(new Vector3(5, -5, 0), new Vector3(0f, 0f, 10f)));
            // scene.AddLight(new DirectionalLight(new Vector3(0, 1, 0), new Vector3(0.2f, 0.2f, 0.2f)));
        }

        public void Render(Surface screen)
        {
            Console.Write("render; ");
            this.screen = screen;
            //Initial part of Debug
            DrawInitialDebug(screen);

            //Cast rays 
            int speedtradeoff = 1;

            for (int x = 0; x < Camera.resolution; x += speedtradeoff)
            {
                for (int y = 0; y < Camera.resolution; y += speedtradeoff)
                {
                    Ray ray = camera.getDirection(x, y);
                    int color = V3toInt(CalculateColor(ray));
                    for (int xx = 0; xx < speedtradeoff; xx++)
                    {
                        for (int yy = 0; yy < speedtradeoff; yy++)
                        {
                            screen.Plot(x + xx, y + yy, color);
                        }
                    }
                }
            }
        }

        private Vector3 CalculateColor(Ray ray, int recursionDepth = 0)
        {
            if (recursionDepth >= MAX_RECURSION)
                return Vector3.Zero;

            Intersection intersection = scene.Intersect(ray);
            if (intersection != null)
            {
                float diffuse = intersection.primitive.material.diffuse;
                Vector3 color = intersection.primitive.GetColor(ray, intersection);
                Vector3 diffusepart = new Vector3(), specularpart = new Vector3();
                if (diffuse > EPS)
                {
                    foreach (Light light in scene.lights)
                    {
                        float intensityscale = light.GetIntensity(ray, intersection, scene);
                        diffusepart += light.intensity * intensityscale;
                    }
                }
                if (1 - diffuse > EPS)
                {
                    Vector3 normal = intersection.normal;
                    Ray secondaryRay = new Ray(intersection.location, ray.direction - 2 * Vector3.Dot(ray.direction, normal) * normal);
                    specularpart = CalculateColor(secondaryRay, recursionDepth + 1);
                }
                return color * (diffuse * diffusepart + (1f - diffuse) * specularpart);
            }
            else
            {
                // The ray doesn't collide with any primitive, so return the color of the skybox
                Vector3 dirnorm = ray.direction;
                double tp = 2 * Math.PI;
                int texx = MathHelper.Clamp((int) ((Math.Atan2(dirnorm.Z, dirnorm.X) + Math.PI) / tp * (skybox.Width - 1)), 0, skybox.Width - 1);
                int texy = dirnorm.Y >= 1.0f - 1e-8f ? 0 : (int)(Math.Acos(dirnorm.Y) / Math.PI * (skybox.Height - 1));
                return skybox.Data[texx, texy];
            }
        }

        private int V3toInt(Vector3 v)
        {
            int r = MathHelper.Clamp((int)(v.X * 255f), 0, 255);
            int g = MathHelper.Clamp((int)(v.Y * 255f), 0, 255);
            int b = MathHelper.Clamp((int)(v.Z * 255f), 0, 255);
            return (r << 16) | (g << 8) | b;
        }

        private void DrawInitialDebug(Surface screen)
        {
            GL.Disable(EnableCap.Texture2D);

            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 m = Matrix4.CreateScale(1 / 10f);
            GL.LoadMatrix(ref m);

            GL.Color3(0.8f, 0.2f, 0.2f);
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(camera.Position.Xz);
            GL.End();

            //Draw camera end
            Vector2 sv1 = (camera.cornerTL.Xz - camera.Position.Xz) * 30.0f;
            Vector2 sv2 = (camera.cornerTR.Xz - camera.Position.Xz) * 30.0f;

            GL.Color3(0.8f, 0.5f, 0.5f);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(camera.Position.Xz);
            GL.Vertex2((camera.Position.Xz + sv1));

            GL.Vertex2(camera.Position.Xz);
            GL.Vertex2((camera.Position.Xz + sv2));

            GL.Color3(0.2f, 1.0f, 0.7f);

            for (int x = 0; x <= WIDTH; x += 64)
            {
                Ray currentray = camera.GenerateRay(x, WIDTH >> 1);
                debugRay(camera.Position, currentray, MAX_RECURSION);
            }

            GL.Color3(0.4f, 1.0f, 0.4f);
            GL.Vertex2(camera.cornerTL.Xz);
            GL.Vertex2(camera.cornerTR.Xz);
            GL.End();


            //primitives
            foreach (Primitive prim in scene.primitives)
            {
                prim.Debug();
            }

        }

        private void debugRay(Vector3 pos, Ray ray, int depth)
        {
            Intersection intersection = scene.Intersect(ray);
            if (intersection == null)
                return;
            Vector3 destination = (pos + ray.direction * intersection.location);
            GL.Color3(0.4f, 1.0f, 0.4f);

            GL.Vertex2(pos.Xz);
            GL.Vertex2(destination.Xz);
            if (intersection.primitive != null && depth > 0)
            {
                debugShadowRay(destination);
                if (intersection.primitive.material.specular > 0)
                {
                    Vector3 m = ray.mirror(intersection.primitive.getNormal(destination));
                    Ray newRay = new Ray(destination + m * 0.05f, m);
                    debugRay(destination, newRay, depth - 1);
                }
                Material material = intersection.primitive.material;
                if (material.diffuse > 0 && depth > 0)
                {
                    Ray newRay = refract(intersection.primitive, ray, destination);
                    if (newRay != null)
                        debugRay(destination, newRay, depth - 1);
                    else
                    {
                        Vector3 m = ray.mirror(intersection.primitive.getNormal(destination));
                        newRay = new Ray(destination + m * 0.05f, m);
                        debugRay(destination, newRay, depth - 1);
                    }
                }
            }
        }

        private void debugShadowRay(Vector3 position)
        {
            GL.Color3(1.0f, 1.0f, 0.4f);
            foreach (Light light in scene.lights)
            {
                Vector3 direction = position - light.location.Normalized();
                Ray ray = new Ray(light.location, direction);
                bool isBlocked = false;
                float curDistance;

                foreach (Primitive prim in scene.primitives)
                {
                    curDistance = (prim.Intersect(ray).location - position).Length;
                    if (curDistance * curDistance < direction.LengthSquared * 0.999f && curDistance > 0)
                    {
                        GL.Vertex2(light.location.Xz);
                        GL.Vertex2(light.location.Xz + curDistance * direction.Xz);
                        isBlocked = true;
                        break;
                    }
                }
                if(!isBlocked)
                {
                    GL.Vertex2(light.location.Xz);
                    GL.Vertex2(position.Xz);
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

        public Ray refract(Primitive prim, Ray ray, Vector3 destination)
        {
            Vector3 normal = prim.getNormal(destination);
            float cos = Vector3.Dot(-ray.direction, normal);
            if (cos < 0)
            {
                normal = -normal;
                cos = -cos;
            }

            Vector3 dir = ray.direction + normal * cos;
            dir.Normalize();
            Ray newRay = new Ray(destination + 0.012f * dir, dir);
            return newRay;
        }

        public float minX = -5, maxX = 5, minY = -1, maxY = 9;

        public int TXDebug(float x)
        {
            float worldWidth = maxX - minX;
            return 512 + (int)((x - minX) * (512 / worldWidth));
        }

        public int TYDebug(float y)
        {
            float worldHeight = maxY - minY;
            return (int)((-y + maxY) * (512 / worldHeight));
        }

        public void DrawCircle(Surface screen, int x, int y, float r, int c)
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
            this.camera.processKeyboard(keyboard);
        }
    }
}
