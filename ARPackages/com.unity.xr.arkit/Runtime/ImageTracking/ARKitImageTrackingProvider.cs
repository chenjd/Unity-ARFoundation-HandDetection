using AOT;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.iOS;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARKit
{
    [Preserve]
    /// <summary>
    /// ARKit implementation of the <c>XRImageTrackingSubsystem</c>. You shouldn't
    /// need to interact directly with this unless using the <see cref="maximumNumberOfTrackedImages"/>
    /// property.
    /// </summary>
    public sealed class ARKitImageTrackingSubsystem : XRImageTrackingSubsystem
    {
        class Provider : IProvider
        {
            public override XRReferenceImageLibrary imageLibrary
            {
                set
                {
                    if (value == null)
                    {
                        UnityARKit_imageTracking_stop();
                        return;
                    }

                    var nativeReturnCode = UnityARKit_imageTracking_trySetReferenceLibrary(value.name, value.name.Length, value.guid);

                    switch (nativeReturnCode)
                    {
                        case SetReferenceLibraryResult.Success:
                            return;

                        case SetReferenceLibraryResult.FeatureUnavailable:
                            throw new InvalidOperationException(string.Format(
                                "Failed to set requested image library '{0}' on ARKit - this feature only works on versions of ARKit 11.3 and newer.",
                                value.name));

                        case SetReferenceLibraryResult.ResourceDoesNotExist:
                            throw new InvalidOperationException(string.Format(
                                "Failed to set requested image library '{0}' on ARKit - there is no matching resource group, or the resource group does not contain any reference images.",
                                value.name));
                    }
                }
            }

            public unsafe override TrackableChanges<XRTrackedImage> GetChanges(
                XRTrackedImage defaultTrackedImage,
                Allocator allocator)
            {
                void* addedPtr, updatedPtr, removedPtr;
                int addedLength, updatedLength, removedLength, stride;

                var context = UnityARKit_imageTracking_acquireChanges(
                    out addedPtr, out addedLength,
                    out updatedPtr, out updatedLength,
                    out removedPtr, out removedLength,
                    out stride);

                try
                {
                    return new TrackableChanges<XRTrackedImage>(
                        addedPtr, addedLength,
                        updatedPtr, updatedLength,
                        removedPtr, removedLength,
                        defaultTrackedImage, stride,
                        allocator);
                }
                finally
                {
                    UnityARKit_imageTracking_releaseChanges(context);
                }
            }

            public override void Destroy()
            {
                UnityARKit_imageTracking_destroy();
            }

            public override int maxNumberOfMovingImages
            {
                set
                {
                    UnityARKit_imageTracking_setMaximumNumberOfTrackedImages(value);
                }
            }
        }

        [DllImport("__Internal")]
        static extern void UnityARKit_imageTracking_setMaximumNumberOfTrackedImages(
            int maxNumTrackedImages);

        [DllImport("__Internal")]
        static extern SetReferenceLibraryResult UnityARKit_imageTracking_trySetReferenceLibrary(
            [MarshalAs(UnmanagedType.LPWStr)] string name, int nameLength, Guid guid);

        [DllImport("__Internal")]
        static extern void UnityARKit_imageTracking_stop();

        [DllImport("__Internal")]
        static extern void UnityARKit_imageTracking_destroy();

        [DllImport("__Internal")]
        static extern unsafe void* UnityARKit_imageTracking_acquireChanges(
            out void* addedPtr, out int addedLength,
            out void* updatedPtr, out int updatedLength,
            out void* removedPtr, out int removedLength,
            out int stride);

        [DllImport("__Internal")]
        static extern unsafe void UnityARKit_imageTracking_releaseChanges(void* changes);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterDescriptor()
        {
            float iOSversion = float.Parse(Device.systemVersion, CultureInfo.InvariantCulture.NumberFormat);

            // No support before iOS 11.3
            if (iOSversion < 11.3)
                return;

#if UNITY_IOS && !UNITY_EDITOR
            XRImageTrackingSubsystemDescriptor.Create(new XRImageTrackingSubsystemDescriptor.Cinfo
            {
                id = "ARKit-ImageTracking",
                subsystemImplementationType = typeof(ARKitImageTrackingSubsystem),
                supportsMovingImages = iOSversion >= 12f
            });
#endif
        }

        protected override IProvider CreateProvider()
        {
            return new Provider();
        }
    }
}
