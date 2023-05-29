using UnityEngine;

namespace PlatformingScripts
{
    public static class PhysicsUtility
    {
        //SUVAT Equations
        // v = u + at
        // v^2 = u^2 + 2as
        // s = u + (1/2)at^2
        // s = vt - (1/2)at^2
        // s = (1/2)(u+v)t
        // s: displacement, u: initial velocity, v: final velocity, a: acceleration, t: time
        public static float HeightToVelocity(float height, float gravity, float gravityScale)
        {
            return (float)Mathf.Sqrt(2 * Mathf.Abs(gravity * gravityScale * height));
        }

        public static float HeightToTime(float height, float gravity, float gravityScale)
        {
            float a = gravity * gravityScale;
            return (float)Mathf.Sqrt(Mathf.Abs((2 * height) / a));
        }

        public static float VelocityChangeToHeight(float startingVelocity, float currentVelocity, float gravity, float gravityScale)
        {
            return (startingVelocity * startingVelocity - currentVelocity * currentVelocity) / (2 * Mathf.Abs(gravity * gravityScale));
        }
    }

}