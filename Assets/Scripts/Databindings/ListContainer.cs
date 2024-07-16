
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding
{
    public class ListContainer : MonoBehaviour
    {
        [SerializeField] private Transform _container;
        [SerializeField] private List<ItemView> _prefabs;
        private PrefabPool _pool;
        private ListWrapper _dataSource;
        private List<ItemView> _instances = new();

        /// <summary>
        /// Type: View
        /// Returns: Action(data, view);
        /// </summary>
        private Dictionary<Type, Delegate> _clickActions = new();
        #region MEMBER
        public void SetClickAction<TData, TView>(Action<TData, int, TView> clickAction) where TView : ItemView
        {
            try
            {
                var typeKey = typeof(TData);
                _clickActions[typeKey] = clickAction;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }
        public void UpdateData<T>(List<T> data)
        {
            ClearAll<T>();
            _dataSource = ListWrapper<T>.Wrap(data);
            for (var i = 0; i < data.Count; i++)
            {
                // var item = data[i];
                // var dataType = data.GetType();
                // var pool = _pools[dataType];
                // var itemView = _pool.Get() as TView;
                // var iCopy = i;
                // itemView.Bind(item);
                // // itemView.AddClickAction(AddClickAction<T, TView>(data[i], iCopy, itemView));
                // _itemViews.Add(itemView);
                if (_dataSource.TryGet<T>(i, out var item))
                {
                    var itemType = item.GetType();
                    ItemView itemView = _pool.GetPrefabInstance(itemType);
                    BindItem(item, itemView, i);
                    var i1 = i;
                    itemView.AddClickAction(() =>
                    {
                        _clickActions[itemType]?.DynamicInvoke(item,i1, itemView);
                    });
                    itemView.transform.SetAsFirstSibling();
                    _instances.Add(itemView);
                }
            }
        }

        public void AddDataBinder<TData, TView>() where TView : ItemView
        {
            _pool.AddPrefabToDataTypeMapping<TData, TView>();
        }
        public void RemoveDataBinder<TData, TView>() where TView : ItemView
        {
            _pool.RemovePrefabToDataTypeMapping<TData, TView>();
        }
        private void BindItem(object data, ItemView item, int index)
        {
            var dataType = data.GetType();
            var dataBinder = _pool.GetDataBinder(dataType);
            dataBinder.InvokeBind(item, _dataSource, index);
        }
        private void UnBindItem(object data, ItemView item, int index)
        {
            var dataType = data.GetType();
            var dataBinder = _pool.GetDataBinder(dataType);
            dataBinder.InvokeUnbind(item, _dataSource, index);
        }
        #endregion
        
        #region ENGINE
        private void Awake()
        {
            _pool = new PrefabPool();
            _pool.Init(_container, _prefabs);
        }
        #endregion

        #region INTERNAL
        private void ClearAll<T>()
        {
            if (_dataSource == null) return;
            for (int i = 0; i < _dataSource.Count; ++i)
            {
                if (_dataSource.TryGet<T>(i, out var item))
                {
                    var itemType = item.GetType();
                    ItemView itemView = _instances[i];
                    int i1 = i;
                    itemView.RemoveClickAction(() =>
                    {
                        _clickActions[itemType]?.DynamicInvoke(item,i1, itemView);
                    });
                    UnBindItem(item, itemView, i);
                    _pool.Release(itemView);

                }
            }
            _instances.Clear();
            if (_dataSource != null)
            {
                _dataSource.Dispose();
            }
        }
        #endregion
    }
}