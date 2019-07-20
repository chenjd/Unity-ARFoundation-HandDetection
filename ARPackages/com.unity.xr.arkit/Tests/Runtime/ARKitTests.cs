using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine.TestTools;
using UnityEngine.XR.ARSubsystems;

#if !UNITY_2019_2_OR_NEWER
using UnityEngine.Experimental;
#endif

namespace UnityEngine.XR.ARKit.Tests
{
    [TestFixture]
    public class ARKitTestFixture
    {
        [Test]
        public void DepthSubsystemRegistered()
        {
            Assert.That(SubsystemDescriptorRegistered<XRDepthSubsystemDescriptor>("ARKit-Depth"));
        }

        [Test]
        public void SessionSubsystemRegistered()
        {
            Assert.That(SubsystemDescriptorRegistered<XRSessionSubsystemDescriptor>("ARKit-Session"));
        }

        [Test]
        public void PlaneSubsystemRegistered()
        {
            Assert.That(SubsystemDescriptorRegistered<XRPlaneSubsystemDescriptor>("ARKit-Plane"));
        }

        [Test]
        public void RaycastSubsystemRegistered()
        {
            Assert.That(SubsystemDescriptorRegistered<XRRaycastSubsystemDescriptor>("ARKit-Raycast"));
        }
        [Test]
        public void ReferencePointSubsystemRegistered()
        {
            Assert.That(SubsystemDescriptorRegistered<XRReferencePointSubsystemDescriptor>("ARKit-ReferencePoint"));
        }

        [Test]
        public void CameraSubsystemRegistered()
        {
            Assert.That(SubsystemDescriptorRegistered<XRCameraSubsystemDescriptor>("ARKit-Camera"));
        }
        bool SubsystemDescriptorRegistered<T>(string id) where T : SubsystemDescriptor
        {
            List<T> descriptors = new List<T>();

            SubsystemManager.GetSubsystemDescriptors<T>(descriptors);

            foreach(T descriptor in descriptors)
            {
                if (descriptor.id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
