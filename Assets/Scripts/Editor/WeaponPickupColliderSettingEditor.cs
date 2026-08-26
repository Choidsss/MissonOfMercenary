using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MIssionOfMercenary
{
    [CustomEditor(typeof(WeaponPickupColliderSetting))]
    public class WeaponPickupColliderSettingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI(); 두번 그려지는 이유
            DrawDefaultInspector();

            EditorGUILayout.Space();

            if(GUILayout.Button("Generate Pick Collider"))
            {
                GeneratePickupCollider((WeaponPickupColliderSetting)target);
            }
        }

        static void GeneratePickupCollider(WeaponPickupColliderSetting setting)
        {
            Transform weaponRoot = setting.transform;

            Renderer[] renderers = weaponRoot.GetComponentsInChildren<Renderer>(true);

            if (!TryCalculateLocalBounds(weaponRoot, renderers, out Bounds bounds))
            {
                Debug.LogWarning("총기 하위에서 크기를 계산할 Renderer를 찾지 못하였습니다.", setting);
                return;
            }

            Transform triggerTransform = weaponRoot.Find(setting.TriggerObjectName);

            //************************************복붙 하였음, 다시 봐야함**********************************************
            if (triggerTransform == null)
            {
                GameObject triggerObject =
                    new GameObject(setting.TriggerObjectName);

                Undo.RegisterCreatedObjectUndo(
                    triggerObject,
                    "Create Pickup Trigger"
                );

                triggerTransform = triggerObject.transform;
                triggerTransform.SetParent(weaponRoot, false);
            }

            Undo.RecordObject(
                triggerTransform,
                "Reset Pickup Trigger Transform"
            );

            triggerTransform.localPosition = Vector3.zero;
            triggerTransform.localRotation = Quaternion.identity;
            triggerTransform.localScale = Vector3.one;

            BoxCollider boxCollider =
                triggerTransform.GetComponent<BoxCollider>();

            if (boxCollider == null)
            {
                boxCollider = Undo.AddComponent<BoxCollider>(
                    triggerTransform.gameObject
                );
            }
            else
            {
                Undo.RecordObject(
                    boxCollider,
                    "Update Pickup Collider"
                );
            }

            float padding = Mathf.Max(0f, setting.Padding);

            boxCollider.center = bounds.center;
            boxCollider.size =
                bounds.size + Vector3.one * padding * 2f;
            boxCollider.isTrigger = true;
            boxCollider.enabled = true;

            EditorUtility.SetDirty(triggerTransform);
            EditorUtility.SetDirty(boxCollider);
            EditorUtility.SetDirty(setting);

            Debug.Log(
                $"Pickup Collider 생성 완료: {weaponRoot.name}",
                setting
            );
        }

        static bool TryCalculateLocalBounds(Transform weapopnRoot, Renderer[] renderers, out Bounds result)
        {
            result = default;
            bool initialize = false;

            foreach (Renderer renderer in renderers)
            {
                if(renderer is ParticleSystemRenderer || renderer is LineRenderer || renderer is TrailRenderer) { continue; }

                Bounds rendererBounds = renderer.localBounds;
                Vector3 min = rendererBounds.min;
                Vector3 max = rendererBounds.max;

                //???????????????????????반복문이 4중첩이요????이거 이렇게 쓰는거 맞나....????? 작아서 괜찮나....??그리고 상수코딩??????차라리 위에서 변수를 Const로 만들어서 쓰는게?
                for (int x = 0;x < 2;x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            Vector3 rendererLocalCorner = new Vector3(x == 0 ? min.x : max.x, y == 0 ? min.y : max.y, z == 0 ? min.z : max.z);
                            Vector3 worldCorner = renderer.transform.TransformPoint(rendererLocalCorner);
                            Vector3 weponLocalCorner = weapopnRoot.InverseTransformPoint(worldCorner);

                            if (!initialize)
                            {
                                result = new Bounds(weponLocalCorner, Vector3.zero);
                                initialize = true;
                            }
                            else
                            {
                                result.Encapsulate(weponLocalCorner);
                            }
                        }
                    }
                }
            }

            return initialize;
        }
    }
}
