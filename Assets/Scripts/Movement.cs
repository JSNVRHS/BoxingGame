using UnityEngine;

public class Movement : MonoBehaviour
{


    public Rigidbody2D body;
    public Animator anim;
    public bool isMoving = false;
    public float speed = 2f;
    float speedMultiplier = 1f;
    bool inputInverted = false;
    Torso ownerTorso;

    public float x;
    public float y;
    public float z;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ownerTorso = GetComponentInParent<Torso>();

        x = 0.0f;
        y = 0.0f;
        z = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (ownerTorso != null && !ownerTorso.AllowPlayerInput)
        {
            return;
        }

        x = 0.0f;
        y = 0.0f;

        if (Input.GetKey(KeyCode.W)) {
            y += 1;
            z = 0;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            x -= 1;
            z = 90;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            x += 1;
            z = -90;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            y -= 1;
            z = 180;
            isMoving = true;
        }

        if (inputInverted)
        {
            x *= -1f;
            y *= -1f;
        }

       
        // setting up the rotation of the legs and their direction for moving
        Vector2 direction = new Vector2(x, y).normalized;;
        Quaternion turn = Quaternion.Euler(0, 0, z);
        // set property true/false for animation change
        anim.SetBool("walking", isMoving);
        // applying new rotation and direction
        Vector2 velocity = direction * speed * speedMultiplier;
        body.linearVelocity = velocity;
        body.transform.rotation = Quaternion.Slerp(body.transform.rotation, turn, 1);

        if (isMoving)
        {
            isMoving = false;
        }





    }

    public void SetInputInverted(bool value)
    {
        inputInverted = value;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
}
