using System;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARKit
{
    /// <summary>
    /// The ARKit implementation of the <c>XRDepthSubsystem</c>. Do not create this directly. Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public sealed class ARKitXRDepthSubsystem : XRDepthSubsystem
    {
        class Provider : IDepthApi
        {
#if UNITY_IOS && !UNITY_EDITOR
            [DllImport("__Internal")]
            static extern void UnityARKit_depth_destroy();

            [DllImport("__Internal")]
            static extern void UnityARKit_depth_start();

            [DllImport("__Internal")]
            static extern void UnityARKit_depth_stop();

            [DllImport("__Internal")]
            static extern unsafe void* UnityARKit_depth_acquireChanges(
                out void* addedPtr, out int addedLength,
                out void* updatedPtr, out int updatedLength,
                out void* removedPtr, out int removedLength,
                out int elementSize);

            [DllImport("__Internal")]
            static extern unsafe void UnityARKit_depth_releaseChanges(
                void* changes);

            [DllImport("__Internal")]
            static extern unsafe void* UnityARKit_depth_acquirePointCloud(
                TrackableId trackableId,
                out void* positionsPtr, out void* identifiersPtr, out int numPoints);

            [DllImport("__Internal")]
            static extern unsafe void UnityARKit_depth_releasePointCloud(
                void* pointCloud);
#else
            static void UnityARKit_depth_destroy()
            { }

            static void UnityARKit_depth_start()
            { }

            static void UnityARKit_depth_stop()
            { }

            static unsafe void* UnityARKit_depth_acquireChanges(
                out void* addedPtr, out int addedLength,
                out void* updatedPtr, out int updatedLength,
                out void* removedPtr, out int removedLength,
                out int elementSize)
            {
                addedPtr = updatedPtr = removedPtr = null;
                addedLength = updatedLength = removedLength = elementSize = 0;
                return null;
            }

            static unsafe void UnityARKit_depth_releaseChanges(
                void* changes)
            { }

            static unsafe void* UnityARKit_depth_acquirePointCloud(
                TrackableId trackableId,
                out void* positionsPtr, out void* identifiersPtr, out int numPoints)
            {
                positionsPtr = identifiersPtr = null;
                numPoints = 0;
                return null;
            }

            static unsafe void UnityARKit_depth_releasePointCloud(
                void* pointCloud)
            { }
#endif

            public override unsafe TrackableChanges<XRPointCloud> GetChanges(
                XRPointCloud defaultPointCloud,
                Allocator allocator)
            {
                int addedLength, updatedLength, removedLength, elementSize;
                void* addedPtr, updatedPtr, removedPtr;

                var context = UnityARKit_depth_acquireChanges(
                    out addedPtr, out addedLength,
                    out updatedPtr, out updatedLength,
                    out removedPtr, out removedLength,
                    out elementSize);

                try
                {
                    return new TrackableChanges<XRPointCloud>(
                        addedPtr, addedLength,
                        updatedPtr, updatedLength,
                        removedPtr, removedLength,
                        defaultPointCloud, elementSize,
                        allocator);
                }
                finally
                {
                    UnityARKit_depth_releaseChanges(context);
                }
            }

            public override void Destroy()
            {
                UnityARKit_depth_destroy();
            }

            public override void Start()
            {
                UnityARKit_depth_start();
            }

            public override void Stop()
            {
                UnityARKit_depth_stop();
            }

            public override unsafe XRPointCloudData GetPointCloudData(
                TrackableId trackableId,
                Allocator allocator)
            {
                void* positionsPtr, identifiersPtr;
                int numPoints;
                var pointCloud = UnityARKit_depth_acquirePointCloud(
                    trackableId,
                    out positionsPtr, out identifiersPtr, out numPoints);

                try
                {
                    var positions = new NativeArray<Vector3>(numPoints, allocator);
                    var positionsHandle = new TransformPositionsJob
                    {
                        positionsIn = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Quaternion>(positionsPtr, numPoints, Allocator.None),
                        positionsOut = positions
                    }.Schedule(numPoints, 32);

                    var identifiers = new NativeArray<ulong>(numPoints, allocator);
                    identifiers.CopyFrom(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<ulong>(identifiersPtr, numPoints, Allocator.None));

                    positionsHandle.Complete();
                    return new XRPointCloudData
                    {
                        positions = positions,
                        identifiers = identifiers
                    };
                }
                finally
                {
                    UnityARKit_depth_releasePointCloud(pointCloud);
                }
            }
        }

        struct TransformPositionsJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Quaternion> positionsIn;

            [WriteOnly]
            public NativeArray<Vector3> positionsOut;

            public void Execute(int index)
            {
                positionsOut[index] = new Vector3(
                     positionsIn[index].x,
                     positionsIn[index].y,
                    -positionsIn[index].z);
            }
        }

        protected override IDepthApi GetInterface()
        {
            return new Provider();
        }

        //this method is run on startup of the app to register this provider with XR Subsystem Manager
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDescriptor()
        {
            var descriptorParams = new XRDepthSubsystemDescriptor.Cinfo
            {
                id = "ARKit-Depth",
                implementationType = typeof(ARKitXRDepthSubsystem),
                supportsFeaturePoints = true,
                supportsConfidence = false,
                supportsUniqueIds = true
            };

            XRDepthSubsystemDescriptor.RegisterDescriptor(descriptorParams);
        }
    }
}
