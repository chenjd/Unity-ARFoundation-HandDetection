using System.Collections.Generic;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
#endif

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// A base class for subsystems whose lifetime is managed by a <c>MonoBehaviour</c>.
    /// </summary>
    /// <typeparam name="TSubsystem">The <c>Subsystem</c> which provides this manager data.</typeparam>
    /// <typeparam name="TSubsystemDescriptor">The <c>SubsystemDescriptor</c> required to create the Subsystem.</typeparam>
    public class SubsystemLifecycleManager<TSubsystem, TSubsystemDescriptor> : MonoBehaviour
        where TSubsystem : Subsystem<TSubsystemDescriptor>
        where TSubsystemDescriptor : SubsystemDescriptor<TSubsystem>
    {
        /// <summary>
        /// Get the <c>TSubsystem</c> whose lifetime this component manages.
        /// </summary>
        public TSubsystem subsystem { get; private set; }

        /// <summary>
        /// The descriptor for the subsystem.
        /// </summary>
        /// <value>
        /// The descriptor for the subsystem.
        /// </value>
        public TSubsystemDescriptor descriptor
        {
            get { return (subsystem == null) ? null : subsystem.SubsystemDescriptor; }
        }

        /// <summary>
        /// Creates a <c>TSubsystem</c>.
        /// </summary>
        /// <returns>The first Subsystem of matching the <c>TSubsystemDescriptor</c>, or <c>null</c> if there aren't any.</returns>
        protected virtual TSubsystem CreateSubsystem()
        {
            SubsystemManager.GetSubsystemDescriptors(s_SubsystemDescriptors);
            if (s_SubsystemDescriptors.Count > 0)
            {
                var descriptor = s_SubsystemDescriptors[0];
                if (s_SubsystemDescriptors.Count > 1)
                {
                    Debug.LogWarningFormat("Multiple {0} found. Using {1}",
                        typeof(TSubsystem).Name,
                        descriptor.id);
                }

                return descriptor.Create();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a subsystem if subsystem is <c>null</c>.
        /// </summary>
        protected void CreateSubsystemIfNecessary()
        {
            if (subsystem == null)
                subsystem = CreateSubsystem();
        }

        /// <summary>
        /// Creates the <c>TSubsystem</c>.
        /// </summary>
        protected virtual void OnEnable()
        {
            CreateSubsystemIfNecessary();

            if (subsystem != null)
            {
                OnBeforeStart();
                subsystem.Start();
                OnAfterStart();
            }
        }

        /// <summary>
        /// Stops the <c>TSubsystem</c>.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (subsystem != null)
                subsystem.Stop();
        }

        /// <summary>
        /// Destroys the <c>TSubsystem</c>.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (subsystem != null)
                subsystem.Destroy();

            subsystem = null;
        }

        /// <summary>
        /// Invoked after creating the subsystem and before calling Start on it.
        /// The <see cref="subsystem"/> is not <c>null</c>.
        /// </summary>
        protected virtual void OnBeforeStart()
        { }

        /// <summary>
        /// Invoked after calling Start on it the Subsystem.
        /// The <see cref="subsystem"/> is not <c>null</c>.
        /// </summary>
        protected virtual void OnAfterStart()
        { }

        static List<TSubsystemDescriptor> s_SubsystemDescriptors =
            new List<TSubsystemDescriptor>();
    }
}
