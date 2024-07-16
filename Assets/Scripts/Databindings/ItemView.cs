using System;
using UnityEngine;
using UnityEngine.UI;


public abstract class ItemView : MonoBehaviour
{
    public Button Button;
    public abstract void Unbind<TData>(TData data);
    public abstract void Bind<TData>(TData data);

    public virtual void AddClickAction(Action clickAction)
    {
        Button.onClick.AddListener(() =>
        {
            clickAction?.Invoke();
        });
    }

    public virtual void RemoveClickAction(Action clickAction)
    {
        Button.onClick.RemoveListener(() =>
        {
            clickAction?.Invoke();
        });
    }
}