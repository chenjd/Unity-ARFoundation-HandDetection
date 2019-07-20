#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using UnityEditor.iOS.Xcode;

namespace UnityEditor.XR.ARKit
{
    internal class XcodeAssetCatalog
    {
        public string name { get; set; }

        public XcodeAssetCatalog(string name)
        {
            this.name = name;
        }

        public void AddResourceGroup(ARResourceGroup group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            if (m_ResourceGroups.Contains(group))
                throw new InvalidOperationException(string.Format("Duplicate resource group '{0}'", group.name));

            m_ResourceGroups.Add(group);
        }

        public void WriteAndAddToPBXProject(PBXProject project, string pathToBuiltProject)
        {
            var unityTargetName = "Unity-iPhone";
            var relativePathToAssetCatalog = Path.Combine(unityTargetName, name + ".xcassets");
            var fullPathToAssetCatalogPath = Path.Combine(pathToBuiltProject, relativePathToAssetCatalog);

            // Create the asset catalog, destroying an existing one.
            if (Directory.Exists(fullPathToAssetCatalogPath))
                Directory.Delete(fullPathToAssetCatalogPath, true);
            Directory.CreateDirectory(fullPathToAssetCatalogPath);

            // Add it to Xcode's build
            var folderGuid = project.AddFile(fullPathToAssetCatalogPath, relativePathToAssetCatalog);
#if UNITY_2019_3_OR_NEWER
            var targetGuid = project.GetUnityMainTargetGuid();
#else
            var targetGuid = project.TargetGuidByName(unityTargetName);
#endif
            project.AddFileToBuild(targetGuid, folderGuid);

            foreach (var resourceGroup in m_ResourceGroups)
            {
                resourceGroup.Write(fullPathToAssetCatalogPath);
            }
        }

        List<ARResourceGroup> m_ResourceGroups = new List<ARResourceGroup>();
    }
}
#endif
