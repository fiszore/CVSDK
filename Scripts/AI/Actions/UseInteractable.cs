using UnityEngine;

namespace AI.Actions {
    public class UseInteractable : Action {
        private readonly IInteractable target;
        private bool returnControl;

        public UseInteractable(IInteractable target, bool returnControl = false) {
            this.target = target;
            this.returnControl = returnControl;
        }

        public ActionTransition Think(Actor actor) {
            float distanceToTarget = Vector3.Distance(target.transform.position, actor.transform.position);
            bool canStillUse = target.CanInteract(actor.GetCharacter());

            if (distanceToTarget >= FollowPathToPoint.maxDistanceFromNavmesh * 5f)
            {
                if (!canStillUse)
                {
                    OnEnd(actor);
                    return new ActionTransitionChangeTo(new DoNothing(2f), "I can't use that station anymore. Oh No!");
                }
            }

            if (distanceToTarget >= FollowPathToPoint.maxDistanceFromNavmesh) {
                return new ActionTransitionSuspendFor(new FollowPathToPoint(target.transform.position, Vector3.down, 1f), "Too far from the interactable, going to walk closer.");
            }

            if (!canStillUse)
            {
                OnEnd(actor);
                return new ActionTransitionChangeTo(new DoNothing(2f), "I can't use that station anymore. Oh No!");
            }

            actor.UseInteractable(target);
            return continueWork;
        }

        public override ActionTransition OnStart(Actor actor) {
            return Think(actor);
        }

        public override ActionTransition OnResume(Actor actor) {
            return Think(actor);
        }

        public override void OnEnd(Actor actor) {
            if (!returnControl) {
                actor.StopUsingAnything();
            }
            base.OnEnd(actor);
        }

        public override ActionTransition Update(Actor actor) {
            if (actor.IsStillUsing(target) && !returnControl) {
                return continueWork;
            }
            return new ActionTransitionChangeTo(new DoNothing(1f),"Done using the item!");
        }
    }
}
