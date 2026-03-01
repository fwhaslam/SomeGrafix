// Copyright (c) 2025 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 
/// Update so it faces the camera at all times.
/// 
/// </summary>
public class QuadFaceCamera : MonoBehaviour {


	// lookup billboard asset and billboard renderer
	void Update() {

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);

    }
}
