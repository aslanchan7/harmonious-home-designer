using UnityEngine;

public static class UISFX
{
    public static void Play(SFXAction action)
    {
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlayGlobal(action);
    }
}
