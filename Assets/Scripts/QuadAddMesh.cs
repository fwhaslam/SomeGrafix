using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Rebuild the mesh of a quad object.
/// </summary>
public class QuadAddMesh : MonoBehaviour {

    internal Vector2 startScale;

    public int Rows = 4;
    public int Cols = 4;

    // Start is called before the first frame update
    void Start() {
        
        startScale = transform.localScale;
        
        var meshFilter = transform.GetComponent<MeshFilter>();

        var blc = meshFilter.sharedMesh.vertices[0];
        var urc = meshFilter.sharedMesh.vertices[3];
//print("BLC="+blc);
//print("URC="+urc);

        meshFilter.sharedMesh = BuildMesh( blc, urc, meshFilter.name );

    }

    /// <summary>
    /// Assume the quad is 1x1
    /// </summary>
    /// <returns></returns>
    internal Mesh BuildMesh( Vector3 blc, Vector3 urc, string key ) {

        var dx = (urc.x-blc.x) / Cols;
        var dy = (urc.y-blc.y) / Rows;

        // points
        var vertices = new List<Vector3>();
        //var normals = new List<Vector3>();

        for ( int ix=0; ix<=Cols; ix++ ) {
            for ( int iy=0; iy<=Rows; iy++) {
                vertices.Add( new Vector3(  dx * ix + blc.x, dy * iy + blc.y, 0f ) );
                //normals.Add( Vector3.up );
            }
        }

        // triangles
        var triangles = new List<int>();

        for ( int ix=0; ix<Cols; ix++) {

            for ( int iy=0; iy<Rows; iy++) {

                var p1 = ix * (1+Rows) + iy;
                var p2 = p1 + 1;
                var p3 = p2 + Rows;
                var p4 = p3 + 1;

                // first triangle
                triangles.Add( p1 );
                triangles.Add( p2 );
                triangles.Add( p3 );

                // second triangle
                triangles.Add( p3 );
                triangles.Add( p2 );
                triangles.Add( p4 );

            }

        }

        // cleanup
//print("Vertices = " + string.Join( ",", vertices) );
//print("Triangles = " + string.Join( ",", triangles) );

        return new Mesh(){ 
            name = key,
            vertices = vertices.ToArray(),
            //normals = normals.ToArray(),
            triangles = triangles.ToArray()
        };
    }
}
