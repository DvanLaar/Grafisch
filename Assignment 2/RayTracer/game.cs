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
            Camera.DisplayKeyInfo();
            raytracer = new Raytracer();
        }

        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);
            raytracer.Render(screen);
        }

        /// <summary>
        /// Processes all the input in one render
        /// </summary>
        /// <param name="keyboard">contains keyboard strokes</param>
        /// <param name="mouse">contains mouse gestures</param>
        public void ProcessInput(KeyboardState keyboard, MouseDevice mouse)
        {
            this.raytracer.processInput(keyboard, mouse);
        }
    }

} // namespace Template