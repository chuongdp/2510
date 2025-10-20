using Mediapipe.Tasks.Vision.FaceLandmarker;
using UnityEngine;

namespace SUPERSELL.HeadTracking
{
    public class FaceToCameraDriver : MonoBehaviour
    {
        [Header("Targets")] [SerializeField] private CameraController cameraController;

        [Header("Sensitivity / Scaling")] [SerializeField]
        private float yawScaleDeg = 90f;

        [SerializeField] private float pitchScaleDeg = 90f;
        [SerializeField] private float xScaleMeters  = 0.6f;
        [SerializeField] private float zScaleMeters  = 0.6f;

        [Header("Invert Axes")] [SerializeField]
        private bool invertYaw;

        [SerializeField] private bool invertPitch;
        [SerializeField] private bool invertX;
        [SerializeField] private bool invertZ;

        private int logCounter;

        private void OnEnable()
        {
            Mediapipe.Unity.Sample.FaceLandmarkDetection.FaceLandmarkerRunner.OnFaceResult += this.OnFace;
            Debug.Log("[FaceToCameraDriver] Subscribed to Face results");
            if (this.cameraController == null) this.cameraController = FindAnyObjectByType<CameraController>();
        }

        private void OnDisable() { Mediapipe.Unity.Sample.FaceLandmarkDetection.FaceLandmarkerRunner.OnFaceResult -= this.OnFace; }

        private void OnFace(FaceLandmarkerResult result)
        {
            if (this.cameraController == null) return;
            var faces = result.faceLandmarks;

            if (faces == null || faces.Count == 0) return;

            var lms = faces[0].landmarks; // 2D normalized landmarks

            if (lms == null || lms.Count == 0) return;

            var leftEye  = lms[Mathf.Clamp(33,  0, lms.Count - 1)];
            var rightEye = lms[Mathf.Clamp(263, 0, lms.Count - 1)];
            var nose     = lms[Mathf.Clamp(1,   0, lms.Count - 1)];

            var cx = (leftEye.x + rightEye.x) * 0.5f;
            var cy = (leftEye.y + rightEye.y) * 0.5f;
            var dx = rightEye.x - leftEye.x;
            var dy = rightEye.y - leftEye.y;

            var yawDeg   = -Mathf.Atan2(dy, dx) * Mathf.Rad2Deg * (this.yawScaleDeg / 60f);
            var pitchDeg = -(nose.y - cy)       * this.pitchScaleDeg;
            var offx     = (nose.x - 0.5f)      * 2f * this.xScaleMeters;
            var offz     = (0.5f   - nose.y)    * 2f * this.zScaleMeters;

            if (this.invertYaw) yawDeg     = -yawDeg;
            if (this.invertPitch) pitchDeg = -pitchDeg;
            if (this.invertX) offx         = -offx;
            if (this.invertZ) offz         = -offz;

            this.cameraController.SetExternalPose(yawDeg, pitchDeg, offx, offz);

            this.logCounter++;
            if (this.logCounter % 15 == 0) Debug.Log($"FacePose yaw={yawDeg:F1} pitch={pitchDeg:F1} x={offx:F2} z={offz:F2}");
        }
    }
}