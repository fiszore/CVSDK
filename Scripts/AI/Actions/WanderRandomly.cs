using AI;
using AI.Actions;
using AI.Events;
using UnityEngine;
using UnityEngine.AI;

namespace ActorActions
{

    public class WanderRandomly : Action
    {
        private float waitOnArrival;
        private Vector3 destination;

        public WanderRandomly(float waitOnArrival = 5f)
        {
            this.waitOnArrival = waitOnArrival;
        }

        public ActionTransition FindNextPosition(Actor actor)
        {
            if (Random.Range(0f, 1f) < 0.1f)
            {
                return new ActionTransitionSuspendFor(new DoNothing(2f), "Gonna think for a bit.");
            }

            if (destination == Vector3.zero)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
                randomDirection += actor.transform.position;
                if (!NavMesh.SamplePosition(randomDirection, out NavMeshHit targetHit, FollowPathToPoint.maxDistanceFromNavmesh * 2f, NavMesh.AllAreas))
                {
                    return continueWork;
                }
                destination = targetHit.position;
            }

            if (Vector3.Distance(destination, actor.transform.TransformPoint(Vector3.down)) > 1f)
            {
                return new ActionTransitionSuspendFor(new FollowPathToPoint(destination, Vector3.down, 5f), "I want to wander!");
            }

            return new ActionTransitionChangeTo(new DoNothing(waitOnArrival), "I've arrived, time to chill.");
        }

        public override ActionTransition OnStart(Actor actor)
        {
            return FindNextPosition(actor);
        }

        public override ActionTransition OnResume(Actor actor)
        {
            return FindNextPosition(actor);
        }

        public override ActionTransition Update(Actor actor)
        {
            return FindNextPosition(actor);
        }

        public override ActionEventResponse OnReceivedEvent(Actor actor, AI.Events.Event e)
        {
            switch (e)
            {
                case FailedToFindPath:
                    return new ActionEventResponseTransition(new ActionTransitionChangeTo(new DoNothing(0.1f), "Giving up on wandering."));
                case HeardInterestingNoise noise:
                    if (actor.GetKnowledgeOf(noise.GetOwner().gameObject).GetKnowledgeLevel() == KnowledgeDatabase.KnowledgeLevel.Ignorant)
                    {
                        if (Random.Range(0f, 1f) < noise.GetHeardSound().GetInterestLevel())
                        {
                            return new ActionEventResponseTransition(new ActionTransitionSuspendFor(new ActionTurnToFaceObject(noise.GetOwner().gameObject, 2f, noise.GetNoiseHeardDirection()), "What was that?"));
                        }
                    }
                    return base.OnReceivedEvent(actor, e);
            }
            return base.OnReceivedEvent(actor, e);
        }
    }

}
