using UnityEngine;

public class Movement : MonoBehaviour
{


    public Rigidbody2D body;
    public Animator anim;
    public bool isMoving = false;
    public float speed = 2f;

    public float x;
    public float y;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        x = 0.0f;
        y = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        x = 0.0f;
        y = 0.0f;

        if (Input.GetKey(KeyCode.UpArrow)) {
            y += 1;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 1;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            x += 1;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            y -= 1;
            isMoving = true;
        }

       

        Vector2 direction = new Vector2(x, y).normalized;;

        anim.SetBool("walking", isMoving);
        
        body.linearVelocity = direction * speed;

        if (isMoving)
        {
            isMoving = false;
        }





    }
}
