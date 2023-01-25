using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickPlay : MonoBehaviour {
  public NetworkController networkController;

  void OnEnable() {
    string name = "Barny";
#if UNITY_EDITOR
    name = "Editorial";
#endif

    networkController.SendMessageJoinCombatantQueue(name);
  }
}
