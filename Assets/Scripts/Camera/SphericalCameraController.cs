using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalCameraController : MonoBehaviour {

    public Transform cameraTarget;

    [Header("Zoom settings")]

    [SerializeField]
    private float _zoomSensitivity;

    [SerializeField]
    private float _zoomRange;


    [Header("Rotation settings")]
    [SerializeField]
    private float _rotationSensitivity;

    private bool _pressingLeftMouseButton = false;

    void Start() {

        float planetRadius = SharedInfo.configurations.planetRadius;

        // Camera starts half way between the target and the maximum zooming range
        transform.position = (cameraTarget.position - transform.forward) * (planetRadius) * 2.0f;
    }

    void Update() {
#region ZOOM
        Vector3 zoomDirection = (cameraTarget.position - transform.position);

        // Stopping from zooming out of the zoom range
        if (zoomDirection.magnitude >= _zoomRange)
            transform.position = cameraTarget.position - zoomDirection.normalized * _zoomRange;
        else
            transform.position = transform.position + zoomDirection.normalized * _zoomSensitivity * Input.GetAxis("Mouse ScrollWheel");
#endregion
#region ROTATION

        // Detect if user has pressed or released the left mouse button
        if (Input.GetMouseButtonDown(0)) {
            _pressingLeftMouseButton = true;
        } else if (Input.GetMouseButtonUp(0)) {
            _pressingLeftMouseButton = false;
        }

        if (_pressingLeftMouseButton) {
            // Rotating the camera about the axis 
            // that passes through the camera target in world coordinates
            transform.RotateAround(cameraTarget.position, 
            transform.up, Input.GetAxis("Mouse X") * _rotationSensitivity);
            transform.RotateAround(cameraTarget.position, 
            -transform.right, Input.GetAxis("Mouse Y") * _rotationSensitivity);
        }
#endregion
    }
}