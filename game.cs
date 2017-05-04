using System;
using System.IO;

namespace Template {

class Game
{
	// member variables
	public Surface screen;
    public float a;

    public float minX = -5, maxX = 2, minY = -5, maxY = 2;

	// initialize
	public void Init()
	{
        a = 0;
	}
	// tick: renders one frame
	public void Tick()
	{
        screen.Clear(0);
        a += 0.1f;
        float[] x = new float[] {-1, 1, 1, -1};
        float[] y = new float[] { 1, 1, -1, -1 };
        float[] rx = new float[4];
        float[] ry = new float[4];

        for (int i = 0; i < 4; i++)
        {
            rx[i] = (float)(x[i] * Math.Cos(a) - y[i] * Math.Sin(a));
            ry[i] = (float)(x[i] * Math.Sin(a) + y[i] * Math.Cos(a));
        }

        screen.Line(TX(rx[0]), TY(ry[0]), TX(rx[1]), TY(ry[1]), 0xffffff);
        screen.Line(TX(rx[1]), TY(ry[1]), TX(rx[2]), TY(ry[2]), 0xffffff);
        screen.Line(TX(rx[2]), TY(ry[2]), TX(rx[3]), TY(ry[3]), 0xffffff);
        screen.Line(TX(rx[3]), TY(ry[3]), TX(rx[0]), TY(ry[0]), 0xffffff);
	}

    /// <summary>
    /// Transforms X-coordinate from world coordinates to screen coordinates
    /// </summary>
    private int TX(float x)
    {
        float worldWidth = maxX - minX;
        return (int)((x - minX)*(screen.width/worldWidth));
    }

    /// <summary>
    /// Transforms Y-coordinate from world coordinates to screen coordinates
    /// </summary>
    private int TY(float y)
    {
        float worldHeight = maxY - minY;
        return (int)((-y+maxY)*(screen.height/worldHeight));
    }
}

} // namespace Template