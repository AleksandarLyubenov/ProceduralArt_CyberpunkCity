using UnityEngine;

public static class SafeDestroy
{
    public static void DestroyObject(Object obj)
    {
        if (obj == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            Object.DestroyImmediate(obj);
        else
            Object.Destroy(obj);
#else
        Object.Destroy(obj);
#endif
    }
}
