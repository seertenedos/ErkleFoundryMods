namespace Portal
{

    public class SPP_Portal : ScreenPanelProfile_Native
    {
        public UIText uiText_name;
        public UIText uiText_destination;

        public PortalSetNameFrame portalNameFramePrefab;
        public PortalSetDestinationFrame portalDestinationFramePrefab;

        public override void updatePanel()
        {
            uiText_name.setText($"Name: {PortalSystem.GetPortalName(relatedEntityId) ?? "Unset"}");
            uiText_destination.setText($"Destination: {PortalSystem.GetPortalDestination(relatedEntityId) ?? "Unset"}");
        }

        public void onClick_setName()
        {
            PortalSetNameFrame.showFrame(relatedEntityId, portalNameFramePrefab);
        }

        public void onClick_setDestination()
        {
            PortalSetDestinationFrame.showFrame(relatedEntityId, portalDestinationFramePrefab);
        }

        public void onClick_teleport()
        {
            PortalSystem.TeleportPlayer(relatedEntityId, GameRoot.getClientUsernameHash());
        }
    }

}
