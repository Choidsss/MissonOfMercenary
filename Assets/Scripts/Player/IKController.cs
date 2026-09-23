using MissionOfMercenary;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.Rendering.DebugUI;

namespace MIssionOfMercenary
{
    public class IKController : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;

        [Header("References")]
        [SerializeField] Transform _weaponBox;

        [Header("Sway")]
        [SerializeField] float _swayAmount;
        [SerializeField] float _swaySpeed;
        [SerializeField] float _swayClamp;

        Vector3 _swayOffset;
        Vector2 _swayInput;
        Vector3 _weaponBoxOrigin;
        private void OnEnable()
        {
            _inputReader.OnLookEvent += HandleSway;
        }

        private void OnDisable()
        {
            _inputReader.OnLookEvent -= HandleSway;
        }

        void Start()
        {
            _weaponBoxOrigin = _weaponBox.localPosition;
        }

        void Update()
        {
            UpdateSway();
        }

        void HandleSway(Vector2 value)
        {
            _swayInput = value;
        }

        void UpdateSway()
        {
            Vector3 swayInput = new Vector3(-_swayInput.y, _swayInput.x, 0);
            swayInput.x = Mathf.Clamp(swayInput.x, -_swayClamp, _swayClamp);
            swayInput.y = Mathf.Clamp(swayInput.y, -_swayClamp, _swayClamp);

            Quaternion rot = _weaponBox.rotation * Quaternion.Euler(swayInput.x, swayInput.y, 0);

            _weaponBox.localPosition = Vector3.Lerp(_weaponBox.localPosition, _weaponBoxOrigin + _swayOffset, _swayAmount * Time.deltaTime);
            _weaponBox.rotation = Quaternion.Lerp(_weaponBox.rotation, rot, _swayAmount * Time.deltaTime);

            _swayInput = Vector2.zero;
        }
    }
}
