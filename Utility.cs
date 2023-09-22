namespace PlatformingScripts
{
    public static class Utility
    {
        public static float NormaliseForJoystickThreshhold(float value)
        {
            switch (value)
            {
                case > Settings.joystickThreshhold:
                    return 1;
                case < -Settings.joystickThreshhold:
                    return -1;
                default:
                    return 0;
            }
        }
    }
}