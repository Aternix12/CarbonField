using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;


namespace CarbonField
{
	static class EntityManager
	{
		static List<GameObject> entities = new();
		static bool isUpdating;
		static readonly List<GameObject> addedEntities = new();
        public static int EntityCounter { get; set;  }

        public static int Count { get { return entities.Count; } }

		public static void Add(GameObject entity, LightingManager lightingManager)
		{
			if (!isUpdating)
				AddEntity(entity, lightingManager);
			else
				addedEntities.Add(entity);
		}

		private static void AddEntity(GameObject entity, LightingManager lightingManager)
		{
			entities.Add(entity);
            if (entity is IHull hullEntity)
            {
                hullEntity.AddHull(lightingManager);
            }
            EntityCounter++;
		}

		private static void RemoveEntity(GameObject entity)
		{
			entities.Remove(entity);
			EntityCounter--;
		}

		public static void Update(GameTime gameTime, GraphicsDeviceManager graphics, LightingManager lightingManager)
		{
			
			isUpdating = true;
			//Collision Needs to be done
			foreach (var entity in entities)
				entity.Update(gameTime, graphics);
			
			isUpdating = false;

			foreach (var entity in addedEntities)
				AddEntity(entity, lightingManager);

			addedEntities.Clear();

			entities = entities.Where(x => !x.IsExpired).ToList();
			
		}


		public static void Draw(SpriteBatch spriteBatch)
		{
			foreach (var entity in entities)
				entity.Draw(spriteBatch);
		}

		
	}
}