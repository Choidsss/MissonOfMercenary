# ToDoList : After_Chase

## 현재 완료된 기능

- [x] 정해진 Waypoint를 따라 순찰
- [x] 시야각과 장애물을 이용한 플레이어 감지
- [x] 플레이어 발견 시 추격
- [x] 총소리 감지 및 발생 위치 조사
- [x] 행동 우선순위: 플레이어 추격 → 총소리 조사 → 순찰
- [x] Enemy Ragdoll 구성 및 평상시 Rigidbody Kinematic 설정

---

## 1순위: 현재 BT 동작 안정화

- [ ] 총소리가 없을 때 바로 Patrol이 실행되는지 확인
- [ ] 순찰 중 총소리를 들으면 Sound Chase로 전환되는지 확인
- [ ] 총소리 조사 중 플레이어를 보면 Player Chase가 우선 실행되는지 확인
- [ ] 소리 위치에 도착하면 `ClearSoundTarget()` 후 Patrol로 돌아가는지 확인
- [ ] 조사 중 새로운 총소리가 발생하면 최신 위치로 목적지가 갱신되는지 확인
- [ ] `NavMeshAgent`가 NavMesh 밖에 있거나 비활성화됐을 때 예외가 발생하지 않는지 확인

완료 기준: 순찰, 총소리 조사, 플레이어 추격이 서로 목적지를 빼앗지 않고 우선순위대로 전환된다.

---

## 2순위: Enemy 체력과 피격 구현

대상 스크립트: `EnemyHit.cs`, `AssultRifle.cs`

- [ ] `EnemyHit`에 최대 체력과 현재 체력 추가
- [ ] `TakeDameged()`에서 전달받은 Damage만큼 체력 감소
- [ ] 체력이 0 이하일 때 `Death()`를 한 번만 호출
- [ ] 이미 죽은 Enemy는 추가 피격 처리를 하지 않도록 방지
- [ ] 총알 Raycast가 자식 Ragdoll Collider에 맞아도 루트 `EnemyHit`을 찾도록 `GetComponentInParent<EnemyHit>()` 사용
- [ ] 필요하면 피격 이펙트와 피격 사운드 추가

완료 기준: 어느 신체 Collider를 맞혀도 동일한 Enemy의 체력이 감소하고, 체력이 0이 되면 사망 처리가 정확히 한 번 실행된다.

---

## 3순위: 신체 부위별 대미지

대상 스크립트: `EnemyBodyPart.cs`, `AssultRifle.cs`, `EnemyHit.cs`

- [ ] 맞은 Collider가 Head, Body, Arms, Legs 중 어디인지 판정
- [ ] 부위별 대미지 배율 설정
- [ ] 권장 초기값: Head 2.0, Body 1.0, Arms 0.75, Legs 0.75
- [ ] Dictionary에 등록되지 않은 Collider가 들어와도 예외가 발생하지 않도록 `TryGetValue()` 사용
- [ ] 문자열 대신 `BodyPart` enum을 반환하도록 정리

완료 기준: 같은 총으로 사격해도 맞은 부위에 따라 체력 감소량이 다르고, 등록되지 않은 Collider에서도 게임이 멈추지 않는다.

---

## 4순위: 사망 시 Ragdoll 전환

대상 스크립트: `EnemyHit.cs` 또는 별도의 `EnemyRagdoll.cs`

- [ ] 시작 시 모든 Ragdoll Rigidbody를 수집
- [ ] 평상시에는 `isKinematic = true` 유지
- [ ] 사망 시 `Animator` 비활성화
- [ ] 사망 시 `NavMeshAgent` 비활성화
- [ ] 사망 시 `EnemyBT`, 감지, 공격 컴포넌트 비활성화
- [ ] 모든 Ragdoll Rigidbody의 `isKinematic = false` 전환
- [ ] 마지막 총알의 방향과 충돌 위치를 이용해 맞은 신체에 힘 적용
- [ ] 사망 처리가 여러 번 호출되어 Ragdoll 상태가 반복 변경되지 않도록 방지

완료 기준: 살아 있을 때는 애니메이션과 NavMesh로 움직이고, 사망 순간 제어권이 Ragdoll 물리로 한 번만 전환된다.

---

## 5순위: Enemy 공격 행동 추가

대상 스크립트: `EnemyAttack.cs`, 신규 BT 조건/행동 노드

