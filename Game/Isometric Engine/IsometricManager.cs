using CarbonField.Game;
using Microsoft.Xna.Framework;

public class IsometricManager
{
    private int width;
    private int height;
    private Tile[,] tileMap;

    public IsometricManager(int width, int height)
    {
        this.width = width;
        this.height = height;
        this.tileMap = new Tile[width, height];
    }

    public void Initialize()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tileMap[x, y] = new Tile(new Vector2(x * Tile.Width, y * Tile.Height));
            }
        }
    }
}
