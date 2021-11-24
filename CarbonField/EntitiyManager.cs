using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace CarbonField
{
	static class EntityManager
	{
		static List<GameObject> entities = new List<GameObject>();


		static bool isUpdating;
		static List<GameObject> addedEntities = new List<GameObject>();

		public static int Count { get { return entities.Count; } }

		public static void Add(GameObject entity)
		{
			if (!isUpdating)
				AddEntity(entity);
			else
				addedEntities.Add(entity);
		}

		private static void AddEntity(GameObject entity)
		{
			entities.Add(entity);
		}

		public static void Update(GameTime gameTime, GraphicsDeviceManager graphics)
		{
			isUpdating = true;
			HandleCollisions();

			foreach (var entity in entities)
				entity.Update(gameTime, graphics);

			isUpdating = false;

			foreach (var entity in addedEntities)
				AddEntity(entity);

			addedEntities.Clear();

			entities = entities.Where(x => !x.IsExpired).ToList();
		}

		static void HandleCollisions()
		{
			// handle collisions between enemies
			/*
			for (int i = 0; i < enemies.Count; i++)
				for (int j = i + 1; j < enemies.Count; j++)
				{
					if (IsColliding(enemies[i], enemies[j]))
					{
						enemies[i].HandleCollision(enemies[j]);
						enemies[j].HandleCollision(enemies[i]);
					}
				}

			// handle collisions between bullets and enemies
			for (int i = 0; i < enemies.Count; i++)
				for (int j = 0; j < bullets.Count; j++)
				{
					if (IsColliding(enemies[i], bullets[j]))
					{
						enemies[i].WasShot();
						bullets[j].IsExpired = true;
					}
				}

			// handle collisions between the player and enemies
			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies[i].IsActive && IsColliding(PlayerShip.Instance, enemies[i]))
				{
					KillPlayer();
					break;
				}
			}

			// handle collisions with black holes
			for (int i = 0; i < blackHoles.Count; i++)
			{
				for (int j = 0; j < enemies.Count; j++)
					if (enemies[j].IsActive && IsColliding(blackHoles[i], enemies[j]))
						enemies[j].WasShot();

				for (int j = 0; j < bullets.Count; j++)
				{
					if (IsColliding(blackHoles[i], bullets[j]))
					{
						bullets[j].IsExpired = true;
						blackHoles[i].WasShot();
					}
				}

				if (IsColliding(PlayerShip.Instance, blackHoles[i]))
				{
					KillPlayer();
					break;
				}
			}*/
		}

		private static bool IsColliding(GameObject a, GameObject b)
		{
			//This needs to be reworked!!
			/*
			float radius = a.Radius + b.Radius;
			return !a.IsExpired && !b.IsExpired && Vector2.DistanceSquared(a.Position, b.Position) < radius * radius;
			*/
			return false;
		}

		/*
		public static IEnumerable<GameObject> GetNearbyEntities(Vector2 position, float radius)
		{
			return entities.Where(x => Vector2.DistanceSquared(position, x.Position) < radius * radius);
		}
		*/

		public static void Draw(SpriteBatch spriteBatch)
		{
			foreach (var entity in entities)
				entity.Draw(spriteBatch);
		}
	}
}