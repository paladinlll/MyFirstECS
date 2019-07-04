using Unity.Rendering;
using UnityEngine;

public class GameDefines : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public RenderMesh BullerRenderer;
    public Mesh highlightMesh;
    //private RenderMesh _hexTerrain;
    public Material DefaultMaterial;
    public Material YellowMaterial;
    public HexOrientation orientation = HexOrientation.Flat;

    public Mesh TileMesh;
    public Material TileMaterial;
    public float TileRadius = 3;
}
