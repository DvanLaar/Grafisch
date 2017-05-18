using System;
using System.IO;
using OpenTK.Input;
using RayTracer;

namespace Template
{

    class Game
    {
        // member variables
        public Surface screen;

        public Raytracer raytracer;

        // initialize
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