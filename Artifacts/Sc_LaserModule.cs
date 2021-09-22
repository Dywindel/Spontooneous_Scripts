using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This laser module fires a straight laser
// If it hits a mirror it is reflected
// If it hits the ground/water, stop with a particle effect

public class Sc_LaserModule : MonoBehaviour
{
    int i = 0;

    public LineRenderer lR;
    public ParticleSystem pS_Start;
    public ParticleSystem pS_End;
    

    // Update is called once per frame
    void LateUpdate()
    {
        // For now, just to reduce the load on the PC
        i += 1;
        if (i % 3 == 0)
        {
            int bounceLimit = 10; // Max of 10 bounces

            // list of positions
            // list of forward directions (From which the laser is fired)
            List<Vector3> listOf_Positions = new List<Vector3>();
            List<Vector3> listOf_Forwards = new List<Vector3>();

            Vector3 forwardCast = transform.forward;

            RaycastHit hit_Laser;
            // We want to reflect off of mirrors
            LayerMask mask_Mirror = LayerMask.GetMask( LayerMask.LayerToName((int)CollLayers.Obstacle), LayerMask.LayerToName((int)CollLayers.LaserInteraction), LayerMask.LayerToName((int)CollLayers.Flammable) );

            // Prepare the first ray
            Ray laserRay = new Ray(transform.position, forwardCast);
            // Store the first point, which is based on the initial direction we're moving in
            listOf_Positions.Add(transform.position);

            // Store the last item you hit
            Transform hitLast = transform;

            // Check we haven't hit a stopping point
            bool checkForWall = false;

            // We continue to test hits until we stop hitting mirror (Or we reach a cap) (Or we hit the same item twice)
            while (!checkForWall && Physics.Raycast(laserRay, out hit_Laser, RL_V.laserMaxDistance, mask_Mirror) && listOf_Positions.Count <= bounceLimit && hit_Laser.transform != hitLast)
            {
                // I'm not sure what the 'Stop' item will be yet
                if (hit_Laser.transform.gameObject.layer == (int)CollLayers.Obstacle)
                {
                    // Store the new hit position point
                    listOf_Positions.Add(hit_Laser.point);
                    // This is a wall
                    checkForWall = true;
                }
                else if (hit_Laser.transform.gameObject.layer == (int)CollLayers.Flammable)
                {
                    // Set fire to object
                    // Disable the game object and run it's onFlammable parents
                    hit_Laser.transform.gameObject.SetActive(false);
                    hit_Laser.transform.parent.gameObject.GetComponent<Generic_OnFunction>().On_Flammable_Portcullis();
                }
                else
                {
                    // Update the new forward by taking the cross produce of the last forwardCast and the hit position's forwardCast
                    forwardCast = Vector3.Reflect(forwardCast, hit_Laser.transform.forward);
                    // Store the new hit position point
                    listOf_Positions.Add(hit_Laser.point);
                    // Create a new ray based on the new values
                    laserRay = new Ray(listOf_Positions.Last(), forwardCast);
                    // Store the last item hit
                    hitLast = hit_Laser.transform;
                }
            }

            // Update the number of setpoints
            lR.positionCount = listOf_Positions.Count;

            // Once we've finished hitting things, we create the points for the line renderer
            for (int p = 0; p < listOf_Positions.Count; p++)
            {
                lR.SetPosition(p, listOf_Positions[p]);
            }

            // The final point is either a wall
            if (checkForWall)
            {
                // Place the particle system at the last hit position
                pS_End.transform.position = listOf_Positions.Last();

                // Rotate it to face the opposite direction to where the laser is firing
                pS_End.transform.rotation = Quaternion.LookRotation(-forwardCast, Vector3.up);
            }
            else
            {
                // Add an additional position
                lR.positionCount = listOf_Positions.Count + 1;
                // or is imagined to be infinite
                lR.SetPosition(listOf_Positions.Count, listOf_Positions[listOf_Positions.Count - 1] + (forwardCast * RL_V.laserMaxDistance));

                // Place the particle system at the end of this position
                pS_End.transform.position = listOf_Positions[listOf_Positions.Count - 1] + (forwardCast * RL_V.laserMaxDistance);
            }
        }
    }
}
