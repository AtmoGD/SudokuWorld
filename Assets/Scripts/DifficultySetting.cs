using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySetting", menuName = "ScriptableObjects/DifficultySetting", order = 1)]
public class DifficultySetting : ScriptableObject
{
    public string difficultyName;
    public Vector2Int range;
}