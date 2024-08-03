using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanIt
{

    public class ItemPanelWithConveyor : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TooltipTrigger tooltipTrigger;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Image conveyorIconImage;
        [SerializeField] private TextMeshProUGUI conveyorText;

        public event System.Action onClicked;

        public void Setup(string tooltip, Sprite icon, double rate, Sprite conveyorIcon, double conveyorAmount)
        {
            iconImage.sprite = icon;
            tooltipTrigger.tooltipText = tooltip;
            SetRate(rate);
            SetConveyor(conveyorIcon, conveyorAmount);
        }

        public void SetRate(double rate)
        {
            labelText.text = rate != 0.0 ? rate.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
        }

        public void SetConveyor(Sprite icon, double amount)
        {
            conveyorIconImage.sprite = icon;
            conveyorText.text = amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        }

        public void OnClicked()
        {
            onClicked?.Invoke();
        }
    }

}
