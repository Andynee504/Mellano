using UnityEngine;

public class ScreenNavBtn : MonoBehaviour
{
    [SerializeField] private ScreenRouter router;
    [SerializeField] private UIScreen targetScreen;
    [SerializeField] private bool openAsRoot = false;

    public void Navigate()
    {
        if (router == null || targetScreen == null)
            return;

        if (openAsRoot)
            router.OpenAsRoot(targetScreen);
        else
            router.Open(targetScreen);
    }
}