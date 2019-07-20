using System.Collections.Generic;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
using UnityEngine.Experimental.XR;
#endif

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Manages the lifetime of the <c>XRInputSubsystem</c>. Add one of these to any <c>GameObject</c> in your scene
    /// if you want device pose information to be available. Read the input by using the <c>TrackedPoseDriver</c>
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_InputManager)]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.5/api/UnityEngine.XR.ARFoundation.ARInputManager.html")]
    public sealed class ARInputManager : MonoBehaviour
    {
        /// <summary>
        /// Get the <c>XRInputSubsystem</c> whose lifetime this component manages.
        /// </summary>
        public XRInputSubsystem subsystem { get; private set; }

        void OnEnable()
        {
            if (subsystem == null)
                subsystem = CreateSubsystem();

            if (subsystem != null)
                subsystem.Start();
        }

        void OnDisable()
        {
            if (subsystem != null)
                subsystem.Stop();
        }

        void OnDestroy()
        {
            if (subsystem != null)
                subsystem.Destroy();

            subsystem = null;
        }

        XRInputSubsystem CreateSubsystem()
        {
            SubsystemManager.GetSubsystemDescriptors(s_SubsystemDescriptors);
            if (s_SubsystemDescriptors.Count > 0)
            {
                var descriptor = s_SubsystemDescriptors[0];
                if (s_SubsystemDescriptors.Count > 1)
                {
                    Debug.LogWarningFormat("Multiple {0} found. Using {1}",
                        typeof(XRInputSubsystem).Name,
                        descriptor.id);
                }

                return descriptor.Create();
            }
            else
            {
                return null;
            }
        }

        static List<XRInputSubsystemDescriptor> s_SubsystemDescriptors =
            new List<XRInputSubsystemDescriptor>();
    }
}
