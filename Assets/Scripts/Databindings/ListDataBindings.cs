using System;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
namespace DataBinding
{
    internal class ObjectPoolWrapper
    {
        private IObjectPool<ItemView> BoxedObjectPool;
        private Type ObjectPoolType;
        private ItemView _prefab;
        private Transform _container;
        private List<ItemView> _itemViews = new();
        public ObjectPoolWrapper(ItemView prefab, Transform container)
        {
            _prefab = prefab;
            _container = container;
            var prefabType = prefab.GetType();
            ObjectPoolType = prefabType;
            BoxedObjectPool = new ObjectPool<ItemView>(CreateItem, TakeItem, ReturnedItem, DestroyedItem);
        }
        internal bool TryGetItem(Type prefabType, out ItemView result)
        {
            result = null;
            if (prefabType == ObjectPoolType)
            {
                result = BoxedObjectPool.Get();
                return true;
            }
            return false;
        }

        internal bool TryReleaseItem(ItemView item)
        {
            if (item.GetType() == ObjectPoolType)
            {
                BoxedObjectPool.Release(item);
                return true;
            }
            return false;
        }
        private ItemView CreateItem()
        {
            var item = Object.Instantiate(_prefab, _container);
            _itemViews.Add(item);
            return item;
        }
        private void TakeItem(ItemView itemView)
        {
            itemView.gameObject.SetActive(true);
        }
        private void ReturnedItem(ItemView itemView)
        {
            itemView.gameObject.SetActive(false);
        }
        private void DestroyedItem(ItemView itemView)
        {
            Object.Destroy(itemView.gameObject);
        }

        public void Clear()
        {
            foreach (var item in _itemViews)
            {
                BoxedObjectPool.Release(item);
            }
        }
    }
    internal class PrefabPool
    {
        private Dictionary<Type, DataBinder> dataBinders = new Dictionary<Type, DataBinder>()
        {
            {typeof(object), new DataBinder<object>() }
        };
        /// <summary>
        /// The pool under which to store pooled prefabs
        /// </summary>
        private Transform _prefabContainer = null;
        /// <summary>
        /// List of prefabs.
        /// </summary>
        private readonly List<ItemView> _listItemPrefabs = new();
        /// <summary>
        /// Dictionary to store data type that corresponds to prefab type.
        /// Type1: Data,
        /// Type2: View
        /// </summary>
        private readonly Dictionary<Type, Type> _prefabToDataTypeBinders = new();
        /// <summary>
        /// Dictionary to store each prefab corresponding to it's view type.
        /// Type: View.
        /// </summary>
        private readonly Dictionary<Type, ItemView> _prefabs = new();
        /// <summary>
        /// Dictionary to store each prefab pool corresponding to it's view type.
        /// Type: View
        /// </summary>
        private readonly Dictionary<Type, ObjectPoolWrapper> _prefabPools = new();
        private bool _initialized;
        internal void Init(Transform prefabOwner, List<ItemView> prefabSources)
        {
            if (_initialized)
            {
                return;
            }
            _prefabContainer = prefabOwner;
            for (int i = 0; i < prefabSources.Count; ++i)
            {
                ItemView listItemPrefab = prefabSources[i];
                if (listItemPrefab == null)
                {
                    continue;
                }
                if (_listItemPrefabs.Contains(listItemPrefab))
                {
                    // Duplicate
                    continue;
                }
                _listItemPrefabs.Add(listItemPrefab);
                _prefabs.Add(listItemPrefab.GetType(), listItemPrefab);
                _prefabPools.Add(listItemPrefab.GetType(), new (listItemPrefab, _prefabContainer));
            }
            _initialized = true;
        }
        
        internal void AddPrefabToDataTypeMapping<TData, TPrefab>() where TPrefab : ItemView
        {
            Type dataType = typeof(TData);
            
            if (_prefabToDataTypeBinders.TryGetValue(dataType, out var prefabTypeOut)) return;
            _prefabToDataTypeBinders[dataType] = typeof(TPrefab);
            if (!dataBinders.ContainsKey(dataType))
            {
                dataBinders.Add(dataType, new DataBinder<TData>());
            }
        }
        internal void RemovePrefabToDataTypeMapping<TData, TPrefab>() where TPrefab : ItemView
        {
            Type dataType = typeof(TData);
            if (_prefabToDataTypeBinders.TryGetValue(dataType, out var prefabTypeOut)) return;
            _prefabToDataTypeBinders.Remove(typeof(TPrefab));
            if (!dataBinders.ContainsKey(dataType))
            {
                dataBinders.Remove(dataType);
            }
        }
        internal ItemView GetPrefabInstance(Type dataType)
        {
            var prefabType = _prefabToDataTypeBinders[dataType];
            ObjectPoolWrapper pool = GetPool(prefabType);
            if (pool.TryGetItem(prefabType, out ItemView item))
            {
                return item;
            }
            throw new KeyNotFoundException($"Couldn't get item with specified index.");
        }

