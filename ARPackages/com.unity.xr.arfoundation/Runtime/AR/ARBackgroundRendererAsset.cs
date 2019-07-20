namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// An asset used with the <see cref="ARFoundationBackgroundRenderer"/>. Useful for working with the
    /// Lightweight Render Pipeline (LWRP).
    /// </summary>
    public abstract class ARBackgroundRendererAsset : ScriptableObject
    {
        /// <summary>
        /// Create a <see cref="ARFoundationBackgroundRenderer"/>.
        /// </summary>
        /// <returns>A new <see cref="ARFoundationBackgroundRenderer"/>.</returns>
        public abstract ARFoundationBackgroundRenderer CreateARBackgroundRenderer();

        /// <summary>
        /// Creates helper components associated with <paramref name="gameObject"/>.
        /// </summary>
        /// <param name="gameObject">The <c>GameObject</c> on which to create the components.</param>
        public abstract void CreateHelperComponents(GameObject gameObject);

        /// <summary>
        /// Creates the <c>Material</c> necessary for the custom background rendering.
        /// </summary>
        /// <returns>A <c>Material</c> used for background rendering.</returns>
        public abstract Material CreateCustomMaterial();
    }
}
