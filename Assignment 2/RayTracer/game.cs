using System;
using System.IO;
using template;

namespace Template {

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
		    screen.Clear( 0 );
            raytracer.Render(screen);
        }
}

} // namespace Template