using System.Collections;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class MuzzleFlashEffect : MonoBehaviour
    {
        [Header("Scriptable_Object")]
        [SerializeField] FirearmDefinition _firearmDef;
        [SerializeField] WeaponEffectDefinition _weaponEffectDef;
        [SerializeField] Transform _muzzle;
        [SerializeField, Min(0)] float _duration = 0.1f;

        GameObject _instance;
        Coroutine _hideCoroutine;

        private void OnDisable()
        {
            HideMuzzleFlash();
        }

        void Awake()
        {
            InitializeMuzzleFlash();
        }

        private void OnDestroy()
        {
            if (_instance != null)
            {
                Destroy(_instance);
            }
        }

        void InitializeMuzzleFlash()
        {
            if(_firearmDef == null || _weaponEffectDef == null || _weaponEffectDef.MuzzleFlash == null) { Debug.LogWarning("머즐 플래시 설정을 확인해주세요.", this); return; }

            _instance = Instantiate(_weaponEffectDef.MuzzleFlash, _muzzle);
            _instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _instance.SetActive(false);
        }

        void HideMuzzleFlash()
        {
            if(_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            if(_instance != null) { _instance.SetActive(false); }
        }

        public void PlayMuzzleFlash()
        {
            if (_instance == null) { return; }

            HideMuzzleFlash();
            _instance.SetActive(true);
            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(_duration);

            if (_instance != null) 
            {
                _instance.SetActive(false);
            }

            _hideCoroutine = null;
        }

        
    }
}
