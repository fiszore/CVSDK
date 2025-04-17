using AI;
using AI.Actions;
using AI.Events;
using UnityEngine;
using UnityEngine.AI;
using Event = AI.Events.Event;
using System.Collections.Generic;

namespace ActorActions
{
    [System.Serializable]
    public class ActionBeABusyBody : BaseAction
    {
        [System.Serializable]
        private class WorkNode
        {
            public NeedStation station;
            public int timesUsed = 2;
        }
        [SerializeField] private CycleMode cycleMode;
        [SerializeField] private List<WorkNode> patrolNodes;
        private int currentNode = -1;
        private Vector3 patrolPosition;
        private int dir = 1;

        private enum CycleMode
        {
            Loop,
            PingPong,
            Random,
        }

        public override void OnEnd(Actor actor)
        {
        }

        public override void OnSuspend(Actor actor)
        {
        }

        public override ActionTransition OnResume(Actor actor)
        {
            switch (actor.GetKnowledgeOf(CharacterBase.GetPlayer().gameObject).GetKnowledgeLevel())
            {
                case KnowledgeDatabase.KnowledgeLevel.Investigative:
                    return new ActionTransitionSuspendFor(new Investigate(CharacterBase.GetPlayer().gameObject), "Oh yeah I was looking for something...");
                case KnowledgeDatabase.KnowledgeLevel.Alert:
                    return new ActionTransitionSuspendFor(new ActionRunAway(CharacterBase.GetPlayer().gameObject), "leg it!");
            }
            return continueWork;
        }

        private void UpdateCurrent()
        {
            if (cycleMode == CycleMode.Loop)
            {
                currentNode = ++currentNode % patrolNodes.Count;
            }
            else if (cycleMode == CycleMode.PingPong)
            {
                currentNode += dir;
                if (currentNode >= patrolNodes.Count)
                {
                    dir *= -1;
                    currentNode = Mathf.Max(patrolNodes.Count - 2, 0);
                }
                else if (currentNode < 0)
                {
                    dir *= -1;
                    currentNode = Mathf.Min(patrolNodes.Count - 1, 1);
                }
            }
            else
            {
                currentNode = Random.Range(0, patrolNodes.Count);
            }
        }

        public override ActionTransition Update(Actor actor)
        {
            UpdateCurrent();
            var need = patrolNodes[currentNode].station;
            if (need != null && need.CanInteract(actor.GetCharacter()))
            {
                return new ActionTransitionSuspendFor(new WanderToStation(need), "I want to work!");
            }

            //Try one more time to find a task to work at.
            UpdateCurrent();
            need = patrolNodes[currentNode].station;
            if (need != null && need.CanInteract(actor.GetCharacter()))
            {
                return new ActionTransitionSuspendFor(new WanderToStation(need), "I want to work!");
            }

            else
            {
                // Try to wander randomly if we got nothing to do.
                if (!NavMesh.SamplePosition(actor.transform.position, out NavMeshHit targetHit, FollowPathToPoint.maxDistanceFromNavmesh * 2f, NavMesh.AllAreas))
                {
                    return continueWork;
                }
                return new ActionTransitionSuspendFor(new FollowPathToPoint(targetHit.position, Vector3.down, 5f), "I want to wander!");
            }
        }

        public override ActionEventResponse OnReceivedEvent(Actor actor, Event e)
        {
            switch (e)
            {
                case GrabbedByCharacter grabbedByCharacter:
                    return new ActionEventResponseTransition(new ActionTransitionSuspendFor(new Grabbed(grabbedByCharacter.GetCharacter()), "Ack! Grabbed!"));
                case KnowledgeChanged knowledgeChanged:
                    if (knowledgeChanged.GetKnowledge().GetKnowledgeLevel() != KnowledgeDatabase.KnowledgeLevel.Ignorant)
                    {
                        actor.StopUsingAnything();
                    }

                    if (knowledgeChanged.GetKnowledge().target.TryGetComponent(out CharacterBase character) && character.IsPlayer())
                    {
                        switch (knowledgeChanged.GetKnowledge().GetKnowledgeLevel())
                        {
                            case KnowledgeDatabase.KnowledgeLevel.Investigative:
                                if (Random.Range(0f, 1f) < 0.5f)
                                {
                                    return new ActionEventResponseTransition(new ActionTransitionSuspendFor(new Investigate(knowledgeChanged.GetKnowledge().target), "What's going on over there?"));
                                }
                                else
                                {
                                    knowledgeChanged.GetKnowledge().TryGetLastKnownDirection(out Vector3 dir);
                                    return new ActionEventResponseTransition(new ActionTransitionSuspendFor(new ActionTurnToFaceDirection(dir, 5f), "What's going on over there?"));
                                }
                            case KnowledgeDatabase.KnowledgeLevel.Alert:
                                return new ActionEventResponseTransition(new ActionTransitionSuspendFor(new GetSurprised(character.gameObject, new ActionRunAway(character.gameObject)),
                                    "is that.. that's cockvore!!"));
                        }
                    }

                    if (knowledgeChanged.GetKnowledge().target.TryGetComponent(out Condom condom))
                    {
                        return new ActionEventResponseTransition(new ActionTransitionSuspendFor(new GetSurprised(condom.gameObject, new ReturnCondomToRecombobulator(condom)), "Is that... is that a condom??"));
                    }

                    return ignoreResponse;
                case HeardInterestingNoise noise:
                    {
                        if (actor.GetKnowledgeOf(noise.GetOwner().gameObject).GetKnowledgeLevel() == KnowledgeDatabase.KnowledgeLevel.Ignorant)
                        {
                            if (Random.Range(0f, 1f) < noise.GetHeardSound().GetInterestLevel())
                            {
                                return new ActionEventResponseTransition(new ActionTransitionSuspendFor(
                                    new ActionTurnToFaceObject(noise.GetOwner().gameObject, 2f, noise.GetNoiseHeardDirection()),
                                    "What was that?"));
                            }
                        }
                        return ignoreResponse;
                    }
                default: return base.OnReceivedEvent(actor, e);
            }
        }
    }
}
