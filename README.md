# 2510 - SUPERSELL -Assignment - Face Tracking Camera Control

## 📖 Developer: Dau Phuc Chuong

Email: chuongdp161@gmail.com

## 🛠️ Libraries và Dependencies

### Core Libraries
- **MediaPipe Unity Plugin**: https://github.com/homuler/MediaPipeUnityPlugin
- **Unity Input System**
- **Universal Render Pipeline (URP)**
- **TextMeshPro**

### MediaPipe Components
- **Face Landmark Detection**

### Input
- **Camera Logitech C922 Pro**

## 🎯 Face Tracking và Camera Synchronization

### System Architecture

```
Webcam Input → MediaPipe Face Detection → Face Processing → Camera Control
```

### Detailed processing flow

#### 1. **Face Detection Pipeline**
```csharp
FaceLandmarkerRunner → OnFaceResult → FaceToCameraDriver
```

#### 2. **Landmark Processing**
- **Eye left/right**: Points 33 and 263 (calculate yaw rotation)
- **Nose**: Point 1 (calculate pitch and position offset)
- **Normalized coordinates**: Convert from [0,1] to world coordinates

#### 3. **Camera Control Algorithm**
```csharp
// Calculate yaw from eye distance
var yawDeg = -Mathf.Atan2(dy, dx) * Mathf.Rad2Deg * (yawScaleDeg / 60f);

// Calculate pitch from nose position
var pitchDeg = -(nose.y - cy) * pitchScaleDeg;

// Calculate position offset
var offx = (nose.x - 0.5f) * 2f * xScaleMeters;
var offz = (0.5f - nose.y) * 2f * zScaleMeters;
```

#### 4. **Smoothing and Limits**
- **Position Lerp**: Smooth position motion
- **Rotation Slerp**: Smooth rotation motion
- **Limits**: Yaw ±45°, Pitch ±30°, Position ±2.5m

### Thread-Safe Communication

```csharp
// Thread-safe pose buffer
private readonly object externalPoseLock = new();
private bool hasNewExternalPose;
private float extYawDeg, extPitchDeg, extOffsetX, extOffsetZ;

public void SetExternalPose(float yawDeg, float pitchDeg, float offsetX, float offsetZ)
{
    lock (this.externalPoseLock)
    {
        this.extYawDeg = yawDeg;
        this.extPitchDeg = pitchDeg;
        this.extOffsetX = offsetX;
        this.extOffsetZ = offsetZ;
        this.hasNewExternalPose = true;
    }
}
```

## 🧪 Testing Environment

### Development Setup
- **Unity Version**: 6000.0.60f1 LTS
- **Platform**: Windows 11
- **Graphics**: OpenGL ES 3.0+ (Android), DirectX 11+ (Windows)
- **Camera**: Webcam Logitech C922 Pro