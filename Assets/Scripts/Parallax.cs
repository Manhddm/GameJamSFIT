using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;
    private Vector2 _startPos;
    private Vector2 CamMoveSinceStart => (Vector2)cam.transform.position - _startPos;
    private float ZDistanceFromTarget => transform.position.z - followTarget.position.z;
    private float ClippingPlane => (cam.transform.position.z + (ZDistanceFromTarget > 0? cam.farClipPlane: cam.nearClipPlane));
    private float ParallaxFactor => Mathf.Abs(ZDistanceFromTarget) /ClippingPlane;
    private float _startingZ;
    void Start(){
        _startPos = transform.position;
        _startingZ = transform.position.z;
    }

    void Update()
    {
        Vector2 newPos = _startPos * CamMoveSinceStart / ParallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, _startingZ);
    }
}
