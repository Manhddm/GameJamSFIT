using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private Material _material;
    [SerializeField] private float _parallaxEffect = 0.01f;
    [SerializeField] private Player _player;
    private float _offset;
    // Start is called before the first frame update
    void Start()
    {
        _material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ParallaxScroll();
    }

    private void ParallaxScroll()
    {
        _offset += _parallaxEffect * Time.fixedDeltaTime * 5f;
        _material.SetTextureOffset("_MainTex", Vector2.right * _offset);
        
    }
}
