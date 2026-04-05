using UnityEngine;

public class UIScreen : MonoBehaviour
{
    [SerializeField] private string screenTitle = "Screen";
    [SerializeField] private bool showBackButton = true;

    public string ScreenTitle => screenTitle;
    public bool ShowBackButton => showBackButton;

    public virtual void Show()
    {
        gameObject.SetActive(true);
        OnEnter();
    }

    public virtual void Hide()
    {
        OnExit();
        gameObject.SetActive(false);
    }

    protected virtual void OnEnter() { }
    protected virtual void OnExit() { }
}