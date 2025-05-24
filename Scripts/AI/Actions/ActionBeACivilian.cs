using AI;
using AI.Actions;
using AI.Events;
using UnityEngine;
using UnityEngine.AI;
using Event = AI.Events.Event;

namespace ActorActions {
    [System.Serializable]
    public class ActionBeACivilian : BaseAction {
        public override void OnEnd(Actor actor) {
        }

        public override void OnSuspend(Actor actor) {
        }

        public class Panicking : AnimationBool
        {
            public Panicking(string name, bool value) : base(name, value) { }
        }

        public override ActionTransition OnResume(Actor actor) {
            switch(actor.GetKnowledgeOf(CharacterBase.GetPlayer().gameObject).GetKnowledgeLevel()) {
                case KnowledgeDatabase.KnowledgeLevel.Ignorant:
                    break;
                case KnowledgeDatabase.KnowledgeLevel.Investigative:
                    return new ActionTransitionSuspendFor(new Investigate(CharacterBase.GetPlayer().gameObject), "Oh yeah I was looking for something...");
                case KnowledgeDatabase.KnowledgeLevel.Alert:
                    return new ActionTransitionSuspendFor(new ActionRunAway(CharacterBase.GetPlayer().gameObject), "leg it!");
            }
            return continueWork;
        }

        public override ActionTransition Update(Actor actor) {
            var need = actor.GetRandomInteractable();
            if (need != null) {
                return new ActionTransitionSuspendFor(new WanderToInteractable(need), "I want to wander!");
            } else {
                // Try to wander randomly if we got nothing to do.
                if (!NavMesh.SamplePosition(actor.transform.position, out NavMeshHit targetHit, FollowPathToPoint.maxDistanceFromNavmesh*2f, NavMesh.AllAreas)) {
                    return continueWork;
                }
                return new ActionTransitionSuspendFor(new FollowPathToPoint(targetHit.position, Vector3.down, 5f), "I want to wander!");
            }
        }

        public override ActionEventResponse OnReceivedEvent(Actor actor, Event e) {
            switch (e) {
                case GrabbedByCharacter grabbedByCharacter:
                    return new ActionEventResponseTransition( new ActionTransitionSuspendFor(new Grabbed(grabbedByCharacter.GetCharacter()), "Ack! Grabbed!"));
                case KnowledgeChanged knowledgeChanged:
                    if (knowledgeChanged.GetKnowledge().GetKnowledgeLevel() != KnowledgeDatabase.KnowledgeLevel.Ignorant) {
                        actor.StopUsingAnything();
                    }

                    if (knowledgeChanged.GetKnowledge().target.TryGetComponent(out CharacterBase character) && character.IsPlayer()) {
                        switch (knowledgeChanged.GetKnowledge().GetKnowledgeLevel()) {
                            case KnowledgeDatabase.KnowledgeLevel.Investigative:
                                if (Random.Range(0f, 1f) < 0.5f) {
                                    return new ActionEventResponseTransition( new ActionTransitionSuspendFor( new Investigate(knowledgeChanged.GetKnowledge().target), "What's going on over there?"));
                                } else {
                                    knowledgeChanged.GetKnowledge().TryGetLastKnownDirection(out Vector3 dir);
                                    return new ActionEventResponseTransition( new ActionTransitionSuspendFor(new ActionTurnToFaceDirection(dir, 5f), "What's going on over there?"));
                                }
                            case KnowledgeDatabase.KnowledgeLevel.Alert:
                                return new ActionEventResponseTransition(new ActionTransitionSuspendFor(new GetSurprised(character.gameObject, new ActionRunAway(character.gameObject)),
                                    "is that.. that's cockvore!!"));
                        }
                    }

                    if (knowledgeChanged.GetKnowledge().target.TryGetComponent(out Condom condom)) {
                        return new ActionEventResponseTransition(new ActionTransitionSuspendFor( new GetSurprised(condom.gameObject, new ReturnCondomToRecombobulator(condom)), "Is that... is that a condom??"));
                    }

                    return ignoreResponse;
                case HeardInterestingNoise noise: {
                    if (actor.GetKnowledgeOf(noise.GetOwner().gameObject).GetKnowledgeLevel() == KnowledgeDatabase.KnowledgeLevel.Ignorant) {
                        if (Random.Range(0f, 1f) < noise.GetHeardSound().GetInterestLevel()) {
                            return new ActionEventResponseTransition(new ActionTransitionSuspendFor(
                                new ActionTurnToFaceObject(noise.GetOwner().gameObject, 2f, noise.GetNoiseHeardDirection()),
                                "What was that?"));
                        }
                    }
                    return ignoreResponse;
                }
                default: return base.OnReceivedEvent(actor,e);
            }
        }

        public override ActionEventResponse UniqueActionResponse(Actor actor, Event e)
        {
            if(e is KnowledgeChanged knowledgeChanged)
            {
                if (knowledgeChanged.GetKnowledge().target.TryGetComponent(out CharacterBase character) && character.IsPlayer()) { 
                    switch(knowledgeChanged.GetKnowledge().GetKnowledgeLevel()) {
                        case KnowledgeDatabase.KnowledgeLevel.Ignorant:
                            actor.RaiseEvent(new Panicking("Panicking", false));
                            break;
                        case KnowledgeDatabase.KnowledgeLevel.Alert:
                            actor.RaiseEvent(new Panicking("Panicking", true));
                            break;
                    }
                }
            }
            return base.UniqueActionResponse(actor, e);
        }
    }
}
