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
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using E.Utility;

namespace E.Game
{
    public class SaveManager : Manager<SaveManager>
    {
        [SerializeField, ReadOnly] private string m_SaveFolder;
        [SerializeField] private string m_SaveName = "1.save";
        private string m_SavePath;


        private void Start()
        {
            //m_SavePath = Application.persistentDataPath + "/gamesave.save";
            m_SaveFolder = Application.dataPath + "/Game/SaveSystem/Resources/";
            m_SavePath = m_SaveFolder + m_SaveName;
        }
        private void Update()
        {
            if (CharacterManager.Singleton.Player != null)
            {
            }
        }

        public void SaveGame()
        {
            if (!Directory.Exists(m_SaveFolder))
            {
                Directory.CreateDirectory(m_SaveFolder);
            }

            Save save = CreateSaveStruct();
            string json = JsonUtility.ToJson(save);
            File.WriteAllText(m_SavePath, json);

            AssetDatabase.Refresh();
            Debug.Log("存档成功：" + m_SavePath + " 时间：" + save.Time);
            Debug.Log("存档内容：" + json);
        }
        public void LoadGame()
        {
            if (File.Exists(m_SavePath))
            {
                string json = File.ReadAllText(m_SavePath);
                Save save = JsonUtility.FromJson<Save>(json);

                CharacterManager.Singleton.Player.GetComponent<CharacterData>().PlayerName = save.PlayerName;
                CharacterManager.Singleton.Player.transform.position = save.Position;
                CharacterManager.Singleton.Player.transform.eulerAngles = save.Rotation;

                Debug.Log("读档成功：" + m_SavePath);
                Debug.Log("读档内容" + json);
            }
            else
            {
                Debug.Log("No game saved!");
            }
        }
        private Save CreateSaveStruct()
        {
            Save save = new Save
            {
                ID = 1,
                Time = DateTime.Now,
                PlayerName = CharacterManager.Singleton.Player.GetComponent<CharacterData>().PlayerName,
                Position = CharacterManager.Singleton.Player.transform.position,
                Rotation = CharacterManager.Singleton.Player.transform.eulerAngles
            };
            return save;
        }
    }

    [Serializable]
    public struct Save
    {
        public int ID;
        public DateTime Time;
        public string PlayerName;
        public Vector3 Position;
        public Vector3 Rotation;

        //public Save(int id, DateTime time, string playerName)
        //{
        //    ID = id;
        //    Time = time;
        //    PlayerName = playerName;
        //}
    }
}