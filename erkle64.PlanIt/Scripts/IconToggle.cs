using UnityEngine;
using UnityEngine.UI;

namespace PlanIt
{

    public class IconToggle : MonoBehaviour
    {
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image iconImage;
        [SerializeField] private TooltipTrigger tooltipTrigger;

        public bool interactable
        {
            get => toggle.interactable;
            set => toggle.interactable = value;
        }

        public bool isOn
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
        }

        public void Setup(Sprite icon, string tooltip)
        {
            iconImage.sprite = icon;
            tooltipTrigger.tooltipText = tooltip;
        }
    }

}