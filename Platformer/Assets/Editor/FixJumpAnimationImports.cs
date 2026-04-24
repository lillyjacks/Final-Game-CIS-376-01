using UnityEditor;
using UnityEngine;

public static class FixJumpAnimationImports
{
    [MenuItem("Tools/Player Animation/Fix Jump Clip Root Motion")]
    public static void FixJumpClipRootMotion()
    {
        string[] clipPaths =
        {
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Jump/HumanM@Jump01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement/Jump/HumanM@Fall01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Female/Movement/Jump/HumanF@Jump01.fbx",
            "Assets/Kevin Iglesias/Human Animations/Animations/Female/Movement/Jump/HumanF@Fall01.fbx"
        };

        int updatedCount = 0;
        foreach (string clipPath in clipPaths)
        {
            if (ApplyClipSettings(clipPath))
            {
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Processed {updatedCount} jump/fall animation importers.");
    }

    private static bool ApplyClipSettings(string assetPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogWarning($"Could not load importer for {assetPath}");
            return false;
        }

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            clips = importer.defaultClipAnimations;
        }

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"No clips found inside {assetPath}");
            return false;
        }

        bool changed = false;
        for (int i = 0; i < clips.Length; i++)
        {
            ModelImporterClipAnimation clip = clips[i];
            bool clipChanged = false;

            clip.lockRootPositionXZ = true;
            clip.lockRootHeightY = true;
            clip.keepOriginalPositionXZ = false;
            clip.keepOriginalPositionY = false;
            clip.heightFromFeet = false;

            clips[i] = clip;
            clipChanged = true;
            changed |= clipChanged;
        }

        if (!changed)
        {
            return false;
        }

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
        Debug.Log($"Updated root transform import settings for {assetPath}");
        return true;
    }
}
