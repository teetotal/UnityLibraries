/*

    touch 이벤트 처리

*/
using System;
using UnityEngine;
using UnityEngine.UI;
public class Touch
{
     private static readonly Lazy<Touch> hInstance =
        new Lazy<Touch>(() => new Touch());
 
    public static Touch Instance
    {
        get {
            return hInstance.Value;
        }
        
    }
    protected Touch()
    {
    }
    /*
    Sprite에 AddComponent<BoxCollider2D>() 같은걸 추가해야 작동함
    */
    public GameObject GetTouchedObject(Vector3 position)
    {
        Vector3 pos = Camera.main.ScreenToWorldPoint(position);
        Vector2 pos2d = new Vector2(pos.x, pos.y);
        
        RaycastHit2D hit = Physics2D.Raycast(pos2d, Vector2.zero);
        if (hit.collider != null) 
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    public Text AddText(Text uiObj, Vector3 worldPosition, Camera camera, Transform tfCanvas)
    {
        Text obj = UnityEngine.Object.Instantiate(uiObj);
        obj.transform.position = camera.WorldToScreenPoint(worldPosition);
        obj.transform.SetParent(tfCanvas);

        return obj;
    }
}