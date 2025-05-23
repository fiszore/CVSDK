using AI.Events;

namespace AI.Actions {
    [System.Serializable]
    public abstract class BaseAction : Action {
        public virtual ActionEventResponse UniqueActionResponse(Actor actor, Event e)  {
            return ignoreResponse;
        }
    }
}
