using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this to a gameobject to draw a reference Gizmo
// In the editor window
public class Sc_Gizmos : MonoBehaviour
{
    public enum GizmoIcon {TargetGizmo, ColliderGizmo, RockGizmo, TextGizmo, NPCGizmo, LightingGizmo, StartGizmo, EndGizmo, ResetGizmo}

    public GizmoIcon gizmoIcon;

    #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            string customName = gizmoIcon.ToString() + ".png";
            Gizmos.DrawIcon(transform.position, customName, true);

            if (gizmoIcon == GizmoIcon.TargetGizmo)
            {
                Gizmos.color = new Color(1.0f, 0.25f, 0.25f, 0.25f);
                Gizmos.DrawSphere(transform.position, 0.4f);
            }
            else if (gizmoIcon == GizmoIcon.ColliderGizmo)
            {
                Gizmos.color = new Color(0.25f, 1.0f, 0.25f, 0.25f);
                // Grab the box collider for reference dimensions
                BoxCollider collider = this.GetComponent<BoxCollider>();
                Gizmos.DrawCube(transform.position + collider.center, collider.size);
            }
            else if (gizmoIcon == GizmoIcon.RockGizmo)
            {
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.45f);
                // Grab the box collider for reference dimensions
                BoxCollider collider = this.GetComponent<BoxCollider>();
                Gizmos.DrawCube(transform.position + collider.center, collider.size);
            }
            else if (gizmoIcon == GizmoIcon.LightingGizmo)
            {
                Gizmos.color = new Color(0.75f, 0.75f, 0.0f, 0.45f);
                // Grab the box collider for reference dimensions
                BoxCollider collider = this.GetComponent<BoxCollider>();
                Gizmos.DrawCube(transform.position + collider.center, collider.size);
            }
        }
    
    #endif
}