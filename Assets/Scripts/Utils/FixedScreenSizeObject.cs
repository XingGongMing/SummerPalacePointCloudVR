using System;
using UnityEngine;

namespace Utils
{
    public class FixedScreenSizeObject : MonoBehaviour
    {
       public static FixedScreenSizeObject Instance; 
        
        public float targetHeightInPixels = 100f; // 你希望它在屏幕上显示的高度
        public Camera mainCamera;


        private void Awake()
        {
            Instance = this;
        }

        void LateUpdate1()
        {
            if (mainCamera == null) return;

            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

            // 根据视场角和距离计算合适的 scale
            float objectHeightInUnits = DistanceToHeight(distance, mainCamera.fieldOfView);
            float desiredScale = targetHeightInPixels / Screen.height * objectHeightInUnits * mainCamera.pixelHeight;

            transform.localScale = Vector3.one * desiredScale;
        }

        float DistanceToHeight(float distance, float fov)
        {
            return 2f * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * distance;
        }

        public Vector3 GetScale()
        {
            float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

            // 根据视场角和距离计算合适的 scale
            float objectHeightInUnits = DistanceToHeight(distance, mainCamera.fieldOfView);
            float desiredScale = targetHeightInPixels / Screen.height * objectHeightInUnits * mainCamera.pixelHeight;

            return Vector3.one * desiredScale;
        }
    }
}