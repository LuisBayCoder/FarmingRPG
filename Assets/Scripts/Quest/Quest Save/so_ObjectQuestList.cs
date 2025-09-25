using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "so_ObjectQuestList", menuName = "Scriptable Objects/Quest/so_ObjectQuestList")]
public class SO_ObjectQuestList : ScriptableObject
{
    [SerializeField]
    public List<QuestObjectDetails> objectDetails;
}

