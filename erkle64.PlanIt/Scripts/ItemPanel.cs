using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanIt
{

    public class ItemPanel : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TooltipTrigger tooltipTrigger;
        [SerializeField] private TextMeshProUGUI labelText;

        public event System.Action onClicked;

        public void Setup(string tooltip, Sprite icon, double rate)
        {
            iconImage.sprite = icon;
            tooltipTrigger.tooltipText = tooltip;
            SetRate(rate);
        }

        public void SetRate(double rate)
        {
            labelText.text = rate != 0.0 ? rate.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
        }

        public void OnClicked()
        {
            onClicked?.Invoke();
        }
    }

}
