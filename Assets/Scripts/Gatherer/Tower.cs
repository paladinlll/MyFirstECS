using UnityEngine;
using System.Collections;

public class Tower
{
    public static bool TrySpendResourceCost()
    {
        return false;
    }

    public static Tower Create(Transform towerPrefab, Vector3 position)
    {
        Transform towerTransform = Object.Instantiate(towerPrefab, position, Quaternion.identity);

        Tower tower = new Tower(towerTransform);
        return tower;
    }


    Transform towerTransform;
    int contructionTick;
    int contructionTickMax;
    private Tower(Transform towerTransform)
    {
        this.towerTransform = towerTransform;
        contructionTick = 0;
        contructionTickMax = 10;

        //FuntionPeriodic.Create(IncreaseContructionTick, 1);
    }

    private void IncreaseContructionTick()
    {
        if(IsContructing())
        {
            contructionTick++;
            switch(contructionTick)
            {
                case 3:
                    break;
                case 6:
                    break;
                case 10:
                    break;
            }
        }
    }

    private bool IsContructing()
    {
        return (contructionTick < contructionTickMax);
    }
}
