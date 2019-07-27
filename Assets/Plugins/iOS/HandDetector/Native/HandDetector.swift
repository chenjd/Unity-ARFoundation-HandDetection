//
//  HandDetector.swift
//  HandDetector
//
//  Created by jiadong chen on 21/07/2019.
//  Copyright © 2019 Jiadong Chen. All rights reserved.
//

import Foundation
import Vision

@objc public class HandDetector: NSObject{
    
    @objc static let shared = HandDetector()
    @objc public var outputBuffer : CVPixelBuffer?
    
    private let visionQueue = DispatchQueue(label: "com.chenjd.HandDetector")
    
    private var callbackTarget: String = "HandDetector"
    
    private var retainedBuffer: CVPixelBuffer?
    
    private lazy var predictionRequest: VNCoreMLRequest = {
        // Load the ML model through its generated class and create a Vision request for it.
        do {
            let model = try VNCoreMLModel(for: HandModel().model)
            let request = VNCoreMLRequest(model: model, completionHandler: detectionCompleteHandler)
            
            request.imageCropAndScaleOption = VNImageCropAndScaleOption.scaleFill
            return request
        } catch {
            fatalError("can't load Vision ML model: \(error)")
        }
    }()
    
    
    //region methods
    @objc func startDetection(buffer: CVPixelBuffer) -> Bool {
        //TODO
        self.retainedBuffer = buffer
        let imageRequestHandler = VNImageRequestHandler(cvPixelBuffer: self.retainedBuffer!, orientation: .right)
        
        visionQueue.async {
            do {
                defer { self.retainedBuffer = nil }
                try imageRequestHandler.perform([self.predictionRequest])
            } catch {
                fatalError("Perform Failed:\"\(error)\"")
            }
        }
        
        return true
    }
    
    
    private func detectionCompleteHandler(request: VNRequest, error: Error?) {
        
        DispatchQueue.main.async {
            
            if(error != nil) {
                UnitySendMessage(self.callbackTarget, "OnHandDetecedFromNative", "")
                fatalError("error\(error)")
            }
            
            guard let observation = self.predictionRequest.results?.first as? VNPixelBufferObservation else {
                UnitySendMessage(self.callbackTarget, "OnHandDetecedFromNative", "")
                fatalError("Unexpected result type from VNCoreMLRequest")
            }
            
            let outBuffer = observation.pixelBuffer
            
            self.outputBuffer = observation.pixelBuffer
            
            guard let point = outBuffer.searchTopPoint() else{
                UnitySendMessage(self.callbackTarget, "OnHandDetecedFromNative", "")
                return
            }
            
            UnitySendMessage(self.callbackTarget, "OnHandDetecedFromNative", "\(point.x),\(point.y)")
        }
    }


}
