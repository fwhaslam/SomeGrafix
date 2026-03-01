// Copyright (c) 2025 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Splash {

	/// <summary>
	/// WIP
	/// 1) Script is attached to the template of the object which needs to 'splash'.  
	/// 2) Places object instance in center of screen.
	/// 3) Grows from zero to Screen size over 'SplashTime' seconds.
	/// 4) Object is 100% opaque until 'StartFade' time has passed, then object becomes transparent over the remaining time.
	/// 5) Then it is removed so it does not catch clicks.
	/// </summary>
	public class RainbowSplash : MonoBehaviour {

		public static UnityEvent startRainbowSplash = new UnityEvent();

		public GameObject RainbowTemplate;

		public float SplashTime = 5f;    // seconds over with the 'splash' occurs
		public float StartFade = 1f;     // seconds when 'fade' begins

		internal float startTime,finishTime;
		internal GameObject SplashGO;
		internal Transform SplashTFM;


		// Start is called before the first frame update
		void Start() {
			startTime = 0f;
			finishTime = 0f;

			var work = RainbowTemplate.transform.GetChild(0);
			SplashGO = Instantiate( work.gameObject );
			SplashTFM = SplashGO.transform;
		}

		public void SplashButton(){
			startRainbowSplash.Invoke();
		}


		public void OnEnable() {
			startRainbowSplash.AddListener( StartSplashAnimation );
		}

		public void OnDisable() {
			startRainbowSplash.RemoveListener( StartSplashAnimation );
		}

		public void StartSplashAnimation(){

			startTime = Time.time;
			finishTime = startTime + SplashTime;

			SplashTFM.localScale = Vector3.zero;
			SplashTFM.localPosition = Vector3.zero;

		}

		// Update is called once per frame
		void Update() {

			var countdown = finishTime - Time.time;
			if (countdown<0) {
				SplashTFM.localScale = Vector3.zero;
				return;
			}

			var scaleUp = ( (SplashTime - countdown) / SplashTime );
			var easeUp = EasingsTool.EaseInSine(scaleUp);
			var objectSize = easeUp * Mathf.Max( Screen.width, Screen.height );

			var scaleDown = (countdown-StartFade)/(SplashTime-StartFade);
			var targetFade = ( countdown<StartFade ? 1f : scaleDown);

			SplashTFM.localScale = Vector3.one * objectSize;

		}

	}

}