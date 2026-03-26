using UnityEngine;

public class NPCHead : MonoBehaviour
{
    public Rigidbody2D head;
    public GameObject torso;

    float targetAngleOffset = 40f;
    float baseAngleOffset = 40f;

    [SerializeField] float hitRotationAmount = 30f;
    [SerializeField] float returnSpeed = 2f;

    void Start()
    {
        head = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        stickToTorso();
        updateRotation();
    }

    public void stickToTorso()
    {
        head.transform.position = torso.transform.position;
    }

    public void updateRotation()
    {
        targetAngleOffset = Mathf.LerpAngle(targetAngleOffset, baseAngleOffset, Time.deltaTime * returnSpeed);
        head.transform.rotation = torso.transform.rotation * Quaternion.Euler(0f, 0f, targetAngleOffset);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("RightHand"))
        {
            targetAngleOffset = baseAngleOffset - hitRotationAmount;
        }
        else if (other.CompareTag("LeftHand"))
        {
            targetAngleOffset = baseAngleOffset + hitRotationAmount;
        }
    }
}