namespace RoadAndCode.Core.StateMachines
{
    /// <summary>Behaviour attached to one state of a <see cref="StateMachine{TKey}"/>.</summary>
    public interface IState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }

    /// <summary>Base for states that only care about some of the callbacks.</summary>
    public abstract class State : IState
    {
        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void Exit() { }
    }
}
