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
			UpdateCollision(gameTime);
			foreach (var entity in entities)
				entity.Update(gameTime, graphics);
			
			isUpdating = false;

			foreach (var entity in addedEntities)
				AddEntity(entity);

			addedEntities.Clear();

			entities = entities.Where(x => !x.IsExpired).ToList();
			
		}

		public static void UpdateCollision(GameTime gameTime)
        {
			foreach(var objA in entities)
            {
				foreach(var objB in entities)
                {
					if (objA == objB)
						continue;
					if (objA.Intersects(objB))
						objA.OnCollide(objB);
					else
                    {
						objA.collisionwait = true;
						objB.collisionwait = true;
                    }
                }
            }

			for (int i = 0; i < Count; i++)
			{
				foreach (var child in entities[i].Children)
					entities.Add(child);

				entities[i].Children.Clear();
			}

			for (int i = 0; i < Count; i++)
			{
				if (entities[i].IsExpired)
				{
					entities.RemoveAt(i);
					i--;
				}
			}
		}

		public static void Draw(SpriteBatch spriteBatch)
		{
			foreach (var entity in entities)
				entity.Draw(spriteBatch);
		}

		
	}
}