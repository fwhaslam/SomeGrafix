// Copyright (c) 2025 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BoidEnum {

    Bouncer, Follower
}

public class Boid : MonoBehaviour {

    internal BoidEnum style;
    internal Boid target;
    internal Vector3 last, next, velocity;
    internal float lastTime, nextTime;

    public void CreateBouncer( float now, Vector3 start, Vector3 heading ) {
        style = BoidEnum.Bouncer;
        last = start;
        velocity = heading;
        next = start + heading;
        lastTime = now;
        nextTime = now +  Time.fixedDeltaTime;
    }

    public void CreateFollower( float now, Vector3 start, Vector3 heading, Boid who ) {

        style = BoidEnum.Follower;
        target = who;

        last = start;
        velocity = heading;
        next = start + heading;
        lastTime = now;
        nextTime = now +  Time.fixedDeltaTime;
    }

    /// <summary>
    /// Using last and next, interpolocate display location.
    /// </summary>
	public void Update() {
		
        // facing is towards next point
        transform.localRotation = Quaternion.LookRotation( next - last );

        // location is between points
        transform.localPosition = Vector3.Lerp( last, next, Time.time - lastTime );

	}

    public void NextPoint( Vector3 where, Vector3 heading, float when ) {

        lastTime = nextTime;
        last = next;

        nextTime = when;
        next = where;

        velocity = heading;
        
    }
}

/// <summary>
/// 3D Rectangular region where Boids fly using various algorithms.
/// </summary>
public class BoidZone : MonoBehaviour {


    public float width, height, depth;
    public float speed,trailing;

    public GameObject BoidTemplate;

    internal List<Boid> boidScripts;


    // Start is called before the first frame update
    public void Start() {
        
        boidScripts = new List<Boid>();
        AddBouncer();
        for (int ix=0;ix<8;ix++) {
            AddFollower( boidScripts[ix] );
        }
    }


    internal void AddBouncer(){

        var start = RandomLoc();
        var end = RandomLoc();

        var direction = end - start;
        direction.Normalize();
        var velocity = speed * direction;

        var boid = Instantiate( BoidTemplate, transform );
        var script = boid.AddComponent<Boid>();
        script.CreateBouncer( Time.time, start, velocity );

        boidScripts.Add( script );
    }

    internal void AddFollower( Boid target ){

        var start = RandomLoc();
        var end = target.last;

        var direction = end - start;
        direction.Normalize();
        var velocity = speed * direction;

        var boid = Instantiate( BoidTemplate, transform );
        var script = boid.AddComponent<Boid>();
        script.CreateFollower( Time.time, start, velocity, target );

        boidScripts.Add( script );
    }


    internal Vector3 RandomLoc(){
        return new Vector3(  Random.Range( -width, width ), Random.Range( -height, height ), Random.Range( -depth, depth ) );
    }

    /// <summary>
    /// Algorithm calculates 'next' location for Boid.
    /// </summary>
	public void FixedUpdate() {

        foreach ( var script in boidScripts ) {

            switch ( script.style ) {
                case BoidEnum.Bouncer: UpdateBouncer( script ); break;
                case BoidEnum.Follower: UpdateFollower( script ); break;
            }
        }
	}

    internal void UpdateBouncer( Boid script ) {

        var velocity = script.velocity;
        var next = script.next + velocity;

        // 'bounce' off of zone edges
        if ( next.x < -width ) { next.x = -2*width - next.x; velocity.x = -velocity.x; }
        if ( next.x > width ) { next.x = 2*width - next.x; velocity.x = -velocity.x; }

        if ( next.y < -height ) { next.y = -2*height - next.y; velocity.y = -velocity.y; }
        if ( next.y > height ) { next.y = 2*height - next.y; velocity.y = -velocity.y; }

        if ( next.z < -depth ) { next.z = -2*depth - next.z; velocity.z = -velocity.z; }
        if ( next.z > depth ) { next.z = 2*depth - next.z; velocity.z = -velocity.z; }

                
        script.NextPoint( next, velocity, Time.time );

    }

    internal void UpdateFollower( Boid script ) {

        var from = script.last;
        var dest = script.target.last - trailing * script.target.velocity;

        var heading = dest - from;
        heading.Normalize();
        var velocity = heading * speed;

        script.NextPoint( from + velocity, velocity, Time.time );
    }
}