- [ ] 공격 가능 거리 조건 노드 구현
- [ ] 플레이어와 Enemy 사이 장애물 검사
- [ ] 공격 중 플레이어 방향으로 회전
- [ ] 연사 간격 또는 공격 쿨타임 구현
- [ ] Enemy 총구에서 플레이어 방향으로 Raycast 발사
- [ ] 플레이어 피격 처리와 연결
- [ ] 사망하거나 플레이어를 놓치면 공격 중단

권장 BT 우선순위:

```text
Selector
├─ 공격 가능 → 플레이어 공격
├─ 플레이어 보임 → 플레이어 추격
├─ 총소리 목표 있음 → 총소리 위치 조사
└─ 순찰
```

완료 기준: 사거리 밖에서는 추격하고, 사거리 안이면서 시야가 확보되면 멈춰서 공격한다.

---

## 6순위: Player 체력과 게임 루프

- [ ] Player Health 및 `IDamageable` 연결
- [ ] Enemy 공격을 받으면 체력 감소
- [ ] 피격 화면 효과 또는 UI 추가
- [ ] Player 사망 처리
- [ ] 재시작 또는 체크포인트 처리
- [ ] Enemy 전멸 또는 목표 지점 도달 시 클리어 처리

완료 기준: 플레이어와 Enemy가 서로 피해를 주고받으며 사망과 재시작까지 하나의 플레이 흐름이 완성된다.

---

## 7순위: 추격 이후 수색 행동

핵심 전투 루프가 완성된 뒤 추가한다.

- [ ] 플레이어를 놓친 마지막 위치 기억
- [ ] 마지막 목격 위치까지 이동
- [ ] 도착 후 일정 시간 주변을 회전하며 수색
- [ ] 수색 중 플레이어 발견 시 즉시 Chase 또는 Attack으로 전환
- [ ] 수색 실패 시 Patrol 복귀
- [ ] 필요하면 Alert 상태와 평상시/경계 시야 범위 분리

완료 기준: 플레이어가 시야에서 사라지는 즉시 순찰로 돌아가지 않고, 마지막 위치를 잠시 조사한다.

---

## 8순위: 애니메이션과 연출 연결

- [ ] Patrol, Chase 속도에 맞게 Animator Speed 값 연결
- [ ] 조준, 발사, 피격 애니메이션 연결
- [ ] 이동 중 발 미끄러짐과 회전 속도 조정
- [ ] 피격 시 짧은 반응 또는 상체 Additive 애니메이션 적용
- [ ] 총소리 조사 시 경계 자세와 애니메이션 적용
- [ ] Ragdoll 전환 순간 애니메이션과 물리 자세가 크게 튀지 않는지 확인

---

## 9순위: 최적화와 구조 정리

- [ ] 매 프레임 실행되는 불필요한 `Debug.Log()` 제거
- [ ] 사용하지 않는 필드, `using`, 빈 `Start()`와 `Update()` 제거
- [ ] `EnemyChase`의 중복 상태인 `DoFind`와 `EnemyHeardSound()` 정리
- [ ] Player/Enemy LayerMask가 정확하게 설정됐는지 확인
- [ ] 다수 Enemy 환경에서 `OverlapSphere`와 시야 검사 성능 확인
- [ ] 죽은 Enemy가 총소리 감지 대상에 포함되지 않도록 처리
- [ ] 클래스와 함수 이름 오탈자 정리: `Dameged`, `Assult`, `sond` 등

---

## 추천 구현 순서 요약

```text
BT 전환 테스트
→ Enemy 체력/피격
→ 부위별 대미지
→ 사망/Ragdoll
→ Enemy 공격
→ Player 체력/사망
→ 추격 후 수색
→ 애니메이션/연출
→ 최적화와 포트폴리오 정리
```

## 포트폴리오에서 강조할 내용

- 시각과 청각 정보를 분리한 Enemy 인지 시스템
- Reactive Selector를 이용한 행동 우선순위 전환
- 조건 노드와 행동 노드의 책임 분리
- Animator/NavMesh 제어에서 Ragdoll 물리 제어로 전환되는 사망 시스템
- 신체 부위별 피격 판정과 대미지 처리
- Patrol → Investigate → Chase → Attack으로 확장되는 적 AI 상태 흐름
