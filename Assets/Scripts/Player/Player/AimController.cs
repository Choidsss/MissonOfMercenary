using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class AimController : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;
        [SerializeField] AssultRifle _ar;
        [SerializeField] float _aimSpeed;

        [SerializeField] GunTransformCorrection _transformCorrection;

        public bool IsAiming { get; private set; } = false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //_transformCorrection = GetComponent<GunTransformCorrection>();
        }

        private void OnEnable()
        {
            _inputReader.OnZoomInToggleAction += AimHandle;
        }

        private void OnDisable()
        {
            _inputReader.OnZoomInToggleAction -= AimHandle;
        }


        void AimHandle()
        {
            if (_ar.aimType == AimType.IronSight) return;

            IsAiming = !IsAiming;

            _transformCorrection.Aiming();
        }

        //void HandleAim()
        //{
        //    Debug.Log($"_ar: {_ar}, _camera: {_camera}");
        //    if (_ar.aimType == AimType.None) return;
        //    IsAiming = !IsAiming;
        //}
    }
}
