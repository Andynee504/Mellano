using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.VectorGraphics;

[ExecuteAlways]
public class PaletteColorBinder : MonoBehaviour
{
    public ColorPalette palette;
    public string colorId;

    [Header("Targets")]
    public SpriteRenderer spriteRendererTarget;
    public Image imageTarget;
    public SVGImage svgImageTarget;
    public TMP_Text tmpTextTarget;
    public Renderer rendererTarget;

    [Header("Renderer Color Property")]
    public string rendererColorProperty = "_BaseColor";

    public void ApplyColor()
    {
        if (palette == null || string.IsNullOrWhiteSpace(colorId))
            return;

        if (!palette.TryGetColor(colorId, out Color color))
            return;

        if (spriteRendererTarget != null)
            spriteRendererTarget.color = color;

        if (imageTarget != null)
            imageTarget.color = color;

        if (svgImageTarget != null)
            svgImageTarget.color = color;

        if (tmpTextTarget != null)
            tmpTextTarget.color = color;

        if (rendererTarget != null && rendererTarget.sharedMaterial != null)
        {
            Material mat = rendererTarget.sharedMaterial;

            if (mat.HasProperty(rendererColorProperty))
            {
                mat.SetColor(rendererColorProperty, color);
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", color);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyColor();
    }

    private void Reset()
    {
        if (TryGetComponent(out SpriteRenderer sr))
            spriteRendererTarget = sr;

        if (TryGetComponent(out Image img))
            imageTarget = img;

        if (TryGetComponent(out SVGImage svgImg))
            svgImageTarget = svgImg;

        if (TryGetComponent(out TMP_Text text))
            tmpTextTarget = text;

        if (TryGetComponent(out Renderer rend))
            rendererTarget = rend;
    }
#endif
}