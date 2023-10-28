using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace CarbonField
{
    public class FrameCounter
    {
        public FrameCounter()
        {
        }

        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }

        public const int MAXIMUM_SAMPLES = 100;

        private readonly Queue<float> _sampleBuffer = new Queue<float>();

        public string FpsString { get; private set; }

        // Add a timer to keep track of elapsed time
        private float elapsedTime = 0.0f;

        private readonly StringBuilder fpsStringBuilder = new StringBuilder();
        private float sumOfFrames = 0;
        private int sampleCount = 0;

        public bool Update(float deltaTime)
        {
            elapsedTime += deltaTime;

            // Only update FPS counter once a second
            if (elapsedTime >= 0.5f)
            {
                CurrentFramesPerSecond = 0.5f / deltaTime;
                sumOfFrames += CurrentFramesPerSecond;
                sampleCount++;

                _sampleBuffer.Enqueue(CurrentFramesPerSecond);

                if (_sampleBuffer.Count > MAXIMUM_SAMPLES)
                {
                    float oldestFrame = _sampleBuffer.Dequeue();
                    sumOfFrames -= oldestFrame;
                    sampleCount--;
                }

                AverageFramesPerSecond = sumOfFrames / sampleCount;

                fpsStringBuilder.Clear();
                fpsStringBuilder.AppendFormat("FPS: {0}", Math.Round(AverageFramesPerSecond));
                FpsString = fpsStringBuilder.ToString();

                TotalFrames++;
                TotalSeconds += deltaTime;

                // Reset the elapsed time
                elapsedTime = 0.0f;
                return true;
            }

            return false;
        }
    }
}