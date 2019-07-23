//
//  CVPixelBuffer+Search.swift
//  ARML
//
//  Created by Gil Nakache on 31/01/2019.
//  Copyright © 2019 viseo. All rights reserved.
//

import CoreVideo

extension CVPixelBuffer {
    func searchTopPoint() -> CGPoint? {
        // Get width and height of buffer
        let width = CVPixelBufferGetWidth(self)
        let height = CVPixelBufferGetHeight(self)

        let bytesPerRow = CVPixelBufferGetBytesPerRow(self)

        // Lock buffer
        CVPixelBufferLockBaseAddress(self, CVPixelBufferLockFlags(rawValue: 0))

        // Unlock buffer upon exiting
        defer {
            CVPixelBufferUnlockBaseAddress(self, CVPixelBufferLockFlags(rawValue: 0))
        }

        var returnPoint: CGPoint?

        var whitePixelsCount = 0

        if let baseAddress = CVPixelBufferGetBaseAddress(self) {
            let buffer = baseAddress.assumingMemoryBound(to: UInt8.self)

            // we look at pixels from bottom to top
            for y in (0 ..< height).reversed() {
                for x in (0 ..< width).reversed() {
                    // We look at top groups of 5 non black pixels
                    let pixel = buffer[y * bytesPerRow + x * 4]
                    let abovePixel = buffer[min(y + 1, height) * bytesPerRow + x * 4]
                    let belowPixel = buffer[max(y - 1, 0) * bytesPerRow + x * 4]
                    let rightPixel = buffer[y * bytesPerRow + min(x + 1, width) * 4]
                    let leftPixel = buffer[y * bytesPerRow + max(x - 1, 0) * 4]

                    if pixel > 0 && abovePixel > 0 && belowPixel > 0 && rightPixel > 0 && leftPixel > 0 {
                        let newPoint = CGPoint(x: x, y: y)
                        // we return a normalized point (0-1)
                        returnPoint = CGPoint(x: newPoint.x / CGFloat(width), y: newPoint.y / CGFloat(height))
                        whitePixelsCount += 1
                    }
                }
            }
        }

        // We count the number of pixels in our frame. If the number is too low then we return nil because it means it's detecting a false positive
        if whitePixelsCount < 30 {
            returnPoint = nil
        }

        return returnPoint
    }
}
