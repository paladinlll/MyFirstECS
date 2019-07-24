using Unity.Rendering;
using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Bootstrap : MonoBehaviour
{
    public static GameDefines Defines;

    public static void NewGame()
    {
        var entityManager = World.Active.EntityManager;
        var player = Object.Instantiate(Defines.PlayerPrefab);
        var entity = player.GetComponent<GameObjectEntity>().Entity;

        entityManager.AddComponentData(entity, new InputComponent { Move = new float2(0, 0) });
        entityManager.AddComponentData(entity, new Rotation { Value = quaternion.identity });
        //entityManager.AddComponentData(entity, new Translation { Value = new CubeIndex(2, 1).ToWorldPos(Defines.TileRadius) });
        Rigidbody rigidBody = player.GetComponent<Rigidbody>();

        var playerPos = new CubeIndex(-2, 1).ToWorldPos(Defines.TileRadius);
        player.transform.position = playerPos;
        rigidBody.position = playerPos;
        rigidBody.velocity = Vector3.zero;

        CreateTile(entityManager, new Point2D { x = 0, y = 0 }, 0);
        //CreateMap(entityManager);
        for (int i = 1; i < 10; i++)
        {
            CreateStrokeMap(entityManager, i, i % 2);
        }
        CreateCastle(entityManager);

        CreateResourceNode(entityManager, new Point2D { x = 2, y = 2 }, 0);
    }

    public static void CreateStrokeMap(EntityManager entityManager, int Radius, int terrainTypeIndex)
    {
        float HexSide = Defines.TileRadius;

        for (HexDirection direction = HexDirection.NE; direction <= HexDirection.NW; direction++)
        {
            Point2D currentPoint = HexDirectionExtensions.Dirs[(int)direction] * Radius;
            Point2D p = HexDirectionExtensions.Dirs[((int)direction + 2) % 6];
            for (int i = 0; i < Radius; i++)
            {
                //h.name = string.Format("Hex Layer: {0}, n: {1}", mult, hn);
                //Debug.Log($"{direction} -> {currentPoint.x}.{currentPoint.y}");
                CreateTile(entityManager, currentPoint, terrainTypeIndex);
                currentPoint = currentPoint + p;
            }
        }
    }

    public static void CreateFillMap(EntityManager entityManager, int terrainTypeIndex)
    {
        float HexSide = Defines.TileRadius;
        int Radius = 5;

        Point2D currentPoint = new Point2D { x = 0, y = 0 };
        currentPoint += HexDirectionExtensions.Dirs[(int)HexDirection.W];
        int i = 0;
        HexDirection direction = HexDirection.NW;
        for (int mult = 0; mult <= Radius; mult++)
        {
            int hn = 0;
            //for (int j = 0; j < lmv; j++)
            //foreach (Point2D p in HexDirectionExtensions.Dirs)
            for (; direction <= HexDirection.NW; direction++)
            {
                Point2D p = HexDirectionExtensions.Dirs[(int)direction];
                for (; i < mult; i++, hn++)
                {
                    //h.name = string.Format("Hex Layer: {0}, n: {1}", mult, hn);
                    Debug.Log($"{direction} -> {currentPoint.x}.{currentPoint.y}");
                    CreateTile(entityManager, currentPoint, terrainTypeIndex);
                    currentPoint = currentPoint + p;

                }
                i = 0;
                if (direction == HexDirection.W)
                {
                    //Debug.Log($"{direction} -> {currentPoint.x}.{currentPoint.y}");
                    CreateTile(entityManager, currentPoint, terrainTypeIndex);
                    currentPoint = currentPoint + p;
                    hn++;
                    if (mult == Radius)
                        break;      //Finished
                }
            }
            direction = HexDirection.NE;
        }
    }

    public static void CreateTile(EntityManager entityManager, Point2D point, int terrainTypeIndex)
    {
        var index = new CubeIndex(point.x, point.y, -point.x - point.y);
        var entity = entityManager.CreateEntity();
        //int tileRange = index.Radius();

        float3 pos = index.ToWorldPos(Defines.TileRadius);

        //Debug.Log($"{index} -> {pos} -> {HexUtils.FromPosition(pos, Defines.TileRadius)}");
        entityManager.AddComponentData(entity, new LocalToWorld { });
        entityManager.AddComponentData(entity, new Translation { Value = pos });
        entityManager.AddComponentData(entity, new Rotation { Value = Quaternion.identity });

        entityManager.AddComponentData(entity, new HexTileComponent
        {
            index = index,
            terrainTypeIndex = terrainTypeIndex
        });
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

                var index = new CubeIndex(q, r, -q - r);
                entityManager.AddComponentData(entity, new HexTileComponent
                {
                    index = index
                });
            }
        }
    }

    public static void CreateCastle(EntityManager entityManager)
    {
        var entity = entityManager.CreateEntity();
        entityManager.AddComponentData(entity, new NonUniformScale { Value = new float3(2.5f, 2.5f, 2.5f) });
        entityManager.AddComponentData(entity, new LocalToWorld { });
        entityManager.AddComponentData(entity, new Translation { Value = new float3() });
        entityManager.AddComponentData(entity, new Rotation { Value = Quaternion.identity });
        entityManager.AddSharedComponentData(entity, Defines.CastlePrefab);

        //var castle = Object.Instantiate(Defines.CastlePrefab);
        //var castlePos = new CubeIndex(0, 0).ToWorldPos(Defines.TileRadius);
        ////castlePos.y = 0.1f;
        //castle.AddComponent<NonUniformScaleProxy>().Value = new NonUniformScale { Value = new float3(2.5f, 2.5f, 2.5f) };
        //castle.AddComponent<TranslationProxy>().Value = new Translation { Value = castlePos };
        //castle.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndDestroy;
    }

    public static void CreateResourceNode(EntityManager entityManager, Point2D point, int resourceId)
    {
        var entity = entityManager.CreateEntity();
        var index = new CubeIndex(point.x, point.y, -point.x - point.y);
        float3 pos = index.ToWorldPos(Defines.TileRadius);

        entityManager.AddComponentData(entity, new NonUniformScale { Value = new float3(2.5f, 2.5f, 2.5f) });
        entityManager.AddComponentData(entity, new LocalToWorld { });
        entityManager.AddComponentData(entity, new Translation { Value = pos });
        entityManager.AddComponentData(entity, new Rotation { Value = Quaternion.identity });
        entityManager.AddSharedComponentData(entity, Defines.ResourceNodesPrefab[resourceId]);
        entityManager.AddComponentData(entity, new ResourceNodeComponent
        {
            CubeIndex = index,
            ResourceId = resourceId
        });
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var settingsGo = GameObject.Find("Defines");
        Defines = settingsGo?.GetComponent<GameDefines>();
        //Assert.IsNotNull(Defines);
    }
}
