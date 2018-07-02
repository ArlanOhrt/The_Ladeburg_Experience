using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.UI;

/// <summary>
/// 
/// This script should be attached to any explosive source, and it will handle all parameters of the explosion event.
/// Including the effective blast radius from weight of charge, get targets and their info (surface area from bombs
/// perspective and position) within that radius, and use that info to add a realistic force to the charge using a
/// simplified (corrected) Kingery model.
/// 
/// Author: Intern, Arlan Ohrt, Summer 2018
/// 
/// </summary>

// Set what components are required, and if they are not there, add them
[RequireComponent(typeof(Rigidbody))]

public class RealisticExplosion : MonoBehaviour {

    public GameObject explosionEffect;
    public GameObject explosionSound;

    private Vector3 explosivePosition;
    private Vector3 targetPosition;
    private Vector3 unitForceVector;
    private Vector3 forceVector;

    ExplosiveType explosiveType;

    private double effectiveRadius;
    private double A, B, C, D, E;
    private double incidentOverpressure;
    private double scaledRange;
    private double distanceBetween;
    private double explosiveWeight;
    private double minimumRelevantPressure;

    private float effectiveSurfaceArea;
    private float forceMagnitude;

    public int counter;

    public void Explode(ExplosiveType explosiveType,ExplosiveLocation location, double minimumRelevantPressure, double explosiveWeight)
    {
        Instantiate(explosionEffect, gameObject.transform.position, gameObject.transform.rotation);
        Instantiate(explosionSound, gameObject.transform.position, gameObject.transform.rotation);
        String locationString = "";

        // From explosive type, determine weight
        if (explosiveType == ExplosiveType.TNT)
            explosiveWeight = explosiveWeight * 1;
        else if (explosiveType == ExplosiveType.PETN)
            explosiveWeight = explosiveWeight * 1.66;

        // From explosive location, store string for comparison
        if (location == ExplosiveLocation.ExteriorWall)
            locationString = "Exterior";
        else if (location == ExplosiveLocation.InteriorWall)
            locationString = "Interior";
        // Determine bomb position
        Rigidbody rbExplosive = GetComponent<Rigidbody>();
        explosivePosition = rbExplosive.position;

        // Determine effective radius (in feet)
        for (int i = 1; ; i++)
        {
            CalculateScaledRange(i, explosiveWeight);

            if (scaledRange > 500)
                incidentOverpressure = 0;

            else if (scaledRange < 0.5)
                incidentOverpressure = minimumRelevantPressure * 2; // This is to make sure for values with unknown coeff, we still stay in loop

            else
            {
                DetermineCoeff(scaledRange);
                CalculateIncidentOverpressure(); // This calculated in psi
            }

            if (incidentOverpressure <= minimumRelevantPressure)
            {
                effectiveRadius = i;
                break;
            }
        }

        // Remove the bomb itself
        if(gameObject != null)
            Destroy(gameObject);

        // Determine colliders in the effective radius
        Collider[] collidersToMove = Physics.OverlapSphere(transform.position, (float)effectiveRadius);

        // Determine number of effected colliders with proper tag
        counter = 0;

        // Effect each collider in the effective radius
        foreach (Collider nearbyObject in collidersToMove)
        {
            if(nearbyObject.tag == locationString)
            {
                // Get each objects rigid body
                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                // Get each objects mesh
                Mesh mesh = nearbyObject.GetComponent<MeshFilter>().mesh;

                if (rb != null)
                {
                    counter++;

                    // Deactivate isKinematic so the objects can have force applied to them
                    rb.isKinematic = false;

                    // Get the unit vector between bomb and target
                    targetPosition = rb.position;
                    unitForceVector = targetPosition - explosivePosition;

                    // Get distance between 
                    distanceBetween = Vector3.Distance(targetPosition, explosivePosition);

                    // Get the pressure at the distance away
                    CalculateScaledRange(distanceBetween, explosiveWeight);
                    if (scaledRange != 0)
                    {
                        DetermineCoeff(scaledRange);
                        CalculateIncidentOverpressure(); // This calculated in psi
                    }
                    else
                        incidentOverpressure = 0;

                    // Get the surface area of the target's mesh
                    effectiveSurfaceArea = SurfaceArea(mesh);

                    // Get the magnitude of the force 
                    forceMagnitude = (float)incidentOverpressure * 144 * effectiveSurfaceArea;

                    // Get the force vector               
                    forceVector = forceMagnitude * unitForceVector;

                    // Apply force vector to object;
                    rb.AddForce(forceVector);
                }
            }
        }
        // Output values for checking purposes
        Debug.Log("Explosive type = " + explosiveType);
        Debug.Log("Explosive weight = " + explosiveWeight);
        Debug.Log("Explosive target = " + locationString);
        Debug.Log("Min Rel Pressure = " + minimumRelevantPressure);
        Debug.Log("Effective Radius = " + effectiveRadius);
    }

    // Purpose of this function is to determine the scaled distance between charge and object
    void CalculateScaledRange(double distance, double explosiveWeight)
    {
        double power = 1.0 / 3.0;
        scaledRange = distance / (Math.Pow(explosiveWeight, power));
    }

    // This function uses the scaledRange to get desired coeff from a table
    void DetermineCoeff(double scaledRange)
    {
        if (scaledRange >= 0.5 && scaledRange <= 7.25)
        {
            A = 6.9137;
            B = -1.4398;
            C = -0.2815;
            D = -0.1416;
            E = 0.0685;
        }

        else if (scaledRange > 7.25 && scaledRange <= 60)
        {
            A = 8.08035;
            B = -3.7001;
            C = 0.2709;
            D = 0.0733;
            E = -0.0127;
        }

        else if (scaledRange > 60 && scaledRange <= 500)
        {
            A = 5.4233;
            B = -1.4066;
            C = 0;
            D = 0;
            E = 0;
        }

    }

    void CalculateIncidentOverpressure()
    {
        double T = Math.Log(scaledRange);
        if (scaledRange > 500)
            incidentOverpressure = 0;
        else
            incidentOverpressure = Math.Exp(A + (B * T) + (C * Math.Pow(T, 2)) + (D * Math.Pow(T, 3)) + (E * Math.Pow(T, 4)));
    }

    // Calculates the viewed surface area from the perspective of the explosive origin
    public float SurfaceArea(Mesh mesh)
    {
        float surfaceArea = 0.0f;

        // Get triangles and mesh
        Vector3[] verts = mesh.vertices;
        int[] indices = mesh.triangles;

        // Find Points by navigating the list
        for (int i = 0; i < mesh.triangles.Length;)
        {
            // Get each triangle point
            Vector3 P1 = verts[indices[i++]];
            Vector3 P2 = verts[indices[i++]];
            Vector3 P3 = verts[indices[i++]];

            // Get triangles normal from points
            Vector3 edge1 = P3 - P2;
            Vector3 edge2 = P1 - P2;
            Vector3 triangleNormal = Vector3.Cross(edge1, edge2);

            // Get dot of normal and vector between explosive origin and triangle
            Vector3 originToTriangle = P2 - gameObject.transform.position;
            float dotProduct = Vector3.Dot(triangleNormal, originToTriangle);

            if (dotProduct < 0)
            {
                float addedArea = 0.5f * Vector3.Magnitude(triangleNormal);
                surfaceArea = surfaceArea + addedArea;
            }
        }

        return surfaceArea;
    }

}