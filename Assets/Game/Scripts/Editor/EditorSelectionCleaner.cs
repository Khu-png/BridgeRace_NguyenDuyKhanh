using System.Linq;
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

        Object[] validObjects = selectedObjects.Where(selectedObject => selectedObject != null).ToArray();
        if (validObjects.Length != selectedObjects.Length)
        {
            Selection.objects = validObjects;
        }
    }
}
