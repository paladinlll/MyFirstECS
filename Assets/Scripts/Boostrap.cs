using Unity.Rendering;
using UnityEngine;
using System.Collections;

public class Boostrap : MonoBehaviour
{
    public static RenderMesh BullerRenderer;

    [SerializeField]
    private RenderMesh _bullerRenderer;

    private void Awake()
    {
        BullerRenderer = _bullerRenderer;
    }
}
