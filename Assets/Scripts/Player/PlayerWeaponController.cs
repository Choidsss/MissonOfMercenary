using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;
        [SerializeField] WeaponManager _weaponManager;

        private void OnEnable()
        {
            _inputReader.OnshotEvent += HandleFire;
            _inputReader.OnShotCancled += HandleRelease;
            _inputReader.OnReloadEvent += HandleReload;
        }

        private void OnDisable()
        {
            _inputReader.OnshotEvent -= HandleFire;
            _inputReader.OnShotCancled -= HandleRelease;
            _inputReader.OnReloadEvent -= HandleReload;
        }

        void HandleFire(float value)
        {
            if (_weaponManager == null || !_weaponManager.isActiveAndEnabled) return;
            _weaponManager.Weapon?.TriggeredPressed();
        }

        //마우스 떼면 연사 중지
        void HandleRelease()
        {
            if (_weaponManager == null || !_weaponManager.isActiveAndEnabled) return;

            _weaponManager.Weapon?.TriggeredReleased();
        }

        void HandleReload(float value)
        {
            if (_weaponManager == null || !_weaponManager.isActiveAndEnabled) return;

            _weaponManager.Weapon?.TryReload();
        }
    }
}
