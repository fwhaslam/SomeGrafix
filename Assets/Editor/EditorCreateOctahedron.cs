// Copyright (c) 2023 Frederick William Haslam born 1962 in the USA.
// All Rights Reserved. Not currently licensed.

using UnityEditor;
using UnityEngine;

using System.IO;
using System.IO.Compression;
using UnityEditor.SceneManagement;
using System;
using System.Collections.Generic;
using System.Security.Policy;

public class EditorCreateOctahedron {

	internal static int CREATE_COUNT = 0;

    internal static void Jot(string msg){
        //Trace.WriteLine(msg);
        //Console.WriteLine(msg);
        Debug.Log(msg);
        //MonoBehaviour.print(msg);
    }

    [MenuItem("Tools/SomeGrafix: Build Octahedron")]
    internal static void BuildOctahedron() {

        Jot("BuildOctahedron - start");

		// find selection or scene
        var owner = Selection.activeGameObject; //Get the object/
		Jot("OWNER="+owner);

		BuildOctahedronGameObject(owner.transform);

        Jot("BuildOctahedron - done");

		// SaveAsAsset( BuildOrigamoMesh(), "Origamo", false, true );
	}


	internal static void SaveAsAsset( Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh ) {

		string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
		if (string.IsNullOrEmpty(path)) return;
        
		path = FileUtil.GetProjectRelativePath(path);

		Mesh meshToSave = (makeNewInstance) ? UnityEngine.Object.Instantiate(mesh) as Mesh : mesh;
		
		if (optimizeMesh)
		     MeshUtility.Optimize(meshToSave);
        
		AssetDatabase.CreateAsset(meshToSave, path);
		AssetDatabase.SaveAssets();

    }

	internal static GameObject BuildOctahedronGameObject( Transform owner ){

		var name = "Octahedron" + ( CREATE_COUNT==0 ? "": "-"+CREATE_COUNT );
		CREATE_COUNT++;

		var work = new GameObject( name );

		work.AddComponent<MeshFilter>().sharedMesh = BuildOctahedronMesh();
		work.AddComponent<MeshRenderer>().material = FindSimpleShaderMaterial();

		work.transform.parent = owner;
		work.transform.localPosition = Vector3.zero;
		work.transform.localScale = Vector3.one;

		return work;
	}

	internal class MeshWork {

		internal List<Vector3> points = new();

		internal List<Vector3> vertices = new();
		internal List<Vector3> normals = new();
		internal List<int> triangles = new();

		internal void AddSurface( int a, int b, int c ) {
			
			int ix = vertices.Count;

			// add specified points
			vertices.Add( points[a] );	vertices.Add( points[b] );	vertices.Add( points[c] );	

			// merge points to calculate normal for all points
			var normal = points[a] + points[b] + points[c];
			normal.Normalize();
			normals.Add( normal );	normals.Add( normal );	normals.Add( normal );

			// triangles index into vertices
			triangles.Add( ix++ );	triangles.Add( ix++ );	triangles.Add( ix++ );

		}
	}


	internal static Mesh BuildOctahedronMesh(){

		var work = new MeshWork();

		// octahedron is eight sided with triangles, points going in ordinal directions.
		work.points.Add( new Vector3( +1f, 0f, 0f ) );		// ix = 0
		work.points.Add( new Vector3( -1f, 0f, 0f ) );		// ix = 1
		work.points.Add( new Vector3( 0f, +1f, 0f ) );		// ix = 2
		work.points.Add( new Vector3( 0f, -1f, 0f ) );		// ix = 3
		work.points.Add( new Vector3( 0f, 0f, +1f ) );		// ix = 4
		work.points.Add( new Vector3( 0f, 0f, -1f ) );		// ix = 5

		// triangles
		work.AddSurface( 0, 2, 4);
		work.AddSurface( 0, 5, 2);

		work.AddSurface( 0, 4, 3);
		work.AddSurface( 0, 3, 5);

		work.AddSurface( 1, 4, 2);
		work.AddSurface( 1, 2, 5);

		work.AddSurface( 1, 3, 4);
		work.AddSurface( 1, 5, 3);

		return new Mesh(){
			vertices = work.vertices.ToArray(),
			triangles = work.triangles.ToArray(),
			normals = work.normals.ToArray()
		};

	}

	internal static Material FindSimpleShaderMaterial(){

		//var found = AssetDatabase.FindAssets("URP Simple Shader Material");
		//var path = AssetDatabase.GUIDToAssetPath( found[0] );
		//return (Material)AssetDatabase.LoadAssetAtPath( path, typeof(Material) );

		return new Material(Shader.Find("Universal Render Pipeline/Lit")){ name = "Octahedron MTL" };
	}

}