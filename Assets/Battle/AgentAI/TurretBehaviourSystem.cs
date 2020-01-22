﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Battle.Movement;

using Battle.Combat;
using Battle.Combat.AttackSources;

namespace Battle.AI
{
    /// <summary>
    /// A turret searches for targets when it does not have one or the target is outside of range.
    /// </summary>
    [UpdateBefore(typeof(PursueBehaviourSystem))]
    public class TurretBehaviourSystem : JobComponentSystem
    {
        [BurstCompile]
        struct IdleJob : IJobForEachWithEntity<TurretBehaviour, LocalToWorld, Target, TurnToDestinationBehaviour, TargettedTool>
        {
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Positions;

            public void Execute(
                Entity e,
                int index,
                [ReadOnly] ref TurretBehaviour behaviour,
                [ReadOnly] ref LocalToWorld localToWorld,
                ref Target target,
                ref TurnToDestinationBehaviour turnTo,
                [ReadOnly] ref TargettedTool tool
                )
            {
                if (target.Value == Entity.Null)
                    return;

                var targetPos = Positions[target.Value].Position;

                // If target is outside fighter range, disengage.
                if (math.lengthsq(targetPos - localToWorld.Position) > tool.Range * tool.Range)
                    target.Value = Entity.Null;

                // set Turret destination to be target
                turnTo.Destination = targetPos;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var pos = GetComponentDataFromEntity<LocalToWorld>(true);
            var job = new IdleJob() { Positions = pos };
            var jobHandle = job.Schedule(this, inputDependencies);
            return jobHandle;
        }
    }
}