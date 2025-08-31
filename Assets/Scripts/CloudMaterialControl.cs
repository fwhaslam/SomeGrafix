using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Initialize the material so that it's random values are different from other instances.
/// 
/// Update so it faces the camera at all times.
/// 
/// </summary>
public class CloudMaterialControl : MonoBehaviour {

    static int SEED_COUNT = 0;

    public Material material;
    public string seedReference;


    // Start is called before the first frame update
    void Start() {
        
        var newMaterial = Instantiate( material );
        //newMaterial.SetFloat( seedReference, ++SEED_COUNT );

        var renderer = GetComponent<MeshRenderer>();
        renderer.materials[0] = newMaterial;

        renderer.materials[0].SetFloat( seedReference, ++SEED_COUNT );
    }

    // lookup billboard asset and billboard renderer
    void Update() {

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);

        //var camerarelative = transform.localposition - camera.main.transform.localposition;
        //transform.lookat(camerarelative);

    }
}
