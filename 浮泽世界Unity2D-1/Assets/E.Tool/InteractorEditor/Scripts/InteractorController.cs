using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using E.Utility;
using E.Tool;

namespace E.Game
{
    public class InteractorController : MonoBehaviour
    {
        [SerializeField] private InteractorData m_InteractorDataClass;
        [ReadOnly] public InteractorData InteractorDataInstance;

        [SerializeField] private SpriteRenderer m_HealthBar;


        private void Start()
        {
            InteractorDataInstance = ScriptableObject.CreateInstance<InteractorData>();
            InteractorDataInstance.SetData(m_InteractorDataClass);
        }
        private void Update()
        {
        }
        private void OnMouseEnter()
        {
            
        }
        private void OnMouseOver()
        {
            if (Input.GetMouseButtonUp(1) && InteractorDataInstance.IsSurveyable)
            {
                UIManager.Singleton.m_UIInteractorInfoController.ShowInteractorInfo(InteractorDataInstance);
            }
        }
        private void OnMouseUp()
        {
            if (InteractorDataInstance.IsPickable)
            {
                CharacterManager.Singleton.Player.GetComponent<PlayerController>().GetInteractor(gameObject);
                gameObject.SetActive(false);
            }
        }
    }
}