        internal DataBinder GetDataBinder(Type T)
        {
            if (dataBinders.TryGetValue(T, out DataBinder db))
            {
                return db;
            }

            return dataBinders[typeof(object)];
        }
        internal void Clear()
        {
            foreach (var pool in _prefabPools)
            {
                pool.Value.Clear();
            }
        }
        private ObjectPoolWrapper GetPool(Type prefabType)
        {
            if (_prefabPools.TryGetValue(prefabType, out var pool))
            {
                return pool;
            }
            throw new KeyNotFoundException($"Couldn't find pool for the item view of type {prefabType.Name}");
        }

        public void Release(ItemView itemView)
        {
            var pool = GetPool(itemView.GetType());
            pool.TryReleaseItem(itemView);
        }
    }
    internal abstract class DataBinder
    {
        public abstract void InvokeBind(ItemView prefab, ListWrapper data, int index);
        public abstract void InvokeUnbind(ItemView prefab, ListWrapper data, int index);
    }
    internal class DataBinder<TData> : DataBinder
    {
        public override void InvokeBind(ItemView prefab, ListWrapper data, int index)
        {
            if (!data.TryGet(index, out TData userData) && index >= 0 && index < data.Count)
            {
                Debug.LogError("Mismatch between prefab and data type. Unable to invoke OnBind event");
                return;
            }
            prefab.gameObject.SetActive(true);
            prefab.Bind(userData);
        }
        public override void InvokeUnbind(ItemView prefab, ListWrapper data, int index)
        {
            if (!data.TryGet(index, out TData userData) && index >= 0 && index < data.Count)
            {
                Debug.LogError("Mismatch between prefab and data type. Unable to invoke OnUnbind event");
                return;
            }
            prefab.Unbind(userData);
        }
    }

    internal class ListWrapper<TData> : ListWrapper
    {
        public override Type ElementType => typeof(TData);
        public override int Count => DataSource?.Count ?? 0;
        public List<TData> DataSource;
        
        private static Queue<ListWrapper<TData>> _pool = new();
        public static ListWrapper<TData> Wrap(List<TData> source)
        {
            ListWrapper<TData> wrapper = _pool.Count > 0 ? _pool.Dequeue() : new ListWrapper<TData>();
            wrapper.DataSource = source;
            return wrapper;
        }
        public override Type GetDataType(int index)
        {
            Type listElementType = typeof(TData);
            var item = TryGet(index, out TData val);
            if (!item) return listElementType;
            return val.GetType();
        }

        public override bool TryGet<T>(int index, out T item)
        {
            item = default;

            if (IsEmpty)
            {
                return false;
            }

            if (DataSource is List<T> typedSource)
            {
                if (index >= 0 && index < Count)
                {
                    item = typedSource[index];
                    return true;
                }
            }

            TData internalItem = Get(index);

            if (internalItem is T castedItem)
            {
                item = castedItem;
                return true;
            }
            else
            {
                // if val is null, that's fine.
                // if val is non-null but not type T, then we have a type mismatch
                return internalItem == null;
            }
        }

        public TData Get(int index)
        {
            return Get(index, DataSource);
        }

        private static TData Get(int index, IList<TData> source)
        {
            if (index >= 0 && index < source.Count)
            {
                return source[index];
            }
            throw new IndexOutOfRangeException();
        }
        public override List<T> GetSource<T>() => DataSource as List<T>;

        public override IEnumerator GetEnumerator() => DataSource.GetEnumerator();

        public override void Dispose()
        {
            base.Dispose();
            _pool.Enqueue(this);
        }
    }
    /// <summary>
    /// Wrapper class to encapsulate arbitiary data lists.
    /// </summary>
    internal abstract class ListWrapper : ICollectionWrapper<int>, IDisposable
    {
        public bool IsEmpty => Count == 0;
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        public abstract Type ElementType { get; }
        public abstract int Count { get; }
        public bool IsSynchronized { get; }
        public object SyncRoot { get; }
        public abstract Type GetDataType(int index);
        public abstract bool TryGet<TData>(int index, out TData item);
        public abstract List<T> GetSource<T>();
        public virtual void Dispose() { }
        public abstract IEnumerator GetEnumerator();
    }
    /// <summary>
    /// Wraps an arbitrary data structure with a key type of <see cref="TKey"/>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal interface ICollectionWrapper<TKey> : ICollection
    {
        Type GetDataType(TKey key);
        bool TryGet<TData>(TKey key, out TData item);
    }
}
