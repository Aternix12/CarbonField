using CarbonField.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CarbonField.Game
{
    public class IsometricManager
    {
        private readonly int width;
        private readonly int height;
        private readonly Tile[,] tileMap;
        private readonly Dictionary<Terrain, SpriteSheet> terrainSpriteSheets;
        private Effect terrainBlendEffect;
        private SpriteSheet blendMaps;
        private SpriteSheet grassSpriteSheet;
        private SpriteSheet dirtSpriteSheet;
        private SpriteSheet blendMaps;
        private Effect terrainBlendEffect;
        private Texture2D grassTexture;
        private Texture2D dirtTexture;



        public IsometricManager(int width, int height, Dictionary<Terrain, SpriteSheet> spriteSheets)
        {
            this.width = width;
            this.height = height;
            this.terrainSpriteSheets = spriteSheets;
            this.tileMap = new Tile[width, height];
        }

        public void Initialize()
        {
            float halfTileWidth = Tile.Width / 2f;
            float halfTileHeight = Tile.Height / 2f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the isometric position
                    Vector2 isoPosition = new Vector2(
                        x * halfTileWidth - y * halfTileWidth,
                        x * halfTileHeight + y * halfTileHeight
                    );

                    Terrain type = (x + y) % 2 == 0 ? Terrain.Grass : Terrain.Dirt;
                    //Terrain type = Terrain.Grass;

                    // Correctly passing the terrainSpriteSheets dictionary
                    int spriteIndexX = x % 10;
                    int spriteIndexY = y % 10;
                    tileMap[x, y] = new Tile(isoPosition, type, terrainSpriteSheets, spriteIndexX, spriteIndexY);
                }
            }
        }

        public void LoadContent(ContentManager content)
        {
            // Populate the SpriteSheet with tiles (assuming 10x10 grid of 64x32 tiles)
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    grassSpriteSheet.AddSprite($"grass_{x}_{y}", x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
                    dirtSpriteSheet.AddSprite($"dirt_{x}_{y}", x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
                }
            }

            

            // Initialize blend maps for each tile
            for (int y = 0; y < IsoManager.Height; y++)
            {
                for (int x = 0; x < IsoManager.Width; x++)
                {
                    IsoManager.TileMap[x, y].InitializeBlendMap(blendMaps, IsoManager);
                }
            }

            // Load the blend effect and blend maps
            terrainBlendEffect = content.Load<Effect>("shaders/TerrainBlend");
            Texture2D blendMapTexture = content.Load<Texture2D>("shaders/blendmaps/terrain");
            blendMaps = new SpriteSheet(blendMapTexture);
            for (int i = 0; i < 4; i++)
            {
                blendMaps.AddSprite($"blendMap{i}", i * blendMapTexture.Width / 4, 0, blendMapTexture.Width / 4, blendMapTexture.Height);
            }

            // Initialize blend maps for each tile
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    TileMap[x, y].InitializeBlendMap(blendMaps, this);
                }
            }

            // Load grass terrain spritesheet
            Texture2D grassSheetTexture = _content.Load<Texture2D>("sprites/terrain/grass_terrain");
            grassSpriteSheet = new SpriteSheet(grassSheetTexture);

            // Load dirt terrain spritesheet
            Texture2D dirtSheetTexture = _content.Load<Texture2D>("sprites/terrain/dirt_terrain");
            dirtSpriteSheet = new SpriteSheet(dirtSheetTexture);

            terrainBlendEffect = _content.Load<Effect>("shaders/TerrainBlend");
            grassTexture = grassSpriteSheet.Texture;
            dirtTexture = dirtSpriteSheet.Texture;
            Texture2D blendMapTexture = _content.Load<Texture2D>("shaders/blendmaps/terrain");
            blendMaps = new SpriteSheet(blendMapTexture);
            for (int i = 0; i < 4; i++)
            {
                blendMaps.AddSprite($"blendMap{i}", i * blendMapTexture.Width / 4, 0, blendMapTexture.Width / 4, blendMapTexture.Height);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawTilesByTerrain(spriteBatch, Terrain.Grass, terrainBlendEffect);
            DrawTilesByTerrain(spriteBatch, Terrain.Dirt, terrainBlendEffect);  
        }

        private void DrawTilesByTerrain(SpriteBatch spriteBatch, Terrain terrain, Effect terrainBlendEffect)
        {
            for (int y = 0; y < IsoManager.Height; y++)
            {
                for (int x = 0; x < IsoManager.Width; x++)
                {
                    Tile tile = IsoManager.TileMap[x, y];
                    if (tile.Terrain == terrain)
                    {
                        tile.Draw(spriteBatch, terrainBlendEffect);
                    }
                }
            }
        }


        public int Width => width;
        public int Height => height;
        public Tile[,] TileMap => tileMap;
    }
}