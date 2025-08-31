// Copyright (c) 2023 Frederick William Haslam born 1962 in the USA.
// All Rights Reserved. Not currently licensed.

using UnityEngine;

/// <summary>
/// Three Dimensional rotation, including a default starting position.
/// </summary>
public class SimpleSpin : MonoBehaviour {

    [Header("Degrees Per Second")]
    public Vector3 spin = Vector3.zero;
    [Header("Degrees / Negative for random")]
    public Vector3 start = Vector3.zero;

    internal void Start() { 
        Vector3 baseSpin = start;
        if (start.x<0) baseSpin.x = Random.Range( 0f, 359.99f );
        if (start.y<0) baseSpin.y = Random.Range( 0f, 359.99f );
        if (start.z<0) baseSpin.z = Random.Range( 0f, 359.99f );
        transform.Rotate( baseSpin );
    }

    // Update is called once per frame
    internal void Update() {
      transform.Rotate( spin * Time.deltaTime );
    }
}
