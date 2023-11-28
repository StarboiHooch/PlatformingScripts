namespace PlatformingScripts
{
    public class EventTrigger_OnPlayerGround : GameJamHelpers.Generic.EventTrigger
    {
        public void Activate()
        {
            if (active)
            {
                InvokeEvents();
            }
        }
    }
}