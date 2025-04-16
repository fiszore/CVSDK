using AI;
using AI.Actions;
using AI.Events;
using UnityEngine;
using UnityEngine.AI;

namespace ActorActions
{

    public class WanderToStation : Action
    {
        private IInteractable targetStation;
        private bool hasWorked = false;

        public WanderToStation(IInteractable targetStation)
        {
            this.targetStation = targetStation;
        }

        public ActionTransition FigureNextAction(Actor actor)
        {
            if (Random.Range(0f, 1f) < 0.1f)
            {
                return new ActionTransitionSuspendFor(new DoNothing(2f), "Gonna think for a bit.");
            }
            if (hasWorked)
            {
                base.OnEnd(actor);
                return new ActionTransitionChangeTo(new DoNothing(0f), "All done here!");
            }
                
            else
                hasWorked = true;

            return new ActionTransitionSuspendFor(new UseInteractable(targetStation), "I've decided what need to use.");
        }

        public override ActionTransition OnStart(Actor actor)
        {
            return FigureNextAction(actor);
        }

        public override ActionTransition OnResume(Actor actor)
        {
            return FigureNextAction(actor);
        }

        public override ActionTransition Update(Actor actor)
        {
            return FigureNextAction(actor);
        }

        public override ActionEventResponse OnReceivedEvent(Actor actor, AI.Events.Event e)
        {
            switch (e)
            {
                case FailedToFindPath:
                    return new ActionEventResponseTransition(new ActionTransitionChangeTo(new DoNothing(0.1f), "Giving up on going to that need."));
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
