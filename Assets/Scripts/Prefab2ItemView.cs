using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Prefab2ItemView : ItemView
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _icon2;
    public override void Unbind<TData>(TData data)
    {
        
    }

    public override void Bind<TData>(TData data)
    {
        if (data is Prefab2Data prefab2Data)
        {
            _nameText.text = prefab2Data.Name;
            _icon.sprite = prefab2Data.Sprite;
            _icon2.sprite = prefab2Data.Sprite;
        }
    }
}