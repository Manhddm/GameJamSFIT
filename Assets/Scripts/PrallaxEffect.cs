using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;

    private Vector2 startPos;

    private float startZ;
    private float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;
    public Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startPos;

    private float clippingPlane =>
        (cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane));
    private float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;
    void Start()
    {
        startPos = transform.position;
        startZ =  transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newPos = startPos + camMoveSinceStart * parallaxFactor;
        
        transform.position =  new Vector3(newPos.x, newPos.y, transform.position.z);
    }
}
