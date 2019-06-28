using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RunFixedUpdateSystems : MonoBehaviour
{
    //private CameraFollowSystem cameraFollowSystem;
    // Start is called before the first frame update
    void Start()
    {
        //cameraFollowSystem = World.Active.GetOrCreateSystem<CameraFollowSystem>();
    }

    private void FixedUpdate()
    {
        //cameraFollowSystem.Update();
    }
}
