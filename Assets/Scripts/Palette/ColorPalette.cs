using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Tools/Color Palette")]
public class ColorPalette : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string id;
        public Color color = Color.white;
    }

    public List<Entry> entries = new();

    public bool TryGetColor(string id, out Color color)
    {
        foreach (var entry in entries)
        {
            if (entry.id == id)
            {
                color = entry.color;
                return true;
            }
        }

        color = Color.white;
        return false;
    }
}