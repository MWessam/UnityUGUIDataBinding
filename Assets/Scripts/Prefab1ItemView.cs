using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Prefab1ItemView : ItemView
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _icon;
    public override void Unbind<TData>(TData data)
    {
        
    }

    public override void Bind<TData>(TData data)
    {
        if (data is Prefab1Data prefab1Data)
        {
            _nameText.text = prefab1Data.Name;
            _icon.sprite = prefab1Data.Sprite;
        }
    }
}



[Serializable]
public class Prefab1Data : BasePrefabData
{
    public string Name;
    public Sprite Sprite;
}
[Serializable]
public class Prefab2Data : BasePrefabData
{
    public string Name;
    public Sprite Sprite;
    public Sprite Sprite2;
}

[Serializable]
public class BasePrefabData
{
}