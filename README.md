Simple data binder for UGUI inspired by Nova UI.

To use:
Create a game object, add the list container script onto it.
Add all the prefabs you want to instantiate (Prefab must inherit from ItemView)
Create a class that has reference to list container.
Add your data binders and onclickaction if available.
Just add this line for each prefab you want to bind.
ListContainer.AddDataBinder<TData, TView>()
Then update data using
ListContainer.UpdateData<TData>(List<TData> data)
Make sure that the data you are binding is a base class that all other data binders can inherit from, otherwise it won't work.
