using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanIt
{

    public class ItemPanelWithInput : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TooltipTrigger tooltipTrigger;
        [SerializeField] private TMP_InputField inputField;

        public event System.Action<double> onRateChanged;
        public event System.Action onClicked;

        public void Setup(string tooltip, Sprite icon, double rate)
        {
            iconImage.sprite = icon;
            tooltipTrigger.tooltipText = tooltip;
            inputField.text = rate.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture);
            inputField.onSubmit.AddListener(OnValueChanged);
        }

        public void OnValueChanged(string value)
        {
            if (double.TryParse(value, out double floatValue))
            {
                onRateChanged?.Invoke(floatValue);
            }
            else
            {
                onRateChanged?.Invoke(0f);
            }
        }

        public void OnClicked()
        {
            onClicked?.Invoke();
        }
    }

}
