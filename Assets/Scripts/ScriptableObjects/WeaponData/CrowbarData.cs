using UnityEngine;

[CreateAssetMenu(fileName = "CrowbarData", menuName = "ScriptableObjects/SpawnCrowbarDataScriptableObject", order = 1)]
public class CrowbarData : ScriptableObject {
    public float attackDuration;
    public float damage;
    public float rayRadius;
    public float attackRange;
}
