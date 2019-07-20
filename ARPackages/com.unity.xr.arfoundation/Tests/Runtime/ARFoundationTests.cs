using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.ARFoundation;

namespace UnityEngine.XR.ARFoundation
{
    [TestFixture]
    public class PoseExtensionsTestFixture
    {
        [Test]
        public void InverseTransformPositionTest()
        {
            Pose pose = default(Pose);
            Vector3 vec = new Vector3(1,2,3);

            Assert.That(vec == pose.InverseTransformPosition(vec), "Inverse transform position not the same as input for a default pose.");
        }
    }
}