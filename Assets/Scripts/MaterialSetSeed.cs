using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// Initialize the material so that it's random values are different from other instances.
/// 
/// </summary>
public class MaterialSetSeed : MonoBehaviour {

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

}
