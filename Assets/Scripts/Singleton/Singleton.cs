using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    public static T instance;

    public static bool HasInstance => instance != null;

    public static T GetInstance
    {
        get
        {
            if (HasInstance)
                return instance;
            else
            {
                T sceneInstance = FindAnyObjectByType<T>();

                if(sceneInstance != null)
                    return sceneInstance;
                else
                {
                    GameObject go = new GameObject(nameof(T));
                    T createdInstance = go.AddComponent<T>();
                    sceneInstance = createdInstance;
                    return sceneInstance;
                }
            }
        }
    }

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this as T;
    }
}
