
//
//  HandDetectorBridge.m
//  HandDetector
//
//  Created by jiadong chen on 21/07/2019.
//  Copyright © 2019 Jiadong Chen. All rights reserved.
//
#import "HandDetector-Swift.h"
#import <Foundation/Foundation.h>
#import "UnityXRNativePtrs.h"
#import <ARKit/ARKit.h>
#import <Metal/Metal.h>
#import <AVFoundation/AVFoundation.h>
#import "UnityAppController.h"
#include "Unity/IUnityInterface.h"


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload();

@interface MyAppController : UnityAppController
{
}
- (void)shouldAttachRenderDelegate;
@end
@implementation MyAppController
- (void)shouldAttachRenderDelegate;
{
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
}
@end
IMPL_APP_CONTROLLER_SUBCLASS(MyAppController);


//region fields
static id<MTLTexture> s_CaptureTexture = nil;
static CVMetalTextureCacheRef s_TextureCache = nil;
static id <MTLDevice> s_Device = nil;
static CVPixelBufferRef s_PixelBuffer = nil;

static IUnityInterfaces*    s_UnityInterfaces   = 0;
//static IUnityGraphics*      s_Graphics          = 0;
//static IUnityGraphicsMetal* s_MetalGraphics     = 0;


//region functions
extern "C" {
    
    bool HandDetector_StartDetect(intptr_t ptr) {
        
        // In case of invalid buffer ref
        if (!ptr) return 0;
        
        UnityXRNativeFrame_1* unityXRFrame = (UnityXRNativeFrame_1*) ptr;
        ARFrame* frame = (__bridge ARFrame*)unityXRFrame->framePtr;
        
        CVPixelBufferRef buffer = frame.capturedImage;
        // Forward message to the swift api
        return [[HandDetector shared] startDetectionWithBuffer: buffer] ? 1 : 0;
    }
    
    uintptr_t HandDetector_GetDetectedTexture()
    {
        s_PixelBuffer = [[HandDetector shared] outputBuffer];
        size_t width = CVPixelBufferGetWidthOfPlane(s_PixelBuffer, 0);
        size_t height = CVPixelBufferGetHeightOfPlane(s_PixelBuffer, 0);
        MTLPixelFormat pixelFormat = MTLPixelFormatRGBA8Unorm;//MTLPixelFormatR8Unorm;
        
        //
        CVPixelBufferRef copy = NULL;
        NSDictionary *attributes = @{
                                     @"IOSurfaceCoreAnimationCompatibility": @YES
                                     };
        CVPixelBufferCreate(kCFAllocatorDefault,
                            CVPixelBufferGetWidth(s_PixelBuffer),
                            CVPixelBufferGetHeight(s_PixelBuffer),
                            CVPixelBufferGetPixelFormatType(s_PixelBuffer),
                            (__bridge CFDictionaryRef)attributes,
                            &copy);
        CVPixelBufferLockBaseAddress(s_PixelBuffer, kCVPixelBufferLock_ReadOnly);
        CVPixelBufferLockBaseAddress(copy, 0);
        
        void *baseAddress = CVPixelBufferGetBaseAddress(s_PixelBuffer);
        void *copyBaseAddress = CVPixelBufferGetBaseAddress(copy);
        
        memcpy(copyBaseAddress, baseAddress, CVPixelBufferGetDataSize(s_PixelBuffer));
        
        CVPixelBufferUnlockBaseAddress(copy, 0);
        CVPixelBufferUnlockBaseAddress(s_PixelBuffer, kCVPixelBufferLock_ReadOnly);
        //
        
        CVMetalTextureRef texture = NULL;
        CVReturn status = CVMetalTextureCacheCreateTextureFromImage(NULL, s_TextureCache, copy, NULL, pixelFormat, width, height, 0, &texture);
        
        if(status == kCVReturnSuccess)
        {
            s_CaptureTexture = CVMetalTextureGetTexture(texture);
            CFRelease(texture);
        }
        
        return (uintptr_t)(__bridge_retained void*)s_CaptureTexture;
    }
}


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    s_Device = MTLCreateSystemDefaultDevice();
    CVMetalTextureCacheCreate(NULL, NULL, s_Device, NULL, &s_TextureCache);
    AVCaptureVideoDataOutput *outputCapture = [[AVCaptureVideoDataOutput alloc] init];
    outputCapture.videoSettings = @{
        (NSString *)kCVPixelBufferMetalCompatibilityKey: @YES,
        (NSString*)kCVPixelBufferPixelFormatTypeKey : @(kCVPixelFormatType_420YpCbCr8BiPlanarVideoRange)
    };
    
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
}
