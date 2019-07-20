using AOT;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARKit
{
    /// <summary>
    /// An internal class with only static methods to register the environment probe subsystem before the scene is
    /// loaded.
    /// </summary>
    internal static class ARKitEnvironmentProbeRegistration
    {
        /// <summary>
        /// The identifying name for the environment probe providing implementation.
        /// </summary>
        static readonly string k_SubsystemId = "ARKit-EnvironmentProbe";

        /// <summary>
        /// Create and register the environment probe subsystem descriptor to advertise a providing implementation for
        /// environment probe functionality.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            XREnvironmentProbeSubsystemCinfo environmentProbeSubsystemInfo = new XREnvironmentProbeSubsystemCinfo()
            {
                id = k_SubsystemId,
                implementationType = typeof(ARKitEnvironmentProbeSubsystem),
                supportsManualPlacement = true,
                supportsRemovalOfManual = true,
                supportsAutomaticPlacement = true,
                supportsRemovalOfAutomatic = true,
                supportsEnvironmentTexture = true,
            };

            if (!XREnvironmentProbeSubsystem.Register(environmentProbeSubsystemInfo))
            {
                Debug.LogErrorFormat("Cannot register the {0} subsystem", k_SubsystemId);
            }
        }
    }

    /// <summary>
    /// This subsystem provides implementing functionality for the <c>XREnvironmentProbeSubsystem</c> class.
    /// </summary>
    [Preserve]
    class ARKitEnvironmentProbeSubsystem : XREnvironmentProbeSubsystem
    {
        protected override IProvider CreateProvider()
        {
            return new Provider();
        }

        class Provider : IProvider
        {
            public Provider()
            {
                // Construct the Objective-C environment probe provider.
                EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_Construct();
            }

            public override void Start()
            {
                EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_Start();
            }

            /// <summary>
            /// Stops the environment probe subsystem by disabling the environment probe state.
            /// </summary>
            public override void Stop()
            {
                // Disable the environment probe state.
                EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_Stop();
            }

            /// <summary>
            /// Destroy the environment probe subsystem by first ensuring that the subsystem has been stopped and then
            /// destroying the provider.
            /// </summary>
            public override void Destroy()
            {
                EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_Destruct();
            }

            /// <summary>
            /// Enable or disable automatic placement of environment probes by the provider.
            /// </summary>
            /// <param name='value'><c>true</c> if the provider should automatically place environment probes in the scene.
            /// Otherwise, <c>false</c></param>.
            public override void SetAutomaticPlacement(bool value)
            {
                EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_SetAutomaticPlacementEnabled(value);
            }

            public override bool TryAddEnvironmentProbe(Pose pose, Vector3 scale, Vector3 size, out XREnvironmentProbe environmentProbe)
            {
                return EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_TryAddEnvironmentProbe(pose, scale, size, out environmentProbe);
            }

            /// <summary>
            /// Remove the environment probe matching the trackable ID from the AR session..
            /// </summary>
            /// <param name='trackableId'>The trackable ID for the environment probe to be removed.</param>
            /// <returns>
            /// <c>true</c> if an environment probe matching the trackable ID is found and will be removed from the AR
            /// session. Otherwise, <c>false</c>.
            /// </returns>
            public override bool RemoveEnvironmentProbe(TrackableId trackableId)
            {
                return EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_TryRemoveEnvironmentProbe(trackableId);
            }

            public override TrackableChanges<XREnvironmentProbe> GetChanges(XREnvironmentProbe defaultEnvironmentProbe,
                                                                            Allocator allocator)
            {
                int numAddedEnvironmentProbes;
                IntPtr addedEnvironmentProbesPointer;

                int numUpdatedEnvironmentProbes;
                IntPtr updatedEnvironmentProbesPointer;

                int numRemovedEnvironmentProbeIds;
                IntPtr removedEnvironmentProbeIdsPointer;

                int stride;

                var context = EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_AcquireChanges(out numAddedEnvironmentProbes, out addedEnvironmentProbesPointer,
                                                                                                     out numUpdatedEnvironmentProbes, out updatedEnvironmentProbesPointer,
                                                                                                     out numRemovedEnvironmentProbeIds, out removedEnvironmentProbeIdsPointer,
                                                                                                     out stride);

                try
                {
                    unsafe
                    {
                        // Wrap the navite pointers into a native array and then copy them into a separate native array enabled
                        // with temporary allocations.
                        return new TrackableChanges<XREnvironmentProbe>(
                            (void*)addedEnvironmentProbesPointer, numAddedEnvironmentProbes,
                            (void*)updatedEnvironmentProbesPointer, numUpdatedEnvironmentProbes,
                            (void*)removedEnvironmentProbeIdsPointer, numRemovedEnvironmentProbeIds,
                            defaultEnvironmentProbe, stride,
                            allocator);
                    }
                }
                finally
                {
                    EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_ReleaseChanges(context);
                }
            }

            public override bool supported
            {
                get { return EnvironmentProbeApi.UnityARKit_EnvironmentProbeProvider_IsSupported(); }
            }
        }
    }
}
