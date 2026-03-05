using UnityEngine;

public class Torso : MonoBehaviour
{


    public Rigidbody2D torso;
    public Animator anim;
    public GameObject legs;

    public GameObject rightShoulder;
    public GameObject leftShoulder;

    public Camera cam;

    public bool duck = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        torso = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cam = Camera.main;
        
    }

    // Update is called once per frame
    void Update()
    {
        rotateAtMouse();
        Duck();
        stickToLegs();
    }



    public void stickToLegs()
    {
        torso.transform.position = legs.transform.position;
        torso.transform.rotation = torso.transform.rotation;
    }

    





    public void rotateAtMouse()
    {
        Vector3 mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        float angleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angleDeg = (180/Mathf.PI)*angleRad - 90;

        torso.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg+130);
    }


    public void Duck() {
        
            if (Input.GetKey(KeyCode.Space)) {
                    duck = true;
            }
            else
            {
                duck = false;
            }
            

        anim.SetBool("duck", duck);

        
    
    }
}
