using UnityEngine;

public class UnitStatus {
    public int remainingHealth;

    public UnitStatus (UnitDefinition unitDefinition) {
        remainingHealth = unitDefinition.health;
    }
}