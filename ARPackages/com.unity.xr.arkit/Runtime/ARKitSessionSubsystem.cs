using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARKit
{
    /// <summary>
    /// ARKit implementation of the <c>XRSessionSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public sealed class ARKitSessionSubsystem : XRSessionSubsystem
    {
        /// <summary>
        /// <para>Asynchronously create an <see cref="ARWorldMap"/>. An <c>ARWorldMap</c>
        /// represents the state of the session and can be serialized to a byte
        /// array to persist the session data, or send it to another device for
        /// shared AR experiences.</para>
        /// <para>It is a wrapper for <a href="https://developer.apple.com/documentation/arkit/arworldmap">ARKit's ARWorldMap</a>.</para>
        /// </summary>
        /// <returns>An <see cref="ARWorldMapRequest"/> which can be used to determine the status
        /// of the request and get the <c>ARWorldMap</c> when complete.</returns>
        /// <seealso cref="ApplyWorldMap(ARWorldMap)"/>
        /// <seealso cref="worldMapSupported"/>
        public ARWorldMapRequest GetARWorldMapAsync()
        {
            var requestId = NativeApi.UnityARKit_createWorldMapRequest();
            return new ARWorldMapRequest(requestId);
        }

        /// <summary>
        /// <para>
        /// Asynchronously create an <see cref="ARWorldMap"/>. An <c>ARWorldMap</c>
        /// represents the state of the session and can be serialized to a byte
        /// array to persist the session data, or send it to another device for
        /// shared AR experiences.
        /// </para>
        /// <para>
        /// It is a wrapper for <a href="https://developer.apple.com/documentation/arkit/arworldmap">ARKit's ARWorldMap</a>.
        /// </para>
        /// <para>
        /// If the <see cref="ARWorldMapRequestStatus"/> is <see cref="ARWorldMapRequestStatus.Success"/>, then
        /// the resulting <see cref="ARWorldMap"/> must be disposed to avoid leaking native resources. Otherwise,
        /// the <see cref="ARWorldMap"/> is not valid, and need not be disposed.
        /// </para>
        /// </summary>
        /// <param name="onComplete">A method to invoke when the world map has either been created, or determined
        /// that it could not be created. Check the value of the <see cref="ARWorldMapRequestStatus"/> parameter
        /// to determine whether the world map was successfully created.</param>
        /// <seealso cref="ApplyWorldMap(ARWorldMap)"/>
        /// <seealso cref="worldMapSupported"/>
        public void GetARWorldMapAsync(
            Action<ARWorldMapRequestStatus, ARWorldMap> onComplete)
        {
            var handle = GCHandle.Alloc(onComplete);
            var context = GCHandle.ToIntPtr(handle);

            NativeApi.UnityARKit_createWorldMapRequestWithCallback(s_OnAsyncWorldMapCompleted, context);
        }

        /// <summary>
        /// Detect <see cref="ARWorldMap"/> support. <c>ARWorldMap</c> requires iOS 12 or greater.
        /// </summary>
        /// <returns><c>true</c> if <c>ARWorldMap</c>s are supported, otherwise <c>false</c>.</returns>
        /// <seealso cref="GetARWorldMapAsync()"/>
        public bool worldMapSupported
        {
            get
            {
                return NativeApi.UnityARKit_worldMapSupported();
            }
        }

        /// <summary>
        /// Get the world mapping status. Used to determine the suitability of the current session for
        /// creating an <see cref="ARWorldMap"/>.
        /// </summary>
        /// <returns>The <see cref="ARWorldMappingStatus"/> of the session.</returns>
        public ARWorldMappingStatus worldMappingStatus
        {
            get
            {
                return NativeApi.UnityARKit_session_getWorldMappingStatus();
            }
        }

        /// <summary>
        /// Apply an existing <see cref="ARWorldMap"/> to the session. This will attempt
        /// to relocalize the current session to the given <paramref name="worldMap"/>.
        /// If relocalization is successful, the stored planes & reference points from
        /// the <paramref name="worldMap"/> will be added to the current session.
        /// This is equivalent to setting the <a href="https://developer.apple.com/documentation/arkit/arworldtrackingconfiguration/2968180-initialworldmap">initialWorldMap</a>
        /// property on the session's <a href="https://developer.apple.com/documentation/arkit/arworldtrackingconfiguration">ARWorldTrackingConfiguration</a>.
        /// </summary>
        /// <param name="worldMap">An <see cref="ARWorldMap"/> with which to relocalize the session.</param>
        public void ApplyWorldMap(
            ARWorldMap worldMap)
        {
            if (worldMap.nativeHandle == ARWorldMap.k_InvalidHandle)
                throw new InvalidOperationException("ARWorldMap has been disposed.");

            NativeApi.UnityARKit_applyWorldMap(worldMap.nativeHandle);
        }

        /// <summary>
        /// Creates the provider interface.
        /// </summary>
        /// <returns>The provider interface for ARKit</returns>
        protected override IProvider CreateProvider()
        {
            return new Provider();
        }

        static ARKitSessionSubsystem()
        {
            s_OnAsyncWorldMapCompleted = OnAsyncConversionComplete;
        }

        static NativeApi.OnAsyncConversionCompleteDelegate s_OnAsyncWorldMapCompleted;

        [MonoPInvokeCallback(typeof(NativeApi.OnAsyncConversionCompleteDelegate))]
        static unsafe void OnAsyncConversionComplete(ARWorldMapRequestStatus status, int worldMapId, IntPtr context)
        {
            var handle = GCHandle.FromIntPtr(context);
            var onComplete = (Action<ARWorldMapRequestStatus, ARWorldMap>)handle.Target;

            if (status.IsError())
            {
                onComplete(status, default(ARWorldMap));
            }
            else
            {
                var worldMap = new ARWorldMap(worldMapId);
                onComplete(status, worldMap);
            }

            handle.Free();
        }

        class Provider : IProvider
        {
            public Provider()
            {
                NativeApi.UnityARKit_session_construct();
            }

            public override void Resume()
            {
                NativeApi.UnityARKit_session_resume();
            }

            public override void Pause()
            {
                NativeApi.UnityARKit_session_pause();
            }

            public override void Update(XRSessionUpdateParams updateParams)
            {
                NativeApi.UnityARKit_session_update();
            }

            public override void Destroy()
            {
                NativeApi.UnityARKit_session_destroy();
            }

            public override void Reset()
            {
                NativeApi.UnityARKit_session_reset();
            }

            public override void OnApplicationPause()
            { }

            public override void OnApplicationResume()
            { }

            public override Promise<SessionAvailability> GetAvailabilityAsync()
            {
                var result = NativeApi.UnityARKit_session_getAvailability();
                var retVal = SessionAvailability.None;
                if (result == NativeApi.Availability.Supported)
                    retVal = SessionAvailability.Installed | SessionAvailability.Supported;

                return Promise<SessionAvailability>.CreateResolvedPromise(retVal);
            }

            public override Promise<SessionInstallationStatus> InstallAsync()
            {
                throw new NotSupportedException("ARKit cannot be installed.");
            }

            public override IntPtr nativePtr
            {
                get
                {
                    return NativeApi.UnityARKit_session_getNativePtr();
                }
            }

            public override TrackingState trackingState
            {
                get { return NativeApi.UnityARKit_session_getTrackingState(); }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDescriptor()
        {
            XRSessionSubsystemDescriptor.RegisterDescriptor(new XRSessionSubsystemDescriptor.Cinfo
            {
                id = "ARKit-Session",
                subsystemImplementationType = typeof(ARKitSessionSubsystem),
                supportsInstall = false
            });
        }

        static class NativeApi
        {
            // Should match ARKitAvailability in ARKitXRSessionProvider.mm
            public enum Availability
            {
                None,
                Supported
            }

            public delegate void OnAsyncConversionCompleteDelegate(
                ARWorldMapRequestStatus status,
                int worldMapId,
                IntPtr context);

#if UNITY_IOS && !UNITY_EDITOR
            [DllImport("__Internal")]
            public static extern int UnityARKit_createWorldMapRequest();

            [DllImport("__Internal")]
            public static extern void UnityARKit_createWorldMapRequestWithCallback(
                OnAsyncConversionCompleteDelegate callback,
                IntPtr context);

            [DllImport("__Internal")]
            public static extern bool UnityARKit_worldMapSupported();

            [DllImport("__Internal")]
            public static extern ARWorldMappingStatus UnityARKit_session_getWorldMappingStatus();

            [DllImport("__Internal")]
            public static extern void UnityARKit_applyWorldMap(int worldMapId);

            [DllImport("__Internal")]
            public static extern IntPtr UnityARKit_session_getNativePtr();

            [DllImport("__Internal")]
            public static extern Availability UnityARKit_session_getAvailability();

            [DllImport("__Internal")]
            public static extern void UnityARKit_session_update();

            [DllImport("__Internal")]
            public static extern void UnityARKit_session_construct();

            [DllImport("__Internal")]
            public static extern void UnityARKit_session_destroy();

            [DllImport("__Internal")]
            public static extern void UnityARKit_session_resume();

            [DllImport("__Internal")]
            public static extern void UnityARKit_session_pause();

            [DllImport("__Internal")]
            public static extern void UnityARKit_session_reset();

            [DllImport("__Internal")]
            public static extern TrackingState UnityARKit_session_getTrackingState();
#else
            public static int UnityARKit_createWorldMapRequest()
            {
                return default(int);
            }

            public static void UnityARKit_createWorldMapRequestWithCallback(
                OnAsyncConversionCompleteDelegate callback,
                IntPtr context)
            {
                callback(ARWorldMapRequestStatus.ErrorNotSupported, ARWorldMap.k_InvalidHandle, context);
            }

            public static bool UnityARKit_worldMapSupported()
            {
                return false;
            }

            public static ARWorldMappingStatus UnityARKit_session_getWorldMappingStatus()
            {
                return ARWorldMappingStatus.NotAvailable;
            }

            public static void UnityARKit_applyWorldMap(int worldMapId)
            { }

            public static IntPtr UnityARKit_session_getNativePtr()
            {
                return IntPtr.Zero;
            }

            public static Availability UnityARKit_session_getAvailability()
            {
                return Availability.None;
            }

            public static void UnityARKit_session_update()
            { }

            public static void UnityARKit_session_construct()
            { }

            public static void UnityARKit_session_destroy()
            { }

            public static void UnityARKit_session_resume()
            { }

            public static void UnityARKit_session_pause()
            { }

            public static void UnityARKit_session_reset()
            { }

            public static TrackingState UnityARKit_session_getTrackingState()
            {
                return TrackingState.None;
            }
#endif
        }
    }
}
