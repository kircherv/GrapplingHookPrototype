﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{

    //plaziere auf cursorpoint
    public GameObject grapplingHookPrefab;
    public GameObject currentGrapplingHook;
    public GameObject failedGrapplePrefab;
    public GameObject failedGrapple;
    // public Rigidbody rigidbody;
    private Vector3 grapplePoint = new Vector3(0, 0, 0);

    public bool grappleDeployed = false;
    public float grappleRetractSpeed = 10.0f;
    public float grapplePayoutSpeed = 10.0f;
    private float ropeLength;
    private Vector3 ropeTension = Vector3.zero;
    private Vector3 directionToGrapple;
    public Vector3 customGravity;
    public int gravityFactor = 60;
    private int retracting;

    private bool clambering = false;
    private float clamberTolerance = 1.0f;
    private Vector3 clamberPoint = Vector3.zero;

    public Vector3 CursorPosition()
    {
        //raytrace von camera auf cursor
        Ray rayFromCameratoMouse = Camera.main.ScreenPointToRay(Input.mousePosition);

        //baue plane auf deren normale auf die camera zeigt
        //Use a function of this plane to set that variable to the distance along the ray that it intersects
        Plane zEqualsZero = new Plane(new Vector3(0, 0, -1), new Vector3(0, 0, 0));
        //distance to z=0 plane
        float distanceFromCameraToZEqualsZero = 0f;
        zEqualsZero.Raycast(rayFromCameratoMouse, out distanceFromCameraToZEqualsZero);

        return rayFromCameratoMouse.GetPoint(distanceFromCameraToZEqualsZero);
    }

    public void setRopeLength()
    {
        ropeLength = Vector3.Distance(grapplePoint, transform.position);
    }

    public bool isClambering()
    {
        return clambering;
    }


    public void UpdateGrappleAncher()
    {
        if (GetComponent<HingeJoint>())
        {
            // hingeJoint.anch
        }
    }

    private void DeployGrappleHook()
    {
        //calculate how much gravity should pull us this frame
        customGravity = Physics.gravity * gravityFactor;

        //vector from our position to our grapple point
        Vector3 vectorToGrapple = grapplePoint - transform.position;

        //Scalar distance from us to grapple
        float distanceToGrapple = vectorToGrapple.magnitude;

        //Unit vector of the direction from us to grapple
        directionToGrapple = vectorToGrapple / distanceToGrapple;

        //how fast is our velocity in the direction of the achor
        float speedTowardsAnchor = Vector3.Dot(GetComponent<Rigidbody>().velocity, directionToGrapple);

        if (grappleDeployed)
        {
            //Rope counteracts the pulling away from anchor component of gravity
            addTension();
            if (retracting == 0)
            {
                if (speedTowardsAnchor != 0)
                {
                    //Add same amount in contrary direction
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity - (speedTowardsAnchor * directionToGrapple);
                }
            }
            //pull to grapple
            else if (retracting == 1)
            {
                if (speedTowardsAnchor != grappleRetractSpeed)
                {
                    //neutralise grapple towards velocity
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity - (speedTowardsAnchor * directionToGrapple);

                    //make it grapple retract speed
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity + (grappleRetractSpeed * directionToGrapple);
                }

                ropeLength = ropeLength - (grappleRetractSpeed * Time.deltaTime);

                if (clambering && ropeLength < clamberTolerance)
                {
                    Vector3 clamberOffset = new Vector3(0, 0, 0);

                    if (Physics.Raycast(clamberPoint, new Vector3(-1, 0, 0), 1))
                    {
                        clamberOffset = new Vector3(-1, 2, 0);
                    }
                    else if (Physics.Raycast(clamberPoint, new Vector3(1, 0, 0), 1))
                    {
                        clamberOffset = new Vector3(1, 2, 0);
                    }
                    else
                    {
                        clambering = false;
                    }

                    if (clambering)
                    {
                        //shift him up the roof
                        transform.position = clamberPoint + clamberOffset;
                        //upright
                        transform.rotation = Quaternion.identity;
                        //stationary
                        GetComponent<Rigidbody>().velocity = Vector3.zero;
                        //Detach
                        Ungrapple();

                        clambering = false;
                    }

                }
            }
            else if (retracting == -1)
            {
                //payout rope
                if (speedTowardsAnchor != grapplePayoutSpeed * -1)
                {
                    //neutralise grapple towards velocity
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity - (speedTowardsAnchor * directionToGrapple);

                    //make it grapple retract speed
                    GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity - (grapplePayoutSpeed * directionToGrapple);
                }

                //minus because if we are moving away , velocity will be negative , we want rope length to increase 
                ropeLength = ropeLength + (grapplePayoutSpeed * Time.deltaTime);
            }

            Debug.Log("Rope Length: " + ropeLength + " Distance to grapple: " + distanceToGrapple);
            Debug.Log("Needed " + (grappleRetractSpeed - speedTowardsAnchor) + " Speed to Anchor: " + speedTowardsAnchor);
        }//grapple Deployed
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        DeployGrappleHook();

        
    }

    private void Update()
    {
        //GH was destroyed
        if(currentGrapplingHook == null)
        {
            grappleDeployed = false;
        }

        //shooting the grapple and disconnecting
        //onClick
        if (Input.GetMouseButtonDown(0))
        //if(Input.GetButtonDown("Click"))
        {
            Destroy(failedGrapple);
            if (grappleDeployed)
            {
                //Disconnect
                Ungrapple();
            }
            else
            {
                //Fire grapple
                RaycastHit grappleHit;
                if(Physics.Raycast(transform.position, CursorPosition()-transform.position, out grappleHit))
                {
                    //we hit something!
                    //grapple onthe first thing we hit
                    grapplePoint = grappleHit.point;

                    //create the hook there, and remeber it to be deleted later
                    currentGrapplingHook = Instantiate(grapplingHookPrefab, grapplePoint, transform.rotation) as GameObject;

                    //note how long the rope should be
                    setRopeLength();

                    //remember we are swinging now
                    grappleDeployed = true;
                } else
                {
                    //we hit nothing, but fire anyway: create object in this position
                    failedGrapple = Instantiate(failedGrapplePrefab, CursorPosition(), transform.rotation) as GameObject;
                }
            }
            //create object in this position
           // Instantiate(grapplingHookPrefab, CursorPosition(), transform.rotation);
        }
            if(grappleDeployed)
            {
                if(Input.GetButton("Up") && !Input.GetButton("Down"))
                {
                    retracting = 1;
                }
                else if (Input.GetButton("Down") && !Input.GetButton("Up"))
                {
                    retracting = -1;
                }
               else
                {
                    retracting = 0;
                }
            }

    }

    private void Ungrapple()
    {
        //destroy last grapple point
        Destroy(currentGrapplingHook);
        grappleDeployed = false;
    }

    private void addTension()
    {
        //Add the tension of the rope = how hard gravity is pulling
        ropeTension = directionToGrapple * Vector3.Dot(customGravity, directionToGrapple) * -1;

        //if gravity is pulling away from the grapple, its anchor component is negative, so we multiply it by -1 because we want a positive forcfe input value
        GetComponent<Rigidbody>().AddForce(ropeTension);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("ClamberTrigger") && grappleDeployed)
        {
            clambering = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ClamberTrigger") && grappleDeployed)
        {
            clambering = true;
            clamberPoint = other.transform.position;
        }
    }

}

