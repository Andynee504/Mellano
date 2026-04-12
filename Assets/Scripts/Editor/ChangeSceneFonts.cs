using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class ChangeSceneFonts : EditorWindow
{
    public TMP_FontAsset tmpFont;
    public Font legacyFont;

    [MenuItem("Tools/Change Scene Fonts")]
    public static void ShowWindow()
    {
        GetWindow<ChangeSceneFonts>("Change Scene Fonts");
    }

    private void OnGUI()
    {
        GUILayout.Label("Trocar tipografia da cena", EditorStyles.boldLabel);

        tmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font", tmpFont, typeof(TMP_FontAsset), false);
        legacyFont = (Font)EditorGUILayout.ObjectField("Legacy Font", legacyFont, typeof(Font), false);

        if (GUILayout.Button("Aplicar em toda a cena"))
        {
            ApplyFonts();
        }
    }

    private void ApplyFonts()
    {
        int count = 0;
        
        TMP_Text[] tmpTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text text in tmpTexts)
        {
            Undo.RecordObject(text, "Change TMP Font");
            if (tmpFont != null)
            {
                text.font = tmpFont;
                EditorUtility.SetDirty(text);
                count++;
            }
        }

        Text[] legacyTexts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Text text in legacyTexts)
        {
            Undo.RecordObject(text, "Change Legacy Font");
            if (legacyFont != null)
            {
                text.font = legacyFont;
                EditorUtility.SetDirty(text);
                count++;
            }
        }

        Debug.Log($"Fontes alteradas em {count} objetos.");
    }
}