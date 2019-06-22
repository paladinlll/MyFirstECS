using UnityEngine;
using System.Collections;

public class RotationCompnent : MonoBehaviour
{
    public Quaternion Value;

    private void Awake()
    {
        Value = Quaternion.identity;
    }
}
