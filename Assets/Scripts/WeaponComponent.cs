using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct Weapon : IComponentData
{
}

public class WeaponComponent : ComponentDataProxy<Weapon>
{


}
