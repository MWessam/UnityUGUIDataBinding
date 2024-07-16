using System.Collections;
using System.Collections.Generic;
using DataBinding;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    [SerializeField] private ListContainer _listContainer;
    [SerializeField] private List<Prefab1Data> _prefab1Data;
    [SerializeField] private List<Prefab2Data> _prefab2Datas;

    private List<BasePrefabData> _prefabDatas;
    // Start is called before the first frame update
    void Start()
    {
        _prefabDatas = new List<BasePrefabData>(_prefab1Data);
        _prefabDatas.AddRange(_prefab2Datas);
        _listContainer.AddDataBinder<Prefab1Data, Prefab1ItemView>();
        _listContainer.AddDataBinder<Prefab2Data, Prefab2ItemView>();
        _listContainer.SetClickAction<Prefab1Data, Prefab1ItemView>(ClickPrefab1);
        _listContainer.SetClickAction<Prefab2Data, Prefab2ItemView>(ClickPrefab2);
        _listContainer.UpdateData(_prefabDatas);
    }

    private void ClickPrefab2(Prefab2Data data, int index, Prefab2ItemView view)
    {
        Debug.Log($"Clicked 2");
    }

    private void ClickPrefab1(Prefab1Data data, int index, Prefab1ItemView view)
    {
        Debug.Log($"Clicked 1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
