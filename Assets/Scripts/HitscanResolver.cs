using UnityEngine;

namespace MIssionOfMercenary
{
    public readonly struct ShotResult
    {
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Distance { get; }
        public bool IsHit { get; }
        public RaycastHit Hit { get; }

        public ShotResult(Vector3 origin, Vector3 direction, float distance, bool isHit, RaycastHit hit)
        {
            Origin = origin;
            Direction = direction;
            Distance = distance;
            IsHit = isHit;
            Hit = hit;
        }
    }

    public sealed class HitscanResolver
    {
        public ShotResult Resolver(Ray aimRay, Vector3 muzzlePosition, float range)
        {
            Vector3 targetPoint;

            /*
             * 처음 레이캐스트는 targetPoint 계산용 캐스트.
             */

            if (Physics.Raycast(aimRay, out RaycastHit aimHit, range))//ray를 쏴서 걸리는게 있다면, 그 hit된 포인트가 타겟
            {
                targetPoint = aimHit.point;
            }
            else
            {
                targetPoint = aimRay.GetPoint(range);//아니라면 aimRay가 그대로 range만큼 간 후 도착한 point가 타겟
            }

            /*
             * 계산된 targetPoint를 바탕으로 방향을 계산
             */

            Vector3 direction = (targetPoint - muzzlePosition).normalized;//타겟과머즐의 벡터방향 계산

            //targetPoint가 없다면, 디폴트반환
            if (direction.sqrMagnitude < 0.00001f || range <= 0f)
            {
                return new ShotResult(muzzlePosition, Vector3.zero, 0, false, default);
            }

            //있으면 본격적인 명중된 정보 연산
            bool isHit = Physics.Raycast(muzzlePosition, direction, out RaycastHit hit, range);

            float distance = isHit ? hit.distance : range;
            
            return new ShotResult(muzzlePosition, direction, distance, isHit, hit);
        }
    }
}
