using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RunFixedUpdateSystems : MonoBehaviour
{
    private CameraFollowSystem cameraFollowSystem;
    private HexCursorSystem hexCursorSystem;
    // Start is called before the first frame update
    void Start()
    {
        //cameraFollowSystem = World.Active.GetOrCreateSystem<CameraFollowSystem>();
        hexCursorSystem = World.Active.GetOrCreateSystem<HexCursorSystem>();
    }

    private void FixedUpdate()
    {
        //cameraFollowSystem.Update();
        hexCursorSystem.Update();
    }
}
