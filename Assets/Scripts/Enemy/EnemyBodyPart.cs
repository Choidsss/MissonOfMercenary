using System.Collections.Generic;
using UnityEngine;

namespace MIssionOfMercenary
{
    public enum BodyPart
    {
        Head,
        Body,
        Arms,
        Legs
    }

    public class EnemyBodyPart : MonoBehaviour
    {
        [SerializeField] Collider _leftArm1;
        [SerializeField] Collider _leftArm2;
        [SerializeField] Collider _righttArm1;
        [SerializeField] Collider _rightArm2;
        [SerializeField] Collider _lefLeg1;
        [SerializeField] Collider _leftLeg2;
        [SerializeField] Collider _rightLeg1;
        [SerializeField] Collider _rightLeg2;
        [SerializeField] Collider _pelvis;
        [SerializeField] Collider _chest;
        [SerializeField] Collider _head;

        Dictionary<Collider, BodyPart> _dicPartName = new Dictionary<Collider, BodyPart>();

        private void Awake()
        {
            SetBodyPartName();
        }

        void SetBodyPartName()
        {
            _dicPartName.Add(_leftArm1, BodyPart.Arms);
            _dicPartName.Add(_leftArm2, BodyPart.Arms);
            _dicPartName.Add(_righttArm1, BodyPart.Arms);
            _dicPartName.Add(_rightArm2, BodyPart.Arms);
            _dicPartName.Add(_lefLeg1, BodyPart.Legs);
            _dicPartName.Add(_leftLeg2, BodyPart.Legs);
            _dicPartName.Add(_rightLeg1, BodyPart.Legs);
            _dicPartName.Add(_rightLeg2, BodyPart.Legs);
            _dicPartName.Add(_pelvis, BodyPart.Body);
            _dicPartName.Add(_chest, BodyPart.Body);
            _dicPartName.Add(_head, BodyPart.Head);
        }

        public string GiveHitPart(Collider col)
        {
            if (_dicPartName[col] == BodyPart.Arms)
            {
                return "Arms";
            }
            else if (_dicPartName[col] == BodyPart.Legs)
            {
                return "Legs";
            }
            else if (_dicPartName[col] == BodyPart.Body)
            {
                return "Body";
            }
            else
            {
                return "Head";
            }
        }
    }
}
