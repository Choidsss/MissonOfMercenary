using MissionOfMercenary;
using TMPro;
using UnityEngine;

namespace MIssionOfMercenary
{
    public class ShowButtonUI : MonoBehaviour
    {
        [Header("Needed Asset")]
        [SerializeField] PlayerAssasinEnemy _assasinToEnemy;

        [Header("Show Text")]
        [SerializeField] TextMeshProUGUI _tmp;
        [SerializeField] GameObject _assasinUIPanel;

        private void Start()
        {
            _assasinUIPanel.SetActive(false);
        }

        private void Update()
        {
            ShowAssasinText();
        }

        void ShowAssasinText()
        {
            if (!_assasinToEnemy.WillAssasin) 
            {
                _assasinUIPanel.SetActive(false);
            }
            else
            {
                _tmp.text = "Press 'F', You Can Assasin Enemy";
                _assasinUIPanel.SetActive(true);
            }
        }
    }
}
