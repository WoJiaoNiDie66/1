// Assets/Scripts/World/MapNode.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMapNode", menuName = "GIB3/MapNode")]
public class MapNode : ScriptableObject
{
    [Tooltip("必须与场景中地图根物体的名字完全一致")]
    public string mapID; 

    [Header("Connections")]
    public MapNode parentNode; 
    public List<MapNode> childNodes = new List<MapNode>(); 

    // 现在的逻辑不再返回 GameObject，而是返回一组需要激活的 MapNode 资源
    public List<MapNode> GetActiveNodeGroup()
    {
        List<MapNode> group = new List<MapNode>();
        group.Add(this);
        if (parentNode != null) group.Add(parentNode);
        foreach (var child in childNodes)
        {
            if (child != null) group.Add(child);
        }
        return group;
    }
}