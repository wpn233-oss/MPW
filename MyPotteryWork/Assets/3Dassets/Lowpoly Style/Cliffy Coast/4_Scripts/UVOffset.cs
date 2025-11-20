using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshRenderer))]
public class UVOffset : MonoBehaviour {
    [Range(0,3f)]    public float ScrollSpeed = 0.5f;
    public bool ScrollY = true;
    private MeshRenderer _renderer;

    void Awake() {
        _renderer = GetComponent<MeshRenderer>();
    }

    void Update() {
        float offset = Time.time * ScrollSpeed;
        _renderer.material.SetTextureOffset("_MainTex", ScrollY ?   new Vector2(offset, 0) : new Vector2(0,offset));
    }
}
