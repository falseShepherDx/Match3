using UnityEngine;

public static class ItemDataBase 
{
    public static Item[] Items {  get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]private static void Initalize() => Items = Resources.LoadAll<Item>("Items/");

}
