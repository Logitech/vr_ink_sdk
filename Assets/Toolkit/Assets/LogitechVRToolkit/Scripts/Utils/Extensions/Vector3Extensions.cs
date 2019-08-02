namespace Logitech.XRToolkit.Utils
{
    using UnityEngine;

    /// <summary>
    /// Extension methods for Vector3.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Rotates a point around a pivot by an angle.
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The point to rotate around.</param>
        /// <param name="angles">The X, Y and Z angles to rotate in degrees.</param>
        /// <returns></returns>
        public static Vector3 RotatePointAroundPivot(this Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }
    }
}
