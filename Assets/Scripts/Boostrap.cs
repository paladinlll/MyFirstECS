using Unity.Rendering;
using UnityEngine;
using System.Collections;

public class Boostrap : MonoBehaviour
{
    public static RenderMesh BullerRenderer;
    public static RenderMesh HighlightTile;

    public static Material DefaultMaterial;
    public static Material YellowMaterial;

    [SerializeField]
    private RenderMesh _bullerRenderer;

    [SerializeField]
    private Material _defaultMaterial;

    [SerializeField]
    private Material _yellowMaterial;


    private void Awake()
    {
        BullerRenderer = _bullerRenderer;
        DefaultMaterial = _defaultMaterial;
        YellowMaterial = _yellowMaterial;
    }
}
