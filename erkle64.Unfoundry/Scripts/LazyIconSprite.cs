using UnityEngine;

namespace Unfoundry
{
    public class LazyIconSprite
    {
        private Sprite sprite = null;
        private string iconName;

        public LazyIconSprite(string iconName)
        {
            this.iconName = iconName;
        }

        private Sprite FetchSprite()
        {
            sprite = ResourceDB.getIcon(iconName);
            if (sprite == null) Debug.LogWarning((string)$"Failed to find icon '{iconName}'");

            return sprite;
        }

        public Sprite Sprite => sprite == null ? FetchSprite() : sprite;
    }
}
