// Copyright (c) 2025 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

using UnityEngine;

public class SimplePulse : MonoBehaviour {

    internal readonly float RADIANS_PER_CIRCLE = Mathf.PI * 2f;
    internal readonly float SECONDS_PER_MINUTE = 60f;

    public float BeatsPerMinute = 60f;
    public float StartSize = 0.7f;
    public float EndSize = 1.3f;

    // sine wave, if bounds is true then negatives get flipped to positives.
    public bool Bounce = false;

    internal Vector3 startScale = Vector3.one;

    // Start is called before the first frame update
    public void Start() {

        startScale = transform.localScale;
        
    }

    // Update is called once per frame
    public void Update() {

        var minutes = Time.time / SECONDS_PER_MINUTE;
        var tempo = minutes * BeatsPerMinute;
        var cycle = tempo * RADIANS_PER_CIRCLE;
        var step = ( Bounce ? 
            Mathf.Abs( Mathf.Sin( cycle / 2f ) ) : 
            ( Mathf.Sin( cycle ) + 1f ) / 2f );
        var pulse = Mathf.Lerp( StartSize, EndSize, step );
//print("Instance="+transform.GetInstanceID()+"  Minutes="+minutes+"  Cycle="+cycle+"  Step="+step+"  Pulse = "+pulse+"  startScale="+startScale );


        transform.localScale = startScale * pulse;

    }
}
