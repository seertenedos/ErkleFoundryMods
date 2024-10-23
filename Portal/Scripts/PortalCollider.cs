using UnityEngine;

namespace Portal
{

    public class PortalCollider : MonoBehaviour
    {
        public PortalGO portalGO;

        void OnTriggerEnter(Collider other)
        {
            var renderCharacter = other.GetComponent<RenderCharacter>();
            if (renderCharacter == null)
                return;

            var character = renderCharacter.relatedCharacter;
            if (character == null)
                return;

            if (character != GameRoot.getClientCharacter())
                return;

            portalGO.TeleportPlayer(character);
        }
    }

}
