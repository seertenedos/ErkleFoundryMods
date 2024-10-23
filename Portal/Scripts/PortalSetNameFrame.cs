using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Portal
{

    public class PortalSetNameFrame : UIFrame
    {
        // singleton
        static PortalSetNameFrame singleton = null;

        // inspector set
        public TMP_InputField inputField_name;
        public TextMeshProUGUI uiText_buttonText;
        public Button uiButton_save;

        // data
        ulong relatedEntityId = 0;

        public static void showFrame(ulong entityId, PortalSetNameFrame prefab)
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

            // get name and set input field
            var portalName = PortalSystem.GetPortalName(entityId) ?? string.Empty;
            singleton.inputField_name.text = portalName;
        }

        bool firstFrame = true;
        void Update()
        {
            if (firstFrame == true)
            {
                // select input field
                inputField_name.gameObject.GetComponent<Selectable>().select_advanced(EventSystem.current);
                firstFrame = false;
            }
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

        public void onChange_inputField()
        {
            // do error validation
            var name = inputField_name.text;
            if (name.Length == 0)
            {
                uiText_buttonText.text = "Name cannot be empty";
                uiButton_save.interactable = false;
            }
            else if (name.Length > 128 || Encoding.UTF8.GetByteCount(name) > 128)
            {
                uiText_buttonText.text = "Name too long";
                uiButton_save.interactable = false;
            }
            else
            {
                uiText_buttonText.text = "Save";
                uiButton_save.interactable = true;
            }
        }

        public void onClick_save()
        {
            var name = inputField_name.text;
            if (string.IsNullOrEmpty(name))
                return;

            if (name.Length > 128)
                name = name.Substring(0, 128);

            PortalSystem.SetPortalName(relatedEntityId, name);

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