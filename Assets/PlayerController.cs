using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    //Movement
    public float moveSpeed = 7;
    public float moveForce = 3000;
    public Rigidbody playerRigidbody;
    public Vector3 resetPlayerPosition = new Vector3(-6, 1.2f, 0);

    private Vector3 playerVec3Rotation = new Vector3(0, 0, 0);
    private Quaternion playerStartRotation;
    //Grounding
    private float distanceToGround = 1;

    private bool leftSideGrounded = false;
    private bool rightSideGrounded = false;

    public Collider m_Collider;
    Vector3 m_Center;
    Vector3 m_Size, m_Min, m_Max;

    // Use this for initialization
    void Start()
    {

        //ColliderBounds
        //Fetch the Collider from the GameObject
        m_Collider = GetComponent<Collider>();
       
        //Output this data into the console
        OutputData();

        playerRigidbody = GetComponent<Rigidbody>();
        //abstand zum boden
        distanceToGround = m_Collider.bounds.extents.y;

        playerStartRotation = transform.rotation;
    }

    //is player on ground, check which side has to be blocked on air
    bool isGrounded()
    {
        leftSideGrounded = Physics.Raycast(transform.position + new Vector3(-0.5f, 0, 0), Vector3.down, distanceToGround + 0.1f);
        rightSideGrounded = Physics.Raycast(transform.position + new Vector3(0.5f, 0, 0), Vector3.down, distanceToGround + 0.1f);
        return (leftSideGrounded || rightSideGrounded);
    }

    void OutputData()
    {
        //Output to the console the center and size of the Collider volume
        Debug.Log("Collider Center : " + m_Center);
        Debug.Log("Collider Size : " + m_Size);
        Debug.Log("Collider bound Minimum : " + m_Min);
        Debug.Log("Collider bound Maximum : " + m_Max);
    }

    void resetPlayer()
    {
        Debug.Log(playerRigidbody.velocity);
        //reset position
        transform.position = resetPlayerPosition;
        transform.rotation = playerStartRotation;
        //transform.rotation = Quaternion.Euler(playerVec3Rotation);
        //zero velocity
        playerRigidbody.velocity = new Vector3(0, 0, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "DeathZone")
        {
            resetPlayer();
            Debug.Log("player died");
        }
    }

    // Update is called once per frame
    void FixedUpdate () {

        
     if (Input.GetButton("MoveRight") && !Input.GetButton("MoveLeft") && isGrounded())
     {
         if (playerRigidbody.velocity.x < moveSpeed)
         {
             playerRigidbody.AddForce(moveForce * Vector3.right);
         }
     }
     else if (Input.GetButton("MoveLeft") && !Input.GetButton("MoveRight") && isGrounded())
     {
         if (playerRigidbody.velocity.x > moveSpeed * -1)
         {
             playerRigidbody.AddForce(moveForce * Vector3.left);
         }
     }

        if (Input.GetButton("Reset")|| gameObject.tag == "DeathZone")
        {
            resetPlayer();
        }

    }
}
