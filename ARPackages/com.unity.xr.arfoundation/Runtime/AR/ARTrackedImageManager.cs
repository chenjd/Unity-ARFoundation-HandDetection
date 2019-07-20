using System;
using System.Collections.Generic;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A manager for <see cref="ARTrackedImage"/>s. Uses the <c>XRImageTrackingSubsystem</c>
    /// to recognize and track 2D images in the physical environment.
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_TrackedImageManager)]
    [RequireComponent(typeof(ARSessionOrigin))]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.5/api/UnityEngine.XR.ARFoundation.ARTrackedImageManager.html")]
    public sealed class ARTrackedImageManager : ARTrackableManager<
        XRImageTrackingSubsystem,
        XRImageTrackingSubsystemDescriptor,
        XRTrackedImage,
        ARTrackedImage>
    {
        [SerializeField]
        [Tooltip("The library of images which will be detected and/or tracked in the physical environment.")]
        XRReferenceImageLibrary m_ReferenceLibrary;

        /// <summary>
        /// The <c>XRReferenceImageLibrary</c> to use during image detection. This is the
        /// library of images which will be detected and/or tracked in the physical environment.
        /// </summary>
        public XRReferenceImageLibrary referenceLibrary
        {
            get { return m_ReferenceLibrary; }
            set
            {
                m_ReferenceLibrary = value;
                UpdateReferenceImages();

                if (subsystem != null)
                    subsystem.imageLibrary = m_ReferenceLibrary;
            }
        }

        [SerializeField]
        [Tooltip("The maximum number of moving images to track in realtime. Not all implementations support this feature.")]
        int m_MaxNumberOfMovingImages;

        /// <summary>
        /// The maximum number of moving images to track in realtime. Support may vary between devices and providers. Check
        /// for support at runtime with <see cref="subsystem"/><c>.SubsystemDescriptor.supportsMovingImages</c>.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the subsystem does not support moving images.</exception>
        public int maxNumberOfMovingImages
        {
            get { return m_MaxNumberOfMovingImages; }
            set
            {
                if (value == m_MaxNumberOfMovingImages)
                    return;

                SetMaxNumberOfMovingImages(value);
            }
        }

        [SerializeField]
        [Tooltip("If not null, instantiates this prefab for each detected image.")]
        GameObject m_TrackedImagePrefab;

        /// <summary>
        /// If not null, instantiates this prefab for each detected image.
        /// </summary>
        public GameObject trackedImagePrefab
        {
            get { return m_TrackedImagePrefab; }
            set { m_TrackedImagePrefab = value; }
        }

        protected override GameObject GetPrefab()
        {
            return m_TrackedImagePrefab;
        }

        /// <summary>
        /// Invoked once per frame with information about the <see cref="ARTrackedImage"/>s that have changed, i.e., been added, updated, or removed.
        /// This happens just before <see cref="ARTrackedImage"/>s are destroyed, so you can set <c>ARTrackedImage.destroyOnRemoval</c> to <c>false</c>
        /// from this event to suppress this behavior.
        /// </summary>
        public event Action<ARTrackedImagesChangedEventArgs> trackedImagesChanged;

        /// <summary>
        /// The name to be used for the <c>GameObject</c> whenever a new image is detected.
        /// </summary>
        protected override string gameObjectName
        {
            get { return "ARTrackedImage"; }
        }

        /// <summary>
        /// Initializes private members.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            m_ReferenceImages = new Dictionary<Guid, XRReferenceImage>();
        }

        /// <summary>
        /// Sets the image library on the subsystem before Start() is called on the <c>XRImageTrackingSubsystem</c>.
        /// </summary>
        protected override void OnBeforeStart()
        {
            UpdateReferenceImages();
            SetMaxNumberOfMovingImages(m_MaxNumberOfMovingImages);
            subsystem.imageLibrary = m_ReferenceLibrary;
        }

        protected override void OnAfterSetSessionRelativeData(
            ARTrackedImage image,
            XRTrackedImage sessionRelativeData)
        {
            XRReferenceImage referenceImage;
            if (m_ReferenceImages.TryGetValue(sessionRelativeData.sourceImageId, out referenceImage))
            {
                image.referenceImage = referenceImage;
            }
            else
            {
                Debug.LogErrorFormat("Could not find reference image with guid {0}", sessionRelativeData.sourceImageId);
            }
        }

        /// <summary>
        /// Invokes the <see cref="trackedImagesChanged"/> event.
        /// </summary>
        /// <param name="added">A list of images added this frame.</param>
        /// <param name="updated">A list of images updated this frame.</param>
        /// <param name="removed">A list of images removed this frame.</param>
        protected override void OnTrackablesChanged(
            List<ARTrackedImage> added,
            List<ARTrackedImage> updated,
            List<ARTrackedImage> removed)
        {
            if (trackedImagesChanged != null)
                trackedImagesChanged(
                    new ARTrackedImagesChangedEventArgs(
                        added,
                        updated,
                        removed));
        }

        void UpdateReferenceImages()
        {
            m_ReferenceImages.Clear();
            if (m_ReferenceLibrary == null)
                return;

            foreach (var image in m_ReferenceLibrary)
                m_ReferenceImages[image.guid] = image;
        }

        void SetMaxNumberOfMovingImages(int value)
        {
            m_MaxNumberOfMovingImages = value;
            if (subsystem != null && descriptor.supportsMovingImages)
            {
                subsystem.maxNumberOfMovingImages = value;
            }
        }

        Dictionary<Guid, XRReferenceImage> m_ReferenceImages;
    }
}
