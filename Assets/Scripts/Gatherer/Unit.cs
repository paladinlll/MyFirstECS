using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public interface IUint
{
    bool IsIdle();
    void MoveTo(Vector3 position, float stopDistance, UnityAction onArriedAtPosition);
    void PlayAnimationMine(Vector3 lookAtPsition, UnityAction onAnimationCompleted);
}

public class Unit : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
