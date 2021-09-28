using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravityController : MonoBehaviour
{
    [SerializeField]
    private float _gravity = -9.81f; // default unity gravity

    public void Attract(Rigidbody rigidbody) {
        
        // Creating a vector that points from the center of the planet towards the rigidbody
        Vector3 gravityUp = (rigidbody.position - transform.position).normalized;

		// Applying a force to pull the rigidbody towards the planet
		rigidbody.AddForce(gravityUp * _gravity, ForceMode.Force);

		// Rotating rigidbody, to be aligned with the planet
        Vector3 localUp = rigidbody.transform.up;
		rigidbody.rotation = Quaternion.FromToRotation(localUp, gravityUp) * rigidbody.rotation;
    }
}
