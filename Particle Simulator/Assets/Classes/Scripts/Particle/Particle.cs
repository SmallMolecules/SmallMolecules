using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** @brief Class for managing the properites of particle objects

    This class manages particle properties such as position, velocity, radius, mass,
    charge and GameObject is is binds to.
    @author Isaac Bergl
    @author Dhruv Jobanputra
    @date September 2021
    \see Simulator Scales
    */
public class Particle
{
    /**The GameObject that this object is dictating*/
    [HideInInspector] public GameObject particle;

    /**Mass of the particle. Min value of 1.0*/
    [HideInInspector] public float mass;
    /**Radius of the particle. Min value of 1.0*/
    [HideInInspector] public float radius;
    /**Charge of the particle. Min value of 1*/
    [HideInInspector] public int charge;

    /**Vector velocity of the particle. Dictates the position.*/
    [HideInInspector] public Vector3 velocity;
    /**Vector position of the particle. Dictates the GameObject position */
    private Vector3 position;

    /**Reference to the Scales of the parent Simulator*/
    [HideInInspector] public Scales scales;

    /**
    Constructor for a particle. Sets the GameObject to be the one given in as a parameter, 
    as well as passing a reference to the Scales, s, and the physical properties.
    @param p (GameObject)
    @param S (Scales)
    @param setMass (float)
    @param setRadius (float)
    @param setCharge (int)
    */
    public Particle(GameObject p, Scales S, float setMass, float setRadius, int setCharge)
    {
        particle = p;
        position = p.transform.position;
        velocity = new Vector3(0.0f, 0.0f, 0.0f);

        scales = S;

        var sphereRenderer = particle.GetComponent<Renderer>();

        if (setCharge < 0)
        {
            sphereRenderer.material.SetColor("_Color", Color.red);
        }
        else if (setCharge > 0)
        {
            sphereRenderer.material.SetColor("_Color", Color.blue);
        }
        else
        {
            sphereRenderer.material.SetColor("_Color", Color.gray);
        }

        var myCollider = particle.GetComponent<SphereCollider>();

        myCollider.radius = setRadius * 0.5f;
        mass = setMass;
        charge = setCharge;
        radius = setRadius;

        particle.transform.localScale = new Vector3(setRadius * 2, setRadius * 2, setRadius * 2);
    }

    /**
    Gets the position of the particle
    @returns position (Vector3)
    */
    public Vector3 GetPos()
    {
        return position;
    }

    /**
    Gets the velocity of the particle
    @returns velocity (Vector3)
    */
    public Vector3 GetVel()
    {
        return velocity;
    }

    /**Updates the velocity when the timescale changes. Used by the
    particles parent simulator*/
    public void adjustVelocity(float ratio)
    {
        velocity *= ratio;
    }

    /**
    Adds a force to the particle 
    @param Unity unit scaled force F (Vector3)
    */
    public void AddForce(Vector3 F)
    {
        velocity += F / mass;
    }

    /**
    Steps the particle position and moves the GameObject according to
    its velocity.
    */
    public void Step()
    {
        position += velocity;
        particle.transform.Translate(velocity);
    }

    /**
    Checks if there was a collision with any wall of the environment box
    and reflects the velocity for the correct axis
    */
    public void CheckBoxCollision()
    {
        bool collideRight = Physics.Raycast(particle.transform.position, Vector3.right, radius);
        bool collideLeft = Physics.Raycast(particle.transform.position, Vector3.left, radius);
        bool collideUp = Physics.Raycast(particle.transform.position, Vector3.up, radius);
        bool collideDown = Physics.Raycast(particle.transform.position, Vector3.down, radius);
        bool collideForward = Physics.Raycast(particle.transform.position, Vector3.forward, radius);
        bool collideBack = Physics.Raycast(particle.transform.position, Vector3.back, radius);

        float vx = velocity.x;
        float vy = velocity.y;
        float vz = velocity.z;

        if (collideRight && vx > 0)
        {
            velocity.x = -vx;
        }
        else if (collideLeft && vx < 0)
        {
            velocity.x = -vx;
        }
        if (collideUp && vy > 0)
        {
            velocity.y = -vy;

        }
        else if (collideDown && vy < 0)
        {
            velocity.y = -vy;
        }
        if (collideForward && vz > 0)
        {
            velocity.z = -vz;
        }
        else if (collideBack && vz < 0)
        {
            velocity.z = -vz;

        }
    }
}
