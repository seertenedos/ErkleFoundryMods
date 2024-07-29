using C3.ModKit;

namespace Unfoundry
{
    public abstract class UnfoundryPlugin
    {
        public virtual void Load(Mod mod) { }
        public virtual void GameEnter() { }
        public virtual void GameExit() { }
    }
}
