#import <Foundation/Foundation.h>
#include "IUnityInterface.h"
#include "UnityAppController.h"

#ifdef __cplusplus
extern "C" {
#endif
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityARKitXRPlugin_PluginLoad(IUnityInterfaces* unityInterfaces);
void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityARKitXRPlugin_SetupiOS(CGSize (UNITY_INTERFACE_API * GetUnityRootViewSizeFuncPtr)());
#ifdef __cplusplus
} // extern "C"
#endif

CGSize UNITY_INTERFACE_API GetUnityRootViewSize()
{
    UnityAppController* appController = _UnityAppController;
    return appController.rootView.bounds.size;
}

@interface UnityARKit : NSObject

+ (void)loadPlugin;

@end

@implementation UnityARKit

+ (void)loadPlugin
{
    UnityRegisterRenderingPluginV5(UnityARKitXRPlugin_PluginLoad, NULL);
    UnityARKitXRPlugin_SetupiOS(&GetUnityRootViewSize);
}

@end
