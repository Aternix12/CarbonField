using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonField.Libraries;
using Microsoft.Xna.Framework;

namespace CarbonField
{
    public class TerrainManager
    {
        private readonly FastNoiseLite noise;
        private int octaves;
        private float persistence;
        private float lacunarity;

        public TerrainManager()
        {
            noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            // Set default values for octaves, persistence, and lacunarity
            octaves = 4;
            persistence = 0.5f;
            lacunarity = 2.0f;
        }

        public void SetOctaves(int octaves)
        {
            this.octaves = octaves;
        }

        public void SetPersistence(float persistence)
        {
            this.persistence = persistence;
        }

        public void SetLacunarity(float lacunarity)
        {
            this.lacunarity = lacunarity;
        }

        public Terrain GetTerrainType(float x, float y)
        {
            float noiseValue = GetFractalNoise(x, y);

            // Map noise value to a terrain type (example logic)
            if (noiseValue < -0.3) return Terrain.Grass;
            else if (noiseValue < 0) return Terrain.Grass;
            else if (noiseValue < 0.3) return Terrain.Dirt;
            else return Terrain.Dirt;
        }

        private float GetFractalNoise(float x, float y)
        {
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = x * frequency;
                float sampleY = y * frequency;
                float perlinValue = noise.GetNoise(sampleX, sampleY);
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return noiseHeight;
        }
    }
}
