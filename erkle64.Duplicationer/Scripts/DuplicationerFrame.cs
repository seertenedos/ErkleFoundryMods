namespace Duplicationer
{
    internal abstract class DuplicationerFrame : UIFrame
    {
        protected BlueprintToolCHM _tool;

        public bool IsOpen => gameObject.activeSelf;

        public void Setup(BlueprintToolCHM tool)
        {
            _tool = tool;
        }

        protected void Shown()
        {
            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIOpen);

            gameObject.SetActive(true);
            GlobalStateManager.addCursorRequirement();
        }

        public void Hide(bool silent = false)
        {
            if (!gameObject.activeSelf) return;

            if (!silent) AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIClose);

            gameObject.SetActive(false);
            GlobalStateManager.removeCursorRequirement();
        }

        public override void iec_triggerFrameClose()
        {
            Hide();
        }
    }
}
