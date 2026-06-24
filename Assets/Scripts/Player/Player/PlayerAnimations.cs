using MissionOfMercenary;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class PlayerAnimations : MonoBehaviour
    {
        [SerializeField] InputReader _inputReader;
        PlayerMove _playerMove;
        Animator _anim;
        AssultRifle _ar;

        //public bool Relolading { get; private set; } = false;
        

        private void OnEnable()
        {
            _inputReader.OnReloadEvent += ReloadAnimationHandle;
        }

        private void OnDisable()
        {
            _inputReader.OnReloadEvent -= ReloadAnimationHandle;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _anim = GetComponent<Animator>();
            _playerMove = GetComponentInParent<PlayerMove>();
            _ar = GetComponentInChildren<AssultRifle>();
        }

        // Update is called once per frame
        void Update()
        {
            PlayMovementAnimation();

            if (_ar == null) { Debug.Log("AssultRifle Not Exist"); return; }

        }

        void ReloadAnimationHandle(float value)
        {
            //Relolading = true;

            ReloadingAnimation();
            //Relolading = false;
        }


        //void PlayWalkAnimation()
        //{
        //    if (_playerMove == null || _anim == null) { Debug.Log("Script NULL");  return; }

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
            //_anim.SetBool("aiming", IsAiming);      // aiming 로직 따로 만드실 거면 이렇게
            _anim.SetBool("DoRun", _playerMove.DoRun);
        }

        void ReloadingAnimation()
        {
            if (_playerMove == null || _anim == null) { Debug.Log("Script NULL"); return; }

            _anim.SetTrigger("Reload");
        }
    }
}
