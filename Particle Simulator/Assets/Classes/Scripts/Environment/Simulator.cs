using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

/** @brief Class for managing the properites of a simulator instance

    This class manages an overall simulation - the internal workings of what
    is considered a "box". This class holds the particles, the fields and the scales.
    @author Isaac Bergl
    @author Dhruv Jobanputra
    @date September 2021
    \see Simulator Scales
    */
public class Simulator : MonoBehaviour
{
    /**Set of scales used by the simulator. This object is referenced by other 
    objects such as particles and fields.
    /see Scales Scale*/
    public Scales scales = new Scales();

    /**Dictates if the simulation is paused. True if this simulation is paused, false otherwise*/
    [HideInInspector] public bool paused;

    /**Specifies if destroy mode has been activated*/
    private bool destroy = false;

    /**The actual box environment object for the current simulation*/
    private GameObject box;

    /**Reference to the simulation manager
    /see SimulationManager*/
    private SimulationManager manager;

    /**The GameObject to spawn (Particle Object)*/
    [HideInInspector] public GameObject particleSpawner;

    /**The GameObject Environment to spawn (Box)*/
    [HideInInspector] public GameObject boxEnvironment;

    /**List of the particles*/
    List<Particle> particles = new List<Particle>();
    /**List of the dynamic fields
    /see DynamicField*/
    List<DynamicField> dynamicFields = new List<DynamicField>();
    /**List of the static fields
    /see StaticField*/
    List<StaticField> staticFields = new List<StaticField>();

    /**System.Random object for random number generation. Each time program starts, a
    random seed is generated and used to construct this object*/
    // private System.Random rand;
    /**The seed used to generate this simulator's random object*/
    private int seed;

    /**
    \see @link https://docs.unity3d.com/ScriptReference/MonoBehaviour.Start.html
    */

    /**Ratio of wall thickness to length of the inside of the sides is 1:40
    Ratio of unity scale to unity length is 1:10
    The values are obtained from the box prefab. Changing the constant will not change 
    the box prefab thickness
    */
    [HideInInspector] public float BOX_THICKNESS_SCALE = 0.025f;
    [HideInInspector] public float BOX_LENGTH_SCALE = 10;
    [HideInInspector] public float boxLength;
    [HideInInspector] public float wallThickness;

    private System.Random rand = new System.Random();
    void Start()
    {
        manager = transform.parent.gameObject.GetComponent<SimulationManager>();
        box = Instantiate(boxEnvironment, transform.position, transform.rotation);
        box.transform.parent = this.transform;
        UpdateBoxSize(box.transform.localScale.x);


        for (int i = 0; i < manager.NUM_PARTICLES; i++)
        {

            float radius = Random.Range(1, 2);
            float mass = Random.Range(1, 2);
            int charge = (int)Random.Range(0, 3) - 1;

            AddNewParticle(GenerateRandomCoords(radius), mass, radius, charge);
        }

        dynamicFields.Add(new LennardJones(this));
        dynamicFields.Add(new Coloumb(this));

    }

    /**
    \see @link https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html
    */
    void Update()
    {
        if (paused || manager.paused)
        {
            if (destroy) HandleDestroyParticle();
            return;
        }

        for (int a = 0; a < particles.Count; a++)
        {
            UpdateVelocity(a);
        }

        UpdatePositions();
    }
    /**Updates the velocity of the particle with an index of "a" in the list
    @param a - the index of the particle to update (int)*/
    private void UpdateVelocity(int a)
    {
        foreach (StaticField F in staticFields)
        {
            F.ApplyForce(particles[a]);
        }

        foreach (DynamicField F in dynamicFields)
        {
            for (int b = a + 1; b < particles.Count; b++)
            {
                F.ApplyForce(particles[a], particles[b]);
            }
        }
    }

    /**Updates the positions of all the particles in the list according to thier velocity*/
    private void UpdatePositions()
    {
        foreach (Particle A in particles)
        {
            A.CheckBoxCollision();
            A.Step();
            CheckOutOfBounds(A);
        }
    }

    /**Adds a new particle at a given position with the specified parameters
    @param pos (Vector3)
    @param mass (float)
    @param radius (float)
    @param charge (int)*/
    public void AddNewParticle(Vector3 pos, float mass, float radius, int charge)
    {
        GameObject sphere = Instantiate(particleSpawner, transform);
        sphere.transform.localPosition = pos;
        particles.Add(new Particle(sphere, scales, mass, radius, charge));
    }

