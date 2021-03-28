using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayActor : MonoBehaviour
{    
    struct RecordNode
    {        
        public Vector3 position;
        public HumanBodyBones bone;
        public Quaternion quaternion;

        public RecordNode(HumanBodyBones bone, Vector3 position, Quaternion q)
        {
            this.bone = bone;
            this.position = new Vector3(position.x, position.y, position.z);
            this.quaternion = new Quaternion(q.x, q.y, q.z, q.w);
        }
    }

    struct Record
    {
        Dictionary<int, List<RecordNode>> records;
        string path;
        int RecordSkipSeq;

        public Record(string fileName, int skipSeq)
        {
            records = new Dictionary<int, List<RecordNode>>();
            this.path = Application.persistentDataPath + "/" + fileName;
            RecordSkipSeq = skipSeq;            
        }

        public void Save()
        {
            string json = "{ \"records\": [";
            foreach(KeyValuePair<int, List<RecordNode>> kv in records)
            {
                string szRecord = "\n{ \"record\": { \"seq\": " + kv.Key.ToString() + ", \"data\": [\n";
                    
                for (int n = 0; n < kv.Value.Count; n++)
                {
                    string szNode = string.Format("\t< \"bone\": {0}, \"position\": < \"x\": {1}, \"y\": {2}, \"z\": {3} >, \"quaternion\": < \"x\": {4}, \"y\": {5}, \"z\": {6}, \"w\": {7} > >"
                                                    , (int)kv.Value[n].bone
                                                    , kv.Value[n].position.x
                                                    , kv.Value[n].position.y
                                                    , kv.Value[n].position.z
                                                    , kv.Value[n].quaternion.x
                                                    , kv.Value[n].quaternion.y
                                                    , kv.Value[n].quaternion.z
                                                    , kv.Value[n].quaternion.w
                                                    );
                    szNode = szNode.Replace("<", "{");
                    szNode = szNode.Replace(">", "}");

                    if (n < kv.Value.Count - 1)
                    {
                        szNode += ",\n";
                    }

                    szRecord += szNode;
                }

                szRecord += "]}},";
                json += szRecord;
            }
            json = json.Substring(0, json.Length - 1) + "]}";

            File.WriteAllText(path, json);
            Debug.Log("Writed file. " + path);
        }

        public int Recording(int seq, HumanBodyBones bone, Transform transfrom)
        {
            if(seq > RecordSkipSeq)
            {                
                if (!records.ContainsKey(seq))
                {
                    records[seq] = new List<RecordNode>();
                }
                RecordNode node = new RecordNode(bone, transfrom.position, transfrom.rotation);
                records[seq].Add(node);
            }
            return records.Keys.Count;
        }
    }

    Record mRecord;
    int recordSeq = 0;

    public string path;
    public int startFrame = 1800;

    private void Awake()
    {
        mRecord = new Record(path, startFrame);
    }

    private void Update()
    {
        //record
        int recoredCnt = 0;
        for (int n = 0; n < (int)HumanBodyBones.LastBone; n++)
        {
            if (anim.GetBoneTransform((HumanBodyBones)n) != null)
                recoredCnt = mRecord.Recording(recordSeq, (HumanBodyBones)n, anim.GetBoneTransform((HumanBodyBones)n).transform);
        }
        recordSeq++;
        if(recoredCnt > 0)
            Debug.Log(string.Format("Recorded cnt = {0}", recoredCnt));
    }

    private void OnDestroy()
    {
        mRecord.Save();
    }

}