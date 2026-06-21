using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerAnimations : MonoBehaviour
    {
        PlayerMove _playerMove;
        Animator _anim;

        public bool IsAiming {  get; private set; }
        

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _anim = GetComponent<Animator>();
            _playerMove = GetComponentInParent<PlayerMove>();
        }

        // Update is called once per frame
        void Update()
        {
            PlayMovementAnimation();
            //Debug.Log(_playerMove.DoRun);
        }

        void PlayWalkAnimation()
        {
            if (_playerMove == null || _anim == null) { Debug.Log("Script NULL");  return; }

            if (!_playerMove.DoRun)
            {
                _anim.SetFloat("walkSpeed", _playerMove.walkSpeed);
                _anim.SetBool("aiming", _playerMove.DoRun);
                _anim.SetBool("DoRun", _playerMove.DoRun);
            }
        }

        //void PlayRunAnimation()
        //{
        //    if (_playerMove == null || _anim == null) { Debug.Log("Script NULL"); return; }

        //    if (!_playerMove.DoRun)
        //    {
        //        _anim.SetFloat("walkSpeed", _playerMove.walkSpeed);
        //        _anim.SetBool("aiming", _playerMove.DoRun);
        //        _anim.SetBool("DoRun", _playerMove.DoRun);
        //    }

        //}

        void PlayMovementAnimation()
        {
            if (_playerMove == null || _anim == null) { Debug.Log("Script NULL"); return; }
            _anim.SetFloat("walkSpeed", _playerMove.walkSpeed);
            _anim.SetBool("aiming", IsAiming);      // aiming 로직 따로 만드실 거면 이렇게
            _anim.SetBool("DoRun", _playerMove.DoRun);
        }
    }
}
