using TMPro;
using UnityEngine;
using System.Collections;
using MissionOfMercenary;

namespace MIssionOfMercenary
{
    public class AttackTypeToggleUI : MonoBehaviour
    {
        [SerializeField] AssultRifle _assultRifle;

        [SerializeField] InputReader _inputReader;
        [SerializeField] GameObject _singleTMP;
        [SerializeField] GameObject _autoTMP;

        float _textDuration = 1.5f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //_assultRifle = GetComponent<AssultRifle>();

            _singleTMP.SetActive(false);
            _autoTMP.SetActive(false);
        }

        private void OnEnable()
        {
            _inputReader.OnAttackTypeToggleEvent += SwitchTypeEvent;
        }

        private void OnDisable()
        {
            _inputReader.OnAttackTypeToggleEvent -= SwitchTypeEvent;
        }

        void SwitchTypeEvent()
        {
            if (_assultRifle.AttackType == AssultRifle.SingleOrAuto.auto)
            {
                _assultRifle.AttackType = AssultRifle.SingleOrAuto.single;
                _singleTMP.SetActive(true);
                _autoTMP.SetActive(false);
                StartCoroutine(OnTextDissapearRoutine());
            }
            else
            {
                _assultRifle.AttackType = AssultRifle.SingleOrAuto.auto;
                _singleTMP.SetActive(false);
                _autoTMP.SetActive(true);
                StartCoroutine(OnTextDissapearRoutine());
            }
        }

        IEnumerator OnTextDissapearRoutine()
        {
            yield return new WaitForSeconds(_textDuration);
            _singleTMP.SetActive(false);
            _autoTMP.SetActive(false);
        }
    }
}
