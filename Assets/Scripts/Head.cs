using UnityEngine;

public class Head : MonoBehaviour
{
 


    public Rigidbody2D head;
//    public Animator anim;
    public GameObject torso;

    public Camera cam;

 //   public bool duck = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        head = GetComponent<Rigidbody2D>();
      //  anim = GetComponent<Animator>();
        cam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        rotateAtMouse();
        
        stickToLegs();
    }



    public void stickToLegs()
    {
        head.transform.position = torso.transform.position;
        head.transform.rotation = head.transform.rotation;
    }







    public void rotateAtMouse()
    {
        Vector3 mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        float angleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angleDeg = (180 / Mathf.PI) * angleRad - 90;

        head.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
    }


    
}
