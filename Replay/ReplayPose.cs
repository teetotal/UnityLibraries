using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

public class ReplayPose
{
    [Serializable]
    public struct RecordNode
    {
        public int bone;
        public Vector3 position;
        public Quaternion quaternion;
    }

    [Serializable]
    public struct RecordSeq
    {
        public int seq;
        public List<RecordNode> data;
    }
   
    [Serializable]
    public struct Record
    {
        public RecordSeq record;
    }
    [Serializable]
    public struct Records
    {
        public List<Record> records;
    }
    int seq = 0;
    Records jsonObject;
    List<List<RecordNode>> animations = new List<List<RecordNode>>();
    Vector3 defaultPos;
    Vector3 firstPos;
    bool enableX, enableY, enableZ;
    float direction;
    float scale;
    public void Load(string path, int skipSeq, Vector3 position, bool x, bool y, bool z, float direction, float scale)
    {
                
        jsonObject = LoadJsonFileFromResources<Records>(path);

        defaultPos = new Vector3(position.x, position.y, position.z);
        enableX = x;
        enableY = y;
        enableZ = z;
        this.direction = direction;
        this.scale = scale;

        for (int n = skipSeq; n < jsonObject.records.Count; n++)
        {
            List<RecordNode> list = new List<RecordNode>();
            for(int i =0; i < jsonObject.records[n].record.data.Count; i++)
            {
                list.Add(jsonObject.records[n].record.data[i]);
            }
            animations.Add(list);
        }
        Debug.Log(string.Format("Loaded Recored Animations {0}", jsonObject.records.Count));
    }

    public int Next(ref Animator animator)
    {
        if (seq >= animations.Count)
            seq = 0;
               
        for (int n = 0; n < animations[seq].Count; n++)
        {
            RecordNode node = animations[seq][n];
            Transform transform = animator.GetBoneTransform((HumanBodyBones)node.bone);
            if (transform != null)
            {
                if ((HumanBodyBones)node.bone == HumanBodyBones.Hips)
                {
                    if(firstPos == null)
                    {
                        firstPos = new Vector3(node.position.x, node.position.y, node.position.z);
                    }

                    Vector3 posDiff = node.position - firstPos;
                    posDiff *= scale;
                    Vector3 position = new Vector3(
                        enableX ? defaultPos.x + posDiff.x : defaultPos.x,
                        enableY ? defaultPos.y + posDiff.y : defaultPos.y,
                        enableZ ? defaultPos.z + posDiff.z : defaultPos.z
                        );                   

                    transform.position = position;
                    //transform.position = node.position;
                }                    

                transform.rotation = Quaternion.AngleAxis(direction, Vector3.up) * new Quaternion(node.quaternion.x, node.quaternion.y, node.quaternion.z, node.quaternion.w);

            }

        }
               
        return seq++;        
    }

    private T ToObject<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    private T LoadJsonFileFromResources<T>(string path)
    {
        TextAsset buffer = Resources.Load<TextAsset>(path);
        string json = Encoding.UTF8.GetString(buffer.bytes);
        return ToObject<T>(json);
    }
}
