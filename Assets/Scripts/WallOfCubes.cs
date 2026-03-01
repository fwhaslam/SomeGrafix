// Copyright (c) 2025 Frederick William Haslam born 1962 in the USA.
// Licensed under "The MIT License" https://opensource.org/license/mit/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;

using UnityEngine;

using static UnityEngine.Rendering.DebugUI.Table;

public class WallOfCubes : MonoBehaviour {

    public int size = 10;
    public float MaxSpin = 1f;
    public Material material;

    // Start is called before the first frame update
    void Start() {

        //var material = LoadMaterial( "Shader Graphs_URP Tainted Ripple Shader Material" );

        for (int ix=-size;ix<=size;ix++) {

            for ( int iy=-size;iy<=size;iy++ ) {

                var work = GameObject.CreatePrimitive(PrimitiveType.Cube);
                work.transform.parent = transform;
	            work.name = "cube_x"+ix+"_y"+iy;

                if ((Mathf.Abs(ix+iy)%2)==0) {
                    var meshFilter = work.transform.GetComponent<MeshFilter>();
                    meshFilter.sharedMesh = BuildMesh( meshFilter.name );
                }

                var script = work.AddComponent<SimpleSpin>();
                script.start = new Vector3( -1,-1,-1 );
                script.spin = new Vector3( Random.Range( 0f, MaxSpin ), Random.Range( 0f, MaxSpin ), Random.Range( 0f, MaxSpin ) );

                FixMaterial( work, material );

                work.transform.localPosition = new Vector3( ix, iy, 0f );
            }
        }
    }

    internal Material LoadMaterial( string materialName ) {

        var guids = AssetDatabase.FindAssets( materialName );
        var assetPath = AssetDatabase.GUIDToAssetPath( guids[0] );
        return (Material)AssetDatabase.LoadAssetAtPath( assetPath, typeof(Material));
    }


    internal void FixMaterial( GameObject work, Material material ) {
        work.GetComponent<MeshRenderer>().material = material;
    }


    /// <summary>
    /// Build an Octogon ( instead of a cube )
    /// </summary>
    /// <returns></returns>
    internal Mesh BuildMesh( string key ) {

        var pts = new List<Vector3>();
        var tris = new List<int>();
        var norms = new List<Vector3>();
        var uvs = new List<Vector2>();

        // starting data
        var workPts = new List<Vector3>(){
            new Vector3( +1, 0, 0 ),
            new Vector3( -1, 0, 0 ),
            new Vector3( 0, +1, 0 ),
            new Vector3( 0, -1, 0 ),
            new Vector3( 0, 0, +1 ),
            new Vector3( 0, 0, -1 ),
        };
        var workUvs = new List<Vector3>(){
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 0f),
            new Vector2(0.0f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.25f, 0.5f),
            new Vector2(0.75f, 0.5f)
        };
        var workTris = new List<int>();
        workTris.AddRange( new List<int>(){ 0, 2, 4 } );
        workTris.AddRange( new List<int>(){ 0, 5, 2 } );
        workTris.AddRange( new List<int>(){ 0, 4, 3 } );
        workTris.AddRange( new List<int>(){ 0, 3, 5 } );

        workTris.AddRange( new List<int>(){ 1, 4, 2 } );
        workTris.AddRange( new List<int>(){ 1, 2, 5 } );
        workTris.AddRange( new List<int>(){ 1, 3, 4 } );
        workTris.AddRange( new List<int>(){ 1, 5, 3 } );

        // surfaces
        for (int sx=0;sx<8;sx++) {

            int px0 = sx*3 + 0;
            int px1 = px0 + 1;
            int px2 = px1 + 1;

            // index is 3 of 24
            var tx0 = workTris[ px0 ];
            var tx1 = workTris[ px1 ];
            var tx2 = workTris[ px2 ];

            // index is 3 of 6
            pts.Add( workPts[tx0] );
            pts.Add( workPts[tx1] );
            pts.Add( workPts[tx2] );

            tris.Add( px0 );
            tris.Add( px1 );
            tris.Add( px2 );

            var norm = workPts[tx0] + workPts[tx1] + workPts[tx2];
            norms.Add( norm );
            norms.Add( norm );
            norms.Add( norm );

            // index is 3 of 6
            uvs.Add( workUvs[ tx0 ] );
            uvs.Add( workUvs[ tx1 ] );
            uvs.Add( workUvs[ tx2 ] );
        }

        // cleanup
        return new Mesh(){ 
            name = key,
            vertices = pts.ToArray(),
            uv = uvs.ToArray(),
            normals = norms.ToArray(),
            triangles = tris.ToArray(),
        };
    }
   

}
