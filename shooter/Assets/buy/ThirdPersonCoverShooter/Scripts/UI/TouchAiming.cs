namespace CoverShooter
{
    /// <summary>
    /// Touchscreen controls for character aiming.
    /// </summary>
    public class TouchAiming : TouchBase
    {
        protected override void Update()
        {
            base.Update();

            if (Controller == null)
                return;

            if (Delta.sqrMagnitude > float.Epsilon)
            {
                Controller.IsAllowedToFire = !IsCancelled;
                Controller.HasAiming = true;
                Controller.Aiming = Delta;
                Controller.Magnitude = Magnitude;
            }
            else
                Controller.HasAiming = false;                
        }
    }
}
