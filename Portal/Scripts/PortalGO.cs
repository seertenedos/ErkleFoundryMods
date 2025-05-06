using UnityEngine;

namespace Portal
{

    public class PortalGO : GenericBuildableObjectGO, IHasScreenPanelManager
    {
        public ScreenPanelManager screenPanelManager;
        public AudioSource teleportSound;
        public ParticleSystem teleportEffect;
        public Vector3 teleportOffset = Vector3.zero;
        public GameObject portal;

        private bool isReal = false;

        public ScreenPanelManager ScreenPanelManager => screenPanelManager;

        public override void init(int renderMode, BuildableObjectTemplate buildableObjectTemplate, ulong relatedEntityId, int itemMode, Chunk anchoredChunk, Color color)
        {
            base.init(renderMode, buildableObjectTemplate, relatedEntityId, itemMode, anchoredChunk, color);

            isReal = renderMode == RenderingMode.NativeEntityGo;
            if (isReal == false)
            {
                var collider = gameObject.GetComponentInChildren<PortalCollider>();
                if (collider != null)
                    Destroy(collider);
            }
        }

        void Update()
        {
            if (isReal == false)
                return;

            var isOn = true;
            var destinationName = PortalSystem.GetPortalDestination(relatedEntityId);
            if (string.IsNullOrEmpty(destinationName))
                isOn = false;

            if (isOn && PortalSystem.HasValidDestination(destinationName, relatedEntityId) == false)
                isOn = false;

            portal.SetActive(isOn);
        }

        public void PlayTeleportEffects(float time)
        {
            if (GlobalStateManager.isDedicatedServer)
                return;

            if (teleportSound != null)
            {
                var startTime = Mathf.Max(0f, Time.time - time);
                if (startTime < teleportSound.clip.length)
                {
                    teleportSound.time = Time.time - time;
                    teleportSound.Play();
                }
            }

            if (teleportEffect != null)
            {
                teleportEffect.Play(true);
                //teleportEffect.time = Mathf.Max(0f, Time.time - time);
            }
        }

        public void TeleportPlayer(Character character)
        {
            PortalSystem.TeleportPlayer(relatedEntityId, character.usernameHash);
        }
    }

}
