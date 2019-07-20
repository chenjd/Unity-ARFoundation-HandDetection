using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A structure for light estimation information provided by the AR device.
    /// </summary>
    public struct ARLightEstimationData : IEquatable<ARLightEstimationData>
    {
        /// <summary>
        /// An estimate for the average brightness in the scene.
        /// Use <c>averageBrightness.HasValue</c> to determine if this information is available.
        /// </summary>
        /// <remarks>
        /// <see cref="averageBrightness"/> may be null when light estimation is not enabled in the <see cref="ARSession"/>,
        /// if the platform does not support it, or if a platform-specific error has occurred.
        /// </remarks>
        public float? averageBrightness { get; set; }

        /// <summary>
        /// An estimate for the average color temperature of the scene.
        /// Use <c>averageColorTemperature.HasValue</c> to determine if this information is available.
        /// </summary>
        /// <remarks>
        /// <see cref="averageColorTemperature"/> may be null when light estimation is not enabled in the <see cref="ARSession"/>,
        /// if the platform does not support it, or if a platform-specific error has occurred.
        /// </remarks>
        public float? averageColorTemperature { get; set; }

        /// <summary>
        /// The scaling factors used for color correction.
        /// The RGB scale factors are used to match the color of the light
        /// in the scene. The alpha channel value is platform-specific.
        /// </summary>
        /// <remarks>
        /// <see cref="colorCorrection"/> may be null when light estimation is not enabled in the <see cref="ARSession"/>,
        /// if the platform does not support it, or if a platform-specific error has occurred.
        /// </remarks>
        public Color? colorCorrection { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                return
                    (averageBrightness.GetHashCode() * 486187739 +
                    averageColorTemperature.GetHashCode()) * 486187739 +
                    colorCorrection.GetHashCode();
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARLightEstimationData))
                return false;

            return Equals((ARLightEstimationData)obj);
        }

        public override string ToString()
        {
            return string.Format("(Avg. Brightness: {0}, Avg. Color Temperature: {1}, Color Correction: {2})",
                averageBrightness, averageColorTemperature, colorCorrection);
        }

        public bool Equals(ARLightEstimationData other)
        {
            return
                (averageBrightness.Equals(other.averageBrightness)) &&
                (averageColorTemperature.Equals(other.averageColorTemperature)) &&
                (colorCorrection.Equals(other.colorCorrection));
        }

        public static bool operator ==(ARLightEstimationData lhs, ARLightEstimationData rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARLightEstimationData lhs, ARLightEstimationData rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}
