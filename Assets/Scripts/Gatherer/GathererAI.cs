using UnityEngine;
using System.Collections;

public class GathererAI : MonoBehaviour
{
    private enum State
    {
        Idle,
        MoveingToResourceNode,
        GatheringResources,
        MovingToStorage,
    };

    ResourceNode resourceNode;
    Transform storageTransform;

    private IUint unit;
    private State state;
    int goldInventoryAmount;

    private void Awake()
    {
        unit = gameObject.GetComponent<IUint>();
        state = State.Idle;
        goldInventoryAmount = 0;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.Idle:
                //resourceNodeTransform = ...
                if (resourceNode != null)
                {
                    state = State.MoveingToResourceNode;
                }
                break;
            case State.MoveingToResourceNode:
                if (unit.IsIdle())
                {
                    unit.MoveTo(resourceNode.GetPosition(), 10, () =>
                    {
                        state = State.GatheringResources;
                    });
                }
                break;
            case State.GatheringResources:
                if (unit.IsIdle())
                {
                    if (goldInventoryAmount > 0)
                    {
                        //storageTransform = ...
                        if (storageTransform != null)
                        {
                            state = State.MovingToStorage;
                        }
                    }
                    else
                    {
                        unit.PlayAnimationMine(resourceNode.GetPosition(), () =>
                        {
                            goldInventoryAmount++;
                        });
                    }
                }
                break;
            case State.MovingToStorage:
                if (unit.IsIdle())
                {
                    unit.MoveTo(storageTransform.position, 5, () =>
                    {
                        goldInventoryAmount = 0;
                        state = State.Idle;
                    });
                }
                break;
        }
    }
}
