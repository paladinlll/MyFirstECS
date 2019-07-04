using Unity.Rendering;
using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Bootstrap : MonoBehaviour
{
    //public static RenderMesh BullerRenderer;
    //public static RenderMesh HighlightTile;
    //public static RenderMesh HexTerrain;

    //public static Material DefaultMaterial;
    //public static Material YellowMaterial;

    public static GameDefines Defines;

    public static void NewGame()
    {
        var player = Object.Instantiate(Defines.PlayerPrefab);
        var entity = player.GetComponent<GameObjectEntity>().Entity;
        var entityManager = World.Active.EntityManager;

        entityManager.AddComponentData(entity, new InputComponent { Move = new float2(0, 0) });
        entityManager.AddComponentData(entity, new Rotation { Value = quaternion.identity });

        CreateMap(entityManager);
    }

    public static void CreateMap(EntityManager entityManager)
    {
        HexOrientation orientation = Defines.orientation;

        Vector3 pos = new Vector3(0, -1, 0);

        int mapSize = 10;
        float hexRadius = Defines.TileRadius;
        for (int q = -mapSize; q <= mapSize; q++)
        {
            int r1 = Mathf.Max(-mapSize, -q - mapSize);
            int r2 = Mathf.Min(mapSize, -q + mapSize);
            for (int r = r1; r <= r2; r++)
            {
                var entity = entityManager.CreateEntity();
                // Place the instantiated entity in a grid with some noise
                pos.x = hexRadius * 3.0f / 2.0f * q;
                pos.z = hexRadius * Mathf.Sqrt(3.0f) * (r + q / 2.0f);
                entityManager.AddComponentData(entity, new LocalToWorld { });
                entityManager.AddComponentData(entity, new Translation { Value = pos });
                entityManager.AddComponentData(entity, new Rotation { Value = Quaternion.identity });
                //entityManager.AddSharedComponentData(entity, hexMesh);

                var index = new CubeIndex(q, r, -q - r);
                entityManager.AddComponentData(entity, new HexTileComponent
                {
                    index = index
                });
                float3[] points = new float3[6];
                for (int i = 0; i < 6; i++)
                    points[i] = HexUtils.Corner(Vector3.zero, 1, i, orientation);
                //BlobAssetReference<Unity.Physics.Collider> collider = Unity.Physics.MeshCollider.Create(points, hexMesh.mesh.GetIndices(0), CollisionFilter.Default);
                //var colliderComponent = new PhysicsCollider { Value = collider };
                //entityManager.AddComponentData(entity, colliderComponent);
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var settingsGo = GameObject.Find("Defines");
        Defines = settingsGo?.GetComponent<GameDefines>();
        //Assert.IsNotNull(Defines);
    }
}
