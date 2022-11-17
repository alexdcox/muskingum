using UnityEditor;
using UnityEngine;

// NOTE: This originally inherited MonoBehaviour, I removed it to try and suppress the error:
//       Cannot add menu item 'GameObject/Create Empty Child' for method 'RuntimeMethodInfo.BringMeBackFromTheEdgeOfMadness' because a menu item with the same name already exists.
public class FixStupidEditorBehavior {
    public static bool _applied;
    [MenuItem("GameObject/Create Empty Child #&n")]
    static void BringMeBackFromTheEdgeOfMadness() {
        if (_applied) return;
        _applied = true;
        GameObject go = new GameObject("GameObject");
        if(Selection.activeTransform != null)
            go.transform.parent = Selection.activeTransform;
    }
}
