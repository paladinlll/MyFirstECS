using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerAnimationSystem : ComponentSystem
{
    private EntityQuery query;

    protected override void OnCreate() {
        query = GetEntityQuery(
            ComponentType.ReadOnly<Animator>(),
            ComponentType.ReadOnly<InputComponent>());
    }

    protected override void OnUpdate() {
        Entities.With(query).ForEach(
            (Entity entity, Animator animator, ref InputComponent inputComponent) =>
            {
                var move = inputComponent.Move;
                //float forward = math.max(math.abs(move.y), math.abs(move.x));
                float turnAmount = Mathf.Atan2(move.x, move.y);
                animator.SetFloat("Forward", move.y, 0.1f, Time.deltaTime);
                animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
                animator.SetBool("OnGround", true);
            });
    }
}
