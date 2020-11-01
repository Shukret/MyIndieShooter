namespace CoverShooter
{
    /// <summary>
    /// Touchscreen controls for character movement.
    /// </summary>
    public class TouchMovement : TouchBase
    {
        protected override void Update()
        {
            base.Update();

            if (Controller == null)
                return;

            if (Delta.sqrMagnitude > float.Epsilon)
            {
                Controller.HasMovement = true;
                Controller.Movement = Delta;
            }
            else
                Controller.HasMovement = false;
        }
    }
}