    /**Adds a particle at a random position with default physical properties
    /see AddNewParticle
    */
    public void AddNewParticleRandom()
    {
        float radius = Random.Range(1, 2);
        float mass = Random.Range(1, 2);
        int charge = (int)Random.Range(0, 3) - 1;
        AddNewParticle(GenerateRandomCoords(), mass, radius, charge);
    }

    /**Called by the UI elements to change the time scale. Also updates
    the velocities of all the paritlces by calculating the ratio of the 
    time-scale change.
    @param coeff - the coefficient of the time scale (float)
    @param exp - the exponent of the time scale (int)*/
    public void UpdateTime(float coeff, int exp)
    {
        Scale ratio = new Scale(coeff / scales.time.COEFF, exp - scales.time.EXP);
        scales.SetTime(coeff, exp);
        foreach (Particle A in particles)
        {
            A.adjustVelocity(ratio.VAL);
        }
    }

    /**Removes a particle, A, from the simulation
    @param A - the particle to remove (Particle)*/
    private void RemoveParticle(Particle A)
    {
        Destroy(A.particle);
        particles.Remove(A);
    }

    /**Toggles the pause state of the simulation*/
    public void TogglePause()
    {
        paused = !paused;
    }

    /**Checks if a particle if clicked to destroy
    and calls RemoveParticle()*/
    private void HandleDestroyParticle()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit[] hits;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int layerMask = 1 << 6;
            hits = Physics.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, layerMask);
            bool deleted = false;
            for (int a = 0; a < particles.Count; a++)
            {
                for (int h = 0; h < hits.Length; h++)
                {
                    if (hits[h].transform.position == particles[a].particle.transform.position)
                    {
                        if (!deleted)
                        {
                            RemoveParticle(particles[a]);
                            deleted = true;
                        }
                    }
                }
            }
        }
    }

    /**Toggles the destroy particle option of the simulation
    @param set - a bool to set destroy to true or false
    */
    public void ToggleDestroy(bool set)
    {
        destroy = set;
    }

    /**Called by the UI elements to change the size of the box
    @param coeff - the coefficient of the size scale (float)
    */
    public void UpdateBoxSize(float coeff)
    {
        if (box == null) return;
        box.transform.localScale = new Vector3(coeff, coeff, coeff);
        boxLength = box.transform.localScale.x * BOX_LENGTH_SCALE;
        wallThickness = boxLength * BOX_THICKNESS_SCALE;

    }

    /**Generates a random coordinate that is inside the bounds of the box
    @param radius - the size of the particle to be added
    */
    public Vector3 GenerateRandomCoords(float radius = 1f)
    {
        float halfLength = boxLength / 2 - radius;
        float fullLength = boxLength - radius;
        float minimum = wallThickness + radius;

        float x = rand.Next((int)Mathf.Ceil(-halfLength), (int)Mathf.Floor(halfLength));
        float y = rand.Next((int)Mathf.Ceil(minimum), (int)Mathf.Floor(fullLength));
        float z = rand.Next((int)Mathf.Ceil(minimum), (int)Mathf.Floor(fullLength));

        Vector3 relative = new Vector3(x, y, z);
        return relative;
    }

    /**Checks if the particle is outside the bounds 
    of the boxand puts it back in
    @param p - the particle being checked
    */
    private void CheckOutOfBounds(Particle p)
    {
        Vector3 pos = p.particle.transform.localPosition;
        float radius = p.radius;

        float halfLength = boxLength / 2 - radius;
        float fullLength = boxLength + wallThickness - radius;
        float minimum = wallThickness + radius;

        float x = pos.x;
        float vx = p.velocity.x;
        float vy = p.velocity.y;
        float vz = p.velocity.z;

        if (pos.x < -halfLength)
        {
            x = -halfLength;
            vx = -vx;
        }
        if (pos.x > halfLength)
        {
            x = halfLength;
            vx = -vx;
        };

        float y = pos.y;
        if (pos.y < minimum)
        {
            y = minimum;
            vy = -vy;
        }

        if (pos.y > fullLength)
        {
            y = fullLength;
            vy = -vy;
        }

        float z = pos.z;
        if (pos.z < minimum)
        {
            z = minimum;
            vz = -vz;
        }

        if (pos.z > fullLength)
        {
            z = fullLength;
            vz = -vz;
        }

        p.velocity = new Vector3(vx, vy, vz);
        p.particle.transform.localPosition = new Vector3(x, y, z);

    }
}