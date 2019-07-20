using System;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// The arguments for the <see cref="ARFoundationBackgroundRenderer.backgroundRendererChanged"/>
    /// event. This is currently empty, but may change in the future without the need to change the
    /// subscribers' method signatures.
    /// </summary>
    public struct ARFoundationBackgroundRendererChangedEventArgs : IEquatable<ARFoundationBackgroundRendererChangedEventArgs>
    {
        /// <summary>
        /// Generates a hash code suitable for use in a <c>Dictionary</c> or <c>HashSet</c>.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return 0;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ARFoundationBackgroundRendererChangedEventArgs))
                return false;

            return Equals((ARFoundationBackgroundRendererChangedEventArgs)obj);
        }

        /// <summary>
        /// Interface for <c>IEquatable</c>
        /// </summary>
        public bool Equals(ARFoundationBackgroundRendererChangedEventArgs other)
        {
            return true;
        }

        public static bool operator ==(ARFoundationBackgroundRendererChangedEventArgs lhs, ARFoundationBackgroundRendererChangedEventArgs rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ARFoundationBackgroundRendererChangedEventArgs lhs, ARFoundationBackgroundRendererChangedEventArgs rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}