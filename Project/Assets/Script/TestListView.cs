using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestListView", menuName = "Scriptable Objects/TestListView")]
public class TestListView : ScriptableObject
{
    public List<PlayerData> players = new();
 
}
