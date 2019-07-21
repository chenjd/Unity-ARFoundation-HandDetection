
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
}
