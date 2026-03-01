// Copyright (c) 2025 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Splash {

    public class PlatesSplash : MonoBehaviour {

        internal static readonly float RADIANS = Mathf.PI * 2f;
 
        public GameObject PlateTemplate;
        public float SplashTime = 15f;
        public int platesPerLayer = 6;

        internal GameObject grid;
        internal float startSplash,endSplash;

        // Start is called before the first frame update
        void Start() {
            grid = new GameObject("PlatesGrid");
            grid.transform.localScale = Vector3.one;
            grid.transform.localPosition = Vector3.zero;
            grid.SetActive(false);

            startSplash = endSplash = 0f;
        }

        public void SplashButton(){
            StartPlateSplashAnimation();
        }

        public void StartPlateSplashAnimation(){
            startSplash = Time.time;
            endSplash = startSplash + SplashTime;
            grid.SetActive(true);
        }

        internal void EndPlateSplashAnimation(){
            grid.SetActive(false);
        }

        // Update is called once per frame
        void Update() {

            if (!grid.activeSelf) return;

            var countdown = endSplash - Time.time;
            if (countdown<0f) {
                EndPlateSplashAnimation();
                return;
            }
        
            // add children to grid ?
            var gridTFM = grid.transform;
            var childCount = gridTFM.childCount;
            if (childCount>=platesPerLayer) return;

            BuildPlates();
        }

        internal void BuildPlates(){

            // add one central plate
            {
                var work = Instantiate(PlateTemplate, grid.transform);
                var workTFM = work.transform;
                workTFM.name = "Petal-" + 0;
                workTFM.localScale = Vector3.one * 20f;
                workTFM.localPosition = new Vector3(0f, 0f, 5f);
            }

            // add one layer of plates
            for ( int ix=0;ix<platesPerLayer;ix++ ) {

                var radians = RADIANS * ix / platesPerLayer;
                var degrees = 360f * ix / platesPerLayer;

                var work = Instantiate( PlateTemplate, grid.transform );
                var workTFM = work.transform;
                workTFM.name = "Petal-"+ix;
                workTFM.localScale = Vector3.one * 30f;
                workTFM.SetLocalPositionAndRotation(
                    // offset in circle around vector.zero
                    15f * new Vector3(-Mathf.Sin(radians), Mathf.Cos(radians), 0f),
                    // rotate 45 on y axis first, then rotate degrees 360/platesPerLayer along z axis
                    workTFM.localRotation = Quaternion.Euler(0f, 0f, degrees) * Quaternion.Euler(0f, 45f, 0f)
                );

                //// offset in circle around vector.zero
                //workTFM.localPosition = 15f * new Vector3( -Mathf.Sin( radians ), Mathf.Cos( radians ), 0f );

                //// rotate 45 on y axis first, then rotate degrees 360/platesPerLayer along z axis
                //workTFM.localRotation = Quaternion.Euler( 0f, 0f, degrees ) * Quaternion.Euler( 0f, 45f, 0f );
 
            }

        }
    }
}