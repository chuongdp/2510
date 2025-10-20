using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Smoothing")] [SerializeField] private float positionLerpSpeed = 6f;
    [SerializeField]                       private float rotationLerpSpeed = 8f;

    [Header("Mapping")] [SerializeField] private Vector2 yawLimitsDeg   = new(-45f, 45f);
    [SerializeField]                     private Vector2 pitchLimitsDeg = new(-30f, 30f);
    [SerializeField]                     private Vector2 xLimitsMeters  = new(-2.5f, 2.5f);
    [SerializeField]                     private Vector2 zLimitsMeters  = new(-2.5f, 2.5f);

    private Quaternion targetRotation;
    private Vector3    targetPosition;
    private float      baseHeight;

    // Thread-safe external pose buffer
    private readonly object externalPoseLock = new();
    private          bool   hasNewExternalPose;
    private          float  extYawDeg;
    private          float  extPitchDeg;
    private          float  extOffsetX;
    private          float  extOffsetZ;

    private void Start()
    {
        this.targetRotation = this.transform.rotation;
        this.targetPosition = this.transform.position;
        this.baseHeight     = this.transform.position.y;
    }

    private void Update()
    {
        // Apply external pose captured from any thread, on main thread only
        if (this.hasNewExternalPose)
        {
            float yaw, pitch, offX, offZ;
            lock (this.externalPoseLock)
            {
                yaw                     = this.extYawDeg;
                pitch                   = this.extPitchDeg;
                offX                    = this.extOffsetX;
                offZ                    = this.extOffsetZ;
                this.hasNewExternalPose = false;
            }

            var clampedYaw   = Mathf.Clamp(yaw,   this.yawLimitsDeg.x,   this.yawLimitsDeg.y);
            var clampedPitch = Mathf.Clamp(pitch, this.pitchLimitsDeg.x, this.pitchLimitsDeg.y);
            var clampedX     = Mathf.Clamp(offX,  this.xLimitsMeters.x,  this.xLimitsMeters.y);
            var clampedZ     = Mathf.Clamp(offZ,  this.zLimitsMeters.x,  this.zLimitsMeters.y);

            this.targetRotation = Quaternion.Euler(-clampedPitch, clampedYaw, 0f);
            var basePos = new Vector3(0f, this.baseHeight, 0f);
            this.targetPosition = basePos + new Vector3(clampedX, 0f, clampedZ);
        }

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.targetRotation, 1f - Mathf.Exp(-this.rotationLerpSpeed * Time.deltaTime));
        this.transform.position = Vector3.Lerp(this.transform.position, this.targetPosition, 1f     - Mathf.Exp(-this.positionLerpSpeed * Time.deltaTime));
    }

    public void SetExternalPose(float yawDeg, float pitchDeg, float offsetX, float offsetZ)
    {
        lock (this.externalPoseLock)
        {
            this.extYawDeg          = yawDeg;
            this.extPitchDeg        = pitchDeg;
            this.extOffsetX         = offsetX;
            this.extOffsetZ         = offsetZ;
            this.hasNewExternalPose = true;
        }
    }
}