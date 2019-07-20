using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation
{
    /// <summary>
    /// Represents a detected point cloud, aka feature points.
    /// </summary>
    [DefaultExecutionOrder(ARUpdateOrder.k_PointCloud)]
    [DisallowMultipleComponent]
    [HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@1.5/api/UnityEngine.XR.ARFoundation.ARPointCloud.html")]
    public class ARPointCloud : ARTrackable<XRPointCloud, ARPointCloud>
    {
        /// <summary>
        /// Invoked whenever the point cloud is updated.
        /// </summary>
        public event Action<ARPointCloudUpdatedEventArgs> updated;

        /// <summary>
        /// An array of positions for each point in the point cloud.
        /// This array is parallel to <see cref="identifiers"/> and
        /// <see cref="confidenceValues"/>. Check for existence with
        /// <c>positions.IsCreated</c>. Positions are provided in
        /// point cloud space, that is, relative to this <see cref="ARPointCloud"/>'s
        /// local position and rotation.
        /// </summary>
        public NativeArray<Vector3> positions
        {
            get
            {
                return GetUndisposable(m_Data.positions);
            }
        }

        /// <summary>
        /// An array of identifiers for each point in the point cloud.
        /// This array is parallel to <see cref="positions"/> and
        /// <see cref="confidenceValues"/>. Check for existence with
        /// <c>identifiers.IsCreated</c>.
        /// </summary>
        public NativeArray<ulong> identifiers
        {
            get
            {
                return GetUndisposable(m_Data.identifiers);
            }
        }

        /// <summary>
        /// An array of confidence values for each point in the point cloud
        /// ranging from 0..1.
        /// This array is parallel to <see cref="positions"/> and
        /// <see cref="identifiers"/>. Check for existence with
        /// <c>confidenceValues.IsCreated</c>.
        /// </summary>
        public NativeArray<float> confidenceValues
        {
            get
            {
                return GetUndisposable(m_Data.confidenceValues);
            }
        }

        void Update()
        {
            if (m_PointsUpdated && updated != null)
            {
                m_PointsUpdated = false;
                updated(new ARPointCloudUpdatedEventArgs());
            }
        }

        void OnDestroy()
        {
            m_Data.Dispose();
        }

        // Creates an alias to the same array, but the caller cannot Dispose it.
        unsafe NativeArray<T> GetUndisposable<T>(NativeArray<T> disposable) where T : struct
        {
            if (!disposable.IsCreated)
                return default(NativeArray<T>);

            var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
                disposable.GetUnsafePtr(),
                disposable.Length,
                Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(
                    ref array,
                    NativeArrayUnsafeUtility.GetAtomicSafetyHandle(disposable));
#endif

            return array;
        }

        internal void UpdateData(XRDepthSubsystem subsystem)
        {
            m_Data.Dispose();
            m_Data = subsystem.GetPointCloudData(trackableId, Allocator.Persistent);
            m_PointsUpdated = m_Data.positions.IsCreated;
        }

        XRPointCloudData m_Data;

        bool m_PointsUpdated = false;
    }
}
