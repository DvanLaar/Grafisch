using OpenTK.Input;
using System;
using System.IO;

namespace Template {

    class Game
    {
        // member variables
        public Surface screen;

        public float zoomSpeed = 0.1f;
        public float translateXSpeed = 0.1f;
        public float translateYSpeed = 0.1f;

        // initialize
        public void Init()
        {
        }

        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);

            KeyboardInput();

            DrawGridLines(0x008800, 0x004400, 1f, 1f);
            PlotFunction(0.1f, 0xff0000);
            PlotPolarFunction(0.01f, 0x0000ff,0,(float)Math.PI*10f);
            screen.Print("x*sin(x)",5,5, 0xff0000);
            screen.Print("t*sin(t)*cos(t)    (polar coordinates)", 5, 25, 0x0000ff);
        }

        private void DrawLine(float x1, float y1, float x2, float y2, int color)
        {
            screen.Line(TX(x1), TY(y1), TX(x2), TY(y2), color);
        }

        private void KeyboardInput()
        {
            KeyboardState keyboard = Keyboard.GetState();
            //Zoom out
            if (keyboard[Key.Z]) { minX -= zoomSpeed; maxX += zoomSpeed; minY -= zoomSpeed; maxY += zoomSpeed; }
            //Zoom in
            if (keyboard[Key.X]
                && minX + zoomSpeed < maxX - zoomSpeed
                && minY + zoomSpeed < maxY - zoomSpeed) { minX += zoomSpeed; maxX -= zoomSpeed; minY += zoomSpeed; maxY -= zoomSpeed; }
            //Move left
            if (keyboard[Key.Left]) { minX -= translateXSpeed; maxX -= translateXSpeed; }
            //Move right
            if (keyboard[Key.Right]) { minX += translateXSpeed; maxX += translateXSpeed; }
            //Move up
            if (keyboard[Key.Up]) { minY += translateYSpeed; maxY += translateYSpeed; }
            //Move down
            if (keyboard[Key.Down]) { minY -= translateYSpeed; maxY -= translateYSpeed; }

        }

        private void DrawGridLines(int coloraxis, int colorlines, float horizontalstepsize, float verticalstepsize)
        {
            //Horizontal
            for (float y = (float)Math.Floor(minY); y < maxY + verticalstepsize; y += verticalstepsize)
            {
                DrawLine(minX, y, maxX, y, colorlines);
                //darker lines inbetween
                DrawLine(minX, y - (verticalstepsize / 2f), maxX, y - (verticalstepsize / 2), colorlines / 4);
            }
            //Vertical
            for (float x = (float)Math.Floor(minX); x < maxX + horizontalstepsize; x += horizontalstepsize)
            {
                DrawLine(x, minY, x, maxY, colorlines);
                //darker lines inbetween
                DrawLine(x - (horizontalstepsize / 2f), minY, x - (horizontalstepsize / 2f), maxY, colorlines / 4);
            }
            //X-Axis
            DrawLine(minX, 0, maxX, 0, coloraxis);
            //Y-Axis
            DrawLine(0, minY, 0, maxY, coloraxis);
        }

        private void PlotFunction(float stepSize, int color)
        {
            if (stepSize <= 0f) //To avoid infinite loop
                return;
            float prevValue = evaluateFunction(minX);
            for (float x = minX + stepSize; x <= maxX; x += stepSize)
            {
                float value = evaluateFunction(x);
                DrawLine(x - stepSize, prevValue, x, value, color);
                prevValue = value;
            }
        }

        private void PlotPolarFunction(float stepSize, int color, float minTheta = (float)-Math.PI * 2f, float maxTheta = (float)Math.PI * 2f)
        {
            if (stepSize <= 0f) //To avoid infinite loop
                return;
            float prevValue = evaluatePolarFunction(minTheta);
            for (float theta = minTheta + stepSize; theta <= maxTheta; theta += stepSize)
            {
                float value = evaluatePolarFunction(theta);
                DrawLine(prevValue * (float)Math.Cos(theta - stepSize), prevValue * (float)Math.Sin(theta - stepSize), value * (float)Math.Cos(theta), value * (float)Math.Sin(theta), color);
                prevValue = value;
            }
        }

        public float minX = -1, maxX = 1, minY = -1, maxY = 1;

        /// <summary>
        /// Transforms X-coordinate from world coordinates to screen coordinates
        /// </summary>
        private int TX(float x)
        {
            float worldWidth = maxX - minX;
            return (int)((x - minX) * (screen.width / worldWidth));
        }

        /// <summary>
        /// Transforms Y-coordinate from world coordinates to screen coordinates
        /// </summary>
        private int TY(float y)
        {
            float worldHeight = maxY - minY;
            return (int)((-y + maxY) * (screen.height / worldHeight));
        }


        private float evaluateFunction(float x)
        {
            return (float)(Math.Sin(x) * x);
        }

        private float evaluatePolarFunction(float theta)
        {
            return (float)(theta*Math.Cos(theta)*Math.Sin(theta));
        }

    }
} // namespace Template