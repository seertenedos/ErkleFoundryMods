using TMPro;

namespace Portal
{

    public class PortalSetDestinationFrame : UIFrame
    {
        // singleton
        static PortalSetDestinationFrame singleton = null;

        // inspector set
        public TMP_Dropdown dropdown_destination;

        // data
        ulong relatedEntityId = 0;

        public static void showFrame(ulong entityId, PortalSetDestinationFrame prefab)
        {
            if (singleton != null)
                return;

            // add cursor req
            GlobalStateManager.addCursorRequirement();

            // play audio
            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIOpen);

            // spawn frame
            singleton = Instantiate(prefab, GlobalStateManager.getDefaultUICanvasTransform());
            singleton.transform.SetAsLastSibling();

            // set data
            singleton.relatedEntityId = entityId;

            // get all names and set dropdown options
            singleton.dropdown_destination.ClearOptions();
            singleton.dropdown_destination.options.Add(new TMP_Dropdown.OptionData("None"));
            var index = 0;
            var selected = 0;
            var destinationName = PortalSystem.GetPortalDestination(entityId);
            foreach (var name in PortalSystem.GetAllPortalNames())
            {
                index++;
                singleton.dropdown_destination.options.Add(new TMP_Dropdown.OptionData(name));
                if (name == destinationName)
                    selected = index;
            }
            singleton.dropdown_destination.value = selected;
        }

        public static void hideFrame()
        {
            if (singleton == null)
                return;

            // remove cursor req
            GlobalStateManager.removeCursorRequirement();

            // play audio
            AudioManager.playUISoundEffect(ResourceDB.resourceLinker.audioClip_UIClose);

            // remove frame
            Destroy(singleton.gameObject);
            singleton = null;
        }

        public void onClick_save()
        {
            var index = dropdown_destination.value;
            if (index < 0 || index >= dropdown_destination.options.Count)
                return;

            if (index == 0)
            {
                PortalSystem.SetPortalDestination(relatedEntityId, null);
            }
            else
            {
                var name = dropdown_destination.options[index].text;
                if (string.IsNullOrEmpty(name))
                    return;

                PortalSystem.SetPortalDestination(relatedEntityId, name);
            }

            // hide frame
            hideFrame();
        }

        // close trigger
        public void onClick_close()
        {
            hideFrame();
        }

        // iec close trigger
        public override void iec_triggerFrameClose()
        {
            hideFrame();
        }
    }

}