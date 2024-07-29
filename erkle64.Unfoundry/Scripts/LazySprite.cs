using C3;
using System.IO;
using UnityEngine;

namespace Duplicationer
{
    public class LazySprite
    {
        private string assetPath;

        private Sprite sprite = null;

        public LazySprite(string assetPath)
        {
            this.assetPath = assetPath;
        }

        public Sprite Sprite
        {
            get
            {
                if (sprite != null) return sprite;
                sprite = AssetManager.Database.LoadAssetAtPath<Sprite>(assetPath);
                if (sprite == null) throw new FileLoadException(assetPath);
                return sprite;
            }
        }
    }
}
