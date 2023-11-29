using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace CarbonField
{
    public class EntityManager
    {
        private readonly List<GameObject> entities = new();
        private readonly List<GameObject> addedEntities = new();
        private bool isUpdating;

        public int EntityCounter { get; private set; }
        public int Count => entities.Count;

        public void Add(GameObject entity, LightingManager lightingManager)
        {
            if (!isUpdating)
                AddEntity(entity, lightingManager);
            else
                addedEntities.Add(entity);
        }

        private void AddEntity(GameObject entity, LightingManager lightingManager)
        {
            entities.Add(entity);
            if (entity is IHull hullEntity)
            {
                hullEntity.AddHull(lightingManager);
            }
            EntityCounter++;
        }

        public void Update(GameTime gameTime, GraphicsDeviceManager graphics, LightingManager lightingManager)
        {
            isUpdating = true;
            foreach (var entity in entities)
                entity.Update(gameTime, graphics);

            isUpdating = false;

            foreach (var entity in addedEntities)
                AddEntity(entity, lightingManager);

            addedEntities.Clear();

            entities.RemoveAll(x => x.IsExpired);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var entity in entities)
                entity.Draw(spriteBatch);
        }
    }
}
