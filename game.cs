using System;
using System.IO;

namespace Template {

class Game
{
	// member variables
	public Surface screen;
    public float a;
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

    private int TX(float x)
    {
        return (int)((x + 2) * screen.width / 4);
    }

    private int TY(float y)
    {
        return (int)((-y + 2) * screen.height / 4);
    }
}

} // namespace Template