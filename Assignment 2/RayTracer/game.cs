using System;
using System.IO;
using OpenTK.Input;
using RayTracer;

namespace Template
{

    class Game
    {
        public Surface screen;
        public Raytracer raytracer;
        
        public void Init()
        {
            raytracer = new Raytracer();
        }

        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);
            raytracer.Render(screen);
        }

        public void ProcessKeyboard(KeyboardState keyboard)
        {
            this.raytracer.processKeyboard(keyboard);
        }
    }

} // namespace Template