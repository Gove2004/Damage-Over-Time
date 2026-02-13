using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class TMPFontAutoFill
{
    private const string FontAssetPath = "Assets/Resources/AlibabaPuHuiTi-3-55 SDF.asset";
    private const string SourceFontPath = "Assets/Resources/AlibabaPuHuiTi-3-55.ttf";
    private const string CardsCsvPath = "Assets/Resources/cards.csv";
    private const string PrefKey = "TMPFontAutoFill.Done";

    [InitializeOnLoadMethod]
    private static void AutoFillOnce()
    {
        if (EditorPrefs.GetBool(PrefKey, false))
        {
            return;
        }

        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (fontAsset == null || sourceFont == null)
        {
            return;
        }

        FillFont();
        EditorPrefs.SetBool(PrefKey, true);
    }

    [MenuItem("Tools/TMP/补全字体字库")]
    public static void FillFont()
    {
        var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        var sourceFont = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (fontAsset == null || sourceFont == null)
        {
            Debug.LogError("未找到字体资源或源字体文件");
            return;
        }

        if (fontAsset.atlasPopulationMode != AtlasPopulationMode.Dynamic)
        {
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        }

        var characters = new HashSet<char>();
        var cardsText = File.Exists(CardsCsvPath) ? File.ReadAllText(CardsCsvPath, Encoding.UTF8) : string.Empty;
        AddCharacters(characters, cardsText);
        AddCharacters(characters, GetCommonCharacters());

        var toAdd = new string(characters.Where(c => !char.IsSurrogate(c)).ToArray());
        fontAsset.TryAddCharacters(toAdd, out var missing);
        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"字体补全完成，已处理字符数: {toAdd.Length}，缺失字符数: {(missing == null ? 0 : missing.Length)}");
    }

    private static void AddCharacters(HashSet<char> set, string text)
    {
        foreach (var c in text)
        {
            if (c == '\0')
            {
                continue;
            }

            set.Add(c);
        }
    }

    private static string GetCommonCharacters()
    {
        return " 0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" +
               "，。！？；：、,.!?;:「」『』【】（）()《》〈〉“”‘’—-·…%￥¥$#@&*+=/\\|^~`" +
               "×÷＝+=-*/<>[]{}" +
               "％‰";
    }
}
