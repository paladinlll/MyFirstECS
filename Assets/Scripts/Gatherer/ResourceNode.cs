using UnityEngine;
using System.Collections;

public class ResourceNode
{
    private Transform resourceNodeTransform;
    private int resourceAmount;
    private int resourceAmountMax;

    public ResourceNode(Transform resourceNodeTransform)
    {
        this.resourceNodeTransform = resourceNodeTransform;
        resourceAmount = 3;
    }

    public Vector3 GetPosition()
    {
        return resourceNodeTransform.position;
    }

    public void GrabResource()
    {
        resourceAmount--;

        if(resourceAmount <= 0)
        {
            //Invoke("ResetResourceAmount", 5);
        }
    }

    private void ResetResourceAmount()
    {
        resourceAmount = resourceAmountMax;
    }
}
