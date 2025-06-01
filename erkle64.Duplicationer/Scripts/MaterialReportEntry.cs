using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MaterialReportEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textLabel;
    [SerializeField] private TextMeshProUGUI _textRemaining;
    [SerializeField] private TextMeshProUGUI _textInventory;
    [SerializeField] private TextMeshProUGUI _textDone;
    [SerializeField] private TextMeshProUGUI _textTotal;
    [SerializeField] private TooltipTrigger _tooltipRemaining;
    [SerializeField] private TooltipTrigger _tooltipInventory;
    [SerializeField] private TooltipTrigger _tooltipDone;
    [SerializeField] private TooltipTrigger _tooltipTotal;
    [SerializeField] private Button _button;
    [SerializeField] private Image _imageBorderBottom;

    public delegate void MaterialReportEntryClicked(MaterialReportEntry entry);
    private event MaterialReportEntryClicked OnClicked;

    public void Setup(string label, int inventory, int done, int total, MaterialReportEntryClicked onClicked = null)
    {
        _textLabel.text = label;
        _textRemaining.text = (done >= total) ? "<sprite index=0>" : (total - done).ToString();
        _textInventory.text = (inventory <= 0) ? string.Empty : inventory.ToString();
        _textDone.text = (done <= 0) ? string.Empty : done.ToString();
        _textTotal.text = (total <= 0) ? string.Empty : total.ToString();
        _tooltipRemaining.tooltipText = (done >= total) ? $"All {label} completed." : $"{label} x{total - done} remaining to place.";
        _tooltipInventory.tooltipText = (inventory <= 0) ? $"No {label} in inventory." : $"{label} x{inventory} in inventory.";
        _tooltipDone.tooltipText = (done <= 0) ? $"No {label} completed." : $"{label} x{done} completed.";
        _tooltipTotal.tooltipText = $"{label} x{total} total required for blueprint.";
        bool isItemLine = onClicked != null;
        _imageBorderBottom.enabled = isItemLine;
        _tooltipRemaining.enabled = isItemLine;
        _tooltipInventory.enabled = isItemLine;
        _tooltipDone.enabled = isItemLine;
        _tooltipTotal.enabled = isItemLine;
        _button.interactable = isItemLine;
        OnClicked = onClicked;
    }

    public void OnClick()
    {
        OnClicked?.Invoke(this);
    }
}
