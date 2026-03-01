// Copyright (c) 2026 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach to Main Camera.
/// </summary>
public class TrackFrameRate : MonoBehaviour {

    static readonly int TRACK_SECONDS = 300;
    static internal List<float> FramesOverTime;

    internal int LastFrameCount;
    internal float LastFrameTime;

    // Start is called before the first frame update
    void Start() {
        
        Application.targetFrameRate = 120;
        
        LastFrameCount = 0;
        LastFrameTime = Time.time;
        FramesOverTime = new List<float>( TRACK_SECONDS );

        StartCoroutine(MeasureFramesPerSecond());
    }


    IEnumerator MeasureFramesPerSecond() {
        while (true) {

            yield return new WaitForSeconds(1);

            var count = Time.frameCount;
            var time = Time.time;

            var frameRate = (count-LastFrameCount) / (time-LastFrameTime);
            FramesOverTime.Remove( TRACK_SECONDS-1 );
            FramesOverTime.Insert( 0, frameRate );

            LastFrameCount = count;
            LastFrameTime = time;

            Debug.LogFormat("Fps {0}", frameRate );
        }
    }

    static public float CurrentFrameRate(){
        return FramesOverTime[0];
    }

    static public List<float> FrameRateHistory(){
        return FramesOverTime;
    }
}
