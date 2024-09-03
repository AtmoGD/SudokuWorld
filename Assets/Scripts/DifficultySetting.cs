using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySetting", menuName = "ScriptableObjects/DifficultySetting", order = 1)]
public class DifficultySetting : ScriptableObject
{
    public string displayName;
    public string uid;
    public GameObject gameFieldPrefab;
    public Vector2Int range;
}