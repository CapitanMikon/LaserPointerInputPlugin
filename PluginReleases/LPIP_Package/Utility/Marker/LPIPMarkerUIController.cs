using System;
using UnityEngine;

//namespace LPIP_Package.UI.Marker
//{
public class LPIPMarkerUIController : MonoBehaviour
    {
        [SerializeField] private GameObject marker;
        
        private readonly Vector3 _laserPointerMarketDefaultPosition = new Vector3(-100, -100, 0);


        private void Start()
        {
            ResetLaserMarkerPos(Vector2.zero);
        }

        private void OnEnable()
        {
            LPIPCoreManager.OnLaserHitDownDetectedEvent += UpdateLaserMarkerPos;
            LPIPCoreManager.OnLaserHitUpDetectedEvent += ResetLaserMarkerPos;
            LPIPCoreManager.OnDetectionStoppedEvent += ResetLaserMarkerPos;
        }

        private void OnDisable()
        {
            LPIPCoreManager.OnLaserHitDownDetectedEvent -= UpdateLaserMarkerPos;
            LPIPCoreManager.OnLaserHitUpDetectedEvent -= ResetLaserMarkerPos;
            LPIPCoreManager.OnDetectionStoppedEvent -= ResetLaserMarkerPos;
        }

        private void UpdateLaserMarkerPos(Vector2 position)
        {
            marker.transform.position = position;
        }
    
        private void ResetLaserMarkerPos(Vector2 position)
        {
            Debug.LogWarning("Marker was reset off screen!");
            marker.transform.position = _laserPointerMarketDefaultPosition;
        }
        private void ResetLaserMarkerPos()
        {
            Debug.LogWarning("Marker was reset off screen!");
            marker.transform.position = _laserPointerMarketDefaultPosition;
        }
    }
//}