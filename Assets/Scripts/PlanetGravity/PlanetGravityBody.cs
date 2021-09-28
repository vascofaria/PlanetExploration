using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlanetGravityBody : MonoBehaviour
{
    private PlanetGravityController _planetGravityController;
    private Rigidbody _rigidbody;
    // Start is called before the first frame update
    void Start()
    {
        _planetGravityController = FindObjectOfType<PlanetGravityController>();
        if (_planetGravityController == null)
            throw new MissingReferenceException("Could not find the PlanetGravityController in the Scene, it must exist!");

        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _planetGravityController.Attract(_rigidbody);
    }
}
