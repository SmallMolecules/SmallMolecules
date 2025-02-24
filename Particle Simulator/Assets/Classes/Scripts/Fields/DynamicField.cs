using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** @brief A base class for dynamic field implementations

    This class specifies the requirements for an implemented instance of a dynamic field -
    a field that calculates forces between two particles. The force is added to both particles
    (due to conservation of forces).
    @author Isaac Bergl
    @author Dhruv Jobanputra
    @date November 2021
    \see StaticField Lennard-Jones Coloumb
    */
public class DynamicField
{
    /**Reference to the scales object of the parent simulator*/
    private Scales scales;

    /**
    The constructor method
    @param sim - the parent simulator (Simulator)
    */
    protected DynamicField(Simulator sim)
    {
        scales = sim.scales;
    }

    /**
    Virtual function that provides the dynamics of the field. Should be overwritten 
    by a custom function to provide the force on the particle.
    @param particle A (Particle)
    @param particle B (Particle)
    @returns zero vector (Vector3)
    */
    public virtual Vector3 FieldDynamics(Particle A, Particle B)
    {
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    /**
    Applies a force to the input particles. 
    @param particle A (Particle)
    @param particle B (Particle)
    */
    public void ApplyForce(Particle A, Particle B)
    {
        Vector3 F = FieldDynamics(A, B) * scales.time.VAL * scales.time.VAL;

        A.AddForce(F);
        B.AddForce(-F);
    }

}
