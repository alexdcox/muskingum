using UnityEditor;
using UnityEngine;

public class FixStupidEditorBehavior : MonoBehaviour {
    static bool _applied;
    [MenuItem("GameObject/Create Empty Child #&n")]
    static void BringMeBackFromTheEdgeOfMadness() {
        if (_applied) return;
        _applied = true;
        GameObject go = new GameObject("GameObject");
        if(Selection.activeTransform != null)
            go.transform.parent = Selection.activeTransform;
    }
}
