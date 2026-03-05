using UnityEngine;
using System.Collections;

public class RightHand : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Rigidbody2D hand;
    Animator anim;

    public GameObject torso;
    public GameObject rightShoulder;
    public Camera cam;
    

    bool punching = false;



    void Start()
    {
        hand = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        cam = Camera.main;
    }


    


    // Update is called once per frame
    void Update()
    {

        Punch();
        StickToShoulder();
        rotateAtMouse();

    }

    public void Punch() {
        if (punching == false && Input.GetKeyDown(KeyCode.Mouse1)) {

            StartCoroutine(punchCooldown());
        }
        anim.SetBool("punching", punching);

    }

    public void rotateAtMouse()
    {
        Vector3 mousePos = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
        float angleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angleDeg = (180 / Mathf.PI) * angleRad - 90;

        hand.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
    }



    public IEnumerator punchCooldown()
    {
        punching = true;
        yield return new WaitForSeconds(1f);
        punching = false;
    }

    
    public void StickToShoulder()
    {
        hand.transform.position = rightShoulder.transform.position;
       // hand.transform.rotation = torso.transform.rotation;
       
    
    }

    








}
