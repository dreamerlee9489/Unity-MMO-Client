using Control;
using Google.Protobuf;
using Net;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Frame
{
    public class EnemyInfo
    {
        public int id = 0;
        public int hp = 0;
        public Vector3 pos = Vector3.zero;

        public void Parse(Proto.Enemy proto)
        {
            id = proto.Id;
            hp = proto.Hp;
            pos.x = proto.Pos.X;
            pos.y = proto.Pos.Y;
            pos.z = proto.Pos.Z;
        }
    }

    public class WorldManager : MonoBehaviour
    {
        List<EnemyController> enemies = new List<EnemyController>();

        public string csvFile = "";

        private void Awake()
        {
            csvFile = Application.streamingAssetsPath + "/CSV/" + csvFile;
            Debug.Log(csvFile);
            using (StreamReader reader = File.OpenText(csvFile))
            {
                reader.ReadLine();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] strs = line.Split(',');
                    EnemyController enemyObj = Instantiate(ResourceManager.Instance.Load<GameObject>("Entity/Enemy/" + strs[1])).GetComponent<EnemyController>();
                    enemyObj.gameObject.SetActive(false);
                    enemyObj.Entity.hp = int.Parse(strs[2]);
                    enemyObj.transform.position = GetPos(strs[3]);
                    enemies.Add(enemyObj.GetComponent<EnemyController>());
                    enemyObj.gameObject.SetActive(true);
                }
            }
            MsgManager.Instance.RegistMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
        }

        private void OnApplicationQuit()
        {
            MsgManager.Instance.RemoveMsgHandler(Proto.MsgId.S2CEnemyList, EnemyListHandler);
        }

        private Vector3 GetPos(string posStr)
        {
            Vector3 pos = Vector3.zero;
            posStr = posStr.Substring(1, posStr.Length - 2);
            string[] strs = posStr.Split(';');
            pos.x = float.Parse(strs[0]);
            pos.y = float.Parse(strs[1]);
            pos.z = float.Parse(strs[2]);
            return pos;
        }

        private void EnemyListHandler(IMessage msg)
        {
            Proto.EnemyList proto = msg as Proto.EnemyList;
            if (proto != null)
            {
                var enemyList = proto.Enemy;
                for (int i = 0; i < enemyList.Count; ++i)
                {
                    enemies[i].Parse(enemyList[i]);
                }
            }
        }
    }
}
