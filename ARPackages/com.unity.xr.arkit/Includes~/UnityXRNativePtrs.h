#pragma once

// Various AR Subsystems have GetNativePtr methods on them, which return
// pointers to the following structs. The first field will always
// be a version number, so code which tries to interpret the native
// pointers can safely check the version prior to casting to the
// appropriate struct.

// XRSessionExtensions.GetNativePtr
typedef struct UnityXRNativeSession_1
{
    int version;
    void* sessionPtr;
} UnityXRNativeSession_1;

// XRPlaneExtensions.GetNativePtr
typedef struct UnityXRNativePlane_1
{
    int version;
    void* planePtr;
} UnityXRNativePlane_1;

// XRReferencePointExtensions.GetNativePtr
typedef struct UnityXRNativeReferencePoint_1
{
    int version;
    void* referencePointPtr;
} UnityXRNativeReferencePoint_1;

static const int kUnityXRNativeSessionVersion = 1;
static const int kUnityXRNativePlaneVersion = 1;
static const int kUnityXRNativeReferencePointVersion = 1;

typedef UnityXRNativeSession_1 UnityXRNativeSession;
typedef UnityXRNativePlane_1 UnityXRNativePlane;
typedef UnityXRNativeReferencePoint_1 UnityXRNativeReferencePoint;
