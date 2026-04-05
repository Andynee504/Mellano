using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScreenRouter : MonoBehaviour
{
    [SerializeField] private UIScreen initialScreen;
    [SerializeField] private List<UIScreen> screens = new();

    [Header("Static UI")]
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text titleText;

    private readonly Stack<UIScreen> history = new();
    private UIScreen currentScreen;

    private void Awake()
    {
        foreach (var screen in screens)
        {
            if (screen != null)
                screen.gameObject.SetActive(false);
        }

        if (backButton != null)
            backButton.onClick.AddListener(Back);
    }

    private void Start()
    {
        OpenAsRoot(initialScreen);
    }

    public void Open(UIScreen target)
    {
        SwitchTo(target, true);
    }

    public void OpenAsRoot(UIScreen target)
    {
        history.Clear();
        SwitchTo(target, false);
    }

    public void Back()
    {
        if (history.Count == 0)
            return;

        if (currentScreen != null)
            currentScreen.Hide();

        currentScreen = history.Pop();
        currentScreen.Show();

        RefreshStaticUI();
    }

    private void SwitchTo(UIScreen target, bool pushHistory)
    {
        if (target == null || target == currentScreen)
            return;

        if (currentScreen != null)
        {
            if (pushHistory)
                history.Push(currentScreen);

            currentScreen.Hide();
        }

        currentScreen = target;
        currentScreen.Show();

        RefreshStaticUI();
    }

    private void RefreshStaticUI()
    {
        if (titleText != null)
            titleText.text = currentScreen != null ? currentScreen.ScreenTitle : "";

        if (backButton != null)
        {
            bool canGoBack = currentScreen != null
                             && currentScreen.ShowBackButton
                             && history.Count > 0;

            backButton.gameObject.SetActive(canGoBack);
        }
    }
}