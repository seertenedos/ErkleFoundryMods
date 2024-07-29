using UnityEngine;
using UnityEngine.UI;

public class AdaptablePreferred : LayoutElement, ILayoutElement
{
    [SerializeField] private RectTransform _contentToGrowWith;

    [SerializeField] private bool _usePreferredHeight;
    [SerializeField] private float _preferredHeightMax;

    private float _preferredHeight;

    public override float preferredHeight => _preferredHeight;

    private System.Action<bool> _onLayout;

    public void Setup(RectTransform contentToGrowWith, bool usePreferredHeight, float preferredHeightMax, System.Action<bool> onLayout)
    {
        _contentToGrowWith = contentToGrowWith;
        _usePreferredHeight = usePreferredHeight;
        _preferredHeightMax = preferredHeightMax;
        _onLayout = onLayout;
    }

    public override void CalculateLayoutInputVertical()
    {
        if (_contentToGrowWith == null)
        {
            return;
        }
        if (_usePreferredHeight)
        {
            var height = LayoutUtility.GetPreferredHeight(_contentToGrowWith);
            var isOversize = _preferredHeightMax <= height;
            _preferredHeight = !isOversize ? height : _preferredHeightMax;

            _onLayout?.Invoke(isOversize);
        }
        else
        {
            _preferredHeight = -1;
        }
    }

    public override void CalculateLayoutInputHorizontal()
    {
    }
}