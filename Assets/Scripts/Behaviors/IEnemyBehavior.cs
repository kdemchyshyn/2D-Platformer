namespace DefaultNamespace.Behaviors
{
    public interface IEnemyBehavior
    {
        void Initialize(
            MovementComponent movement,
            EnvironmentNavigator navigator
        );

        void Activate();

        void Tick(float deltaTime);

        void Deactivate();
    }
}