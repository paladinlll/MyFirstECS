using Unity.Rendering;
using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Boostrap : MonoBehaviour
{
    public static RenderMesh BullerRenderer;
    public static RenderMesh HighlightTile;
    public static RenderMesh HexTerrain;

    public static Material DefaultMaterial;
    public static Material YellowMaterial;

    public static GameObject PlayerPrefab;

    [SerializeField]
    private RenderMesh _bullerRenderer;

    [SerializeField]
    private RenderMesh _hexTerrain;

    [SerializeField]
    private Material _defaultMaterial;

    [SerializeField]
    private Material _yellowMaterial;

    public GameObject _playerPrefab;

    private void Awake()
    {
        HexTerrain = _hexTerrain;
        BullerRenderer = _bullerRenderer;
        DefaultMaterial = _defaultMaterial;
        YellowMaterial = _yellowMaterial;
        PlayerPrefab = _playerPrefab;

        NewGame();
    }

    public static void NewGame()
    {
        var player = Object.Instantiate(PlayerPrefab);
        var entity = player.GetComponent<GameObjectEntity>().Entity;
        var entityManager = World.Active.EntityManager;

        entityManager.AddComponentData(entity, new InputComponent { Move = new float2(0, 0) });
        entityManager.AddComponentData(entity, new Rotation { Value = quaternion.identity });
    }
}
