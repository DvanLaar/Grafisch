using System;
using System.IO;

namespace Template {

class Game
{
	// member variables
	public Surface screen;
	// initialize
	public void Init()
	{
	}
	// tick: renders one frame
	public void Tick()
	{
		screen.Clear( 0 );
		screen.Print( "hello world", 2, 2, 0xffffff );
        screen.Line(2, 20, 160, 20, 0xff0000);
        int dx = screen.width / 2 - 128;
        int dy = screen.height / 2 - 128;
        for (int x = 0; x < 256; x++)
        {
            for (int y = 0; y < 256; y++)
            {
                int location = (x + dx) + (y + dy) * screen.width;
                screen.pixels[location] = (x << 16) + (y << 8);
            }
        }
	}
}

} // namespace Template