using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayActor : MonoBehaviour
{
    
    public string jsonName;
    public int startFrame = 70;
    public float fps = 1.0f / 24.0f;
    public bool enablePositionX;
    public bool enablePositionY;
    public bool enablePositionZ;
    public float direction = 0;


    ReplayPose pose = new ReplayPose();
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        Time.fixedDeltaTime = fps;

        anim = GetComponent<Animator>();
        pose.Load(jsonName, startFrame, transform.position, enablePositionX, enablePositionY, enablePositionZ, direction, transform.localScale.x);
    }

    private void FixedUpdate()
    {
        pose.Next(ref anim);
    }
}
