/*
    created by Jiadong Chen

    email: chenjd1024@gmail.com
 */

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public static class BuildPostProcessing
{

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if(buildTarget != BuildTarget.iOS)
        {
            return;
        }

        string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
        
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);
        var targetGUID = proj.TargetGuidByName("Unity-iPhone");

        //set xcode proj properties
        proj.AddBuildProperty(targetGUID, "SWIFT_VERSION", "4.0");
        proj.SetBuildProperty(targetGUID, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Plugins/iOS/HandDetector/Native/HandDetector.h");
        proj.SetBuildProperty(targetGUID, "SWIFT_OBJC_INTERFACE_HEADER_NAME","HandDetector-Swift.h");
        proj.SetBuildProperty(targetGUID, "COREML_CODEGEN_LANGUAGE", "Swift");
        
        
        //add handmodel to xcode proj build phase.
        var buildPhaseGUID = proj.AddSourcesBuildPhase(targetGUID);
        var handModelPath = Application.dataPath + "/../CoreML/HandModel.mlmodel";
        var fileGUID = proj.AddFile(handModelPath, "/HandModel.mlmodel");
        proj.AddFileToBuildSection(targetGUID, buildPhaseGUID, fileGUID);
        
        proj.WriteToFile(projPath);

    }
}
