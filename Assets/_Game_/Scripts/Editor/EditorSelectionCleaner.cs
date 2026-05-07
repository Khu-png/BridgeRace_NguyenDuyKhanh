using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorSelectionCleaner
{
    static EditorSelectionCleaner()
    {
        Selection.selectionChanged += RemoveMissingSelectionTargets;
        EditorApplication.hierarchyChanged += QueueRemoveMissingSelectionTargets;
        EditorApplication.playModeStateChanged += _ => QueueRemoveMissingSelectionTargets();
        EditorApplication.delayCall += RemoveMissingSelectionTargets;
    }

    private static void QueueRemoveMissingSelectionTargets()
    {
        EditorApplication.delayCall -= RemoveMissingSelectionTargets;
        EditorApplication.delayCall += RemoveMissingSelectionTargets;
    }

    private static void RemoveMissingSelectionTargets()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            return;
        }

        int validCount = 0;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] != null)
            {
                validCount++;
            }
        }

        if (validCount == selectedObjects.Length)
        {
            return;
        }

        Object[] validObjects = new Object[validCount];
        int validIndex = 0;
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            if (selectedObjects[i] != null)
            {
                validObjects[validIndex] = selectedObjects[i];
                validIndex++;
            }
        }

        Selection.objects = validObjects;
    }
}
