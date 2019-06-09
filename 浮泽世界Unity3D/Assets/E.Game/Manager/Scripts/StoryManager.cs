// ========================================================
// 作者：E Star
// 创建时间：2019-01-27 01:41:08
// 当前版本：1.0
// 作用描述：
// 挂载目标：
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Unity.Collections;
using E.Utility;
using E.Tool;

namespace E.Game
{
    public class StoryManager : Manager<StoryManager>
    {
        [Header("当前故事状态")]
        //[Tooltip("当前周目")] public int m_CurrentStoryRound;
        //[Tooltip("当前章节")] public int m_CurrentStoryChapter;
        //[Tooltip("当前场景")] public int m_CurrentStoryScene;
        //[Tooltip("当前对话")] public int m_CurrentStoryDialog;
        //[Tooltip("当前分支")] public int m_CurrentStoryBranch;
        [Tooltip("是否正在对话")] public bool m_IsDialog = false;
        [Tooltip("当前对话")] public StoryNode m_CurrentStoryNode;
        [Tooltip("当前句子")] public int m_CurrentSentence = 0;

        private void Update()
        {
        }

        /// <summary>
        /// 开始调查或对话
        /// </summary>
        public void SurveyOrDialog()
        {
            if (CameraManager.Singleton.m_CurrentCamera == null)
            {
                Debug.Log("无法开始对话，相机缺失");
                return;
            }
            GameObject target = CameraManager.Singleton.m_CurrentCamera.GetComponent<RaycastController>().m_HitTarget;
            if (target == null)
            {
                Debug.Log("无法开始对话，目标不存在");
                return;
            }

            //当射线碰撞目标为角色，执行开始对话
            if (target.layer == 9)
            { StartDialog(target); }
            //当射线碰撞目标为prop
            else if (target.layer == 10)
            { StartSurvey(target); }
            else
            { Debug.Log("无法调查或对话" + name); }
        }
        /// <summary>
        /// 开始调查
        /// </summary>
        /// <param name="prop"></param>
        public void StartSurvey(GameObject prop)
        {
            string name = prop.GetComponent<InteractorData>().Name;
            string des = prop.GetComponent<InteractorData>().Describe;

            Debug.Log("开始调查" + name);
            Debug.Log(des);
        }
        /// <summary>
        /// 开始对话
        /// </summary>
        public void StartDialog(GameObject character)
        {
            m_IsDialog = true;
            UIManager.Singleton.IsShowDialog = true;
            m_CurrentStoryNode = character.GetComponent<NPCData>().m_StoryNode;
            if (m_CurrentStoryNode != null)
            {
                ChooseSentence(0);
                Debug.Log("开始对话" + character.name);
            }
            else
            {
                Debug.LogError("无法开始对话，对象没有对话内容" + character.name);
            }
        }
        /// <summary>
        /// 下一句对话
        /// </summary>
        public void NextSentence()
        {
            if (m_IsDialog)
            {
                if (m_CurrentStoryNode == null)
                {
                    Debug.Log("无法开始对话，目标不存在");
                    return;
                }

                m_CurrentSentence++;
                int sentenceCount = m_CurrentStoryNode.Content.Sentences.Length;
                if (m_CurrentSentence < sentenceCount)
                {
                    ChooseSentence(m_CurrentSentence);
                }
                else
                {
                    m_CurrentSentence = 0;
                    FinishDialog();
                }

            }
        }
        /// <summary>
        /// 选择对话
        /// </summary>
        /// <param name="i">选择的段落</param>
        /// <param name="i">选择的句子</param>
        public void ChooseSentence(int i)
        {
            string speaker = m_CurrentStoryNode.Content.Sentences[i].Speaker;
            string content = m_CurrentStoryNode.Content.Sentences[i].Words;
            UIManager.Singleton.m_UIDialogController.SetSpeaker(speaker);
            UIManager.Singleton.m_UIDialogController.SetContent(content);
            m_CurrentStoryNode.Content.Sentences[i].IsReaded = true;
        }
        /// <summary>
        /// 结束对话
        /// </summary>
        public void FinishDialog()
        {
            m_IsDialog = false;
            UIManager.Singleton.IsShowDialog = false;
            m_CurrentStoryNode.IsPassed = true;
            m_CurrentStoryNode = null;
            m_CurrentSentence = 0;
            Debug.Log("结束对话");
        }

        public void StartTimes()
        {

        }
        public void FinishTimes()
        {

        }
        public void StartChapter()
        {

        }
        public void FinishChapter()
        {

        }
        public void StartScene()
        {

        }
        public void FinishScene()
        {

        }
    }
}