using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanIt
{

    public class ItemPanelWithPower : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TooltipTrigger tooltipTrigger;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI powerText;

        public event System.Action onClicked;

        public void Setup(string tooltip, Sprite icon, double rate, double power)
        {
            iconImage.sprite = icon;
            tooltipTrigger.tooltipText = tooltip;
            SetRate(rate);
            SetPower(power);
        }

        public void SetRate(double rate)
        {
            labelText.text = rate != 0.0 ? rate.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture) : string.Empty;
        }

        public void SetPower(double power)
        {
            string powerString;
            if (power >= 10000000000.0)
            {
                powerString = $"{power / 1000000000.0:0.#}TW";
            }
            else if (power >= 10000000.0)
            {
                powerString = $"{power / 1000000.0:0.#}GW";
            }
            else if (power >= 10000.0)
            {
                powerString = $"{power / 1000.0:0.#}MW";
            }
            else
            {
                powerString = $"{Mathf.RoundToInt((float)power)}KW";
            }

            powerText.text = powerString;
        }

        public void OnClicked()
        {
            onClicked?.Invoke();
        }
    }

}
