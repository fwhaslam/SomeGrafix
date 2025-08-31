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

public class EditorCreateOrigamo {

	internal static int CREATE_COUNT = 0;

    internal static void Jot(string msg){
        //Trace.WriteLine(msg);
        //Console.WriteLine(msg);
        Debug.Log(msg);
        //MonoBehaviour.print(msg);
    }

    [MenuItem("Tools/SomeGrafix: Build Origamo")]
    internal static void BuildOrigamo() {

        Jot("BuildOrigamo - start");

		// find selection or scene
        var owner = Selection.activeGameObject; //Get the object/
		Jot("OWNER="+owner);

		BuildOrigamoGameObject(owner.transform);

        Jot("BuildOrigamo - done");

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

	internal static GameObject BuildOrigamoGameObject( Transform owner ){

		var name = "Origamo" + ( CREATE_COUNT==0 ? "": "-"+CREATE_COUNT );
		CREATE_COUNT++;

		var work = new GameObject( name );

		work.AddComponent<MeshFilter>().sharedMesh = BuildOrigamoMesh();
		work.AddComponent<MeshRenderer>().material = FindSimpleShaderMaterial();

		work.transform.parent = owner;
		work.transform.localPosition = Vector3.zero;
		work.transform.localScale = Vector3.one;

		return work;
	}


	internal static Mesh BuildOrigamoMesh(){

		var vertices = new List<Vector3>();
		var triangles = new List<int>();

		// 'origamo' object is a stick outline, using 4 counter corners of a cube.
		var c1 = new Vector3( -0.5f, -0.5f, -0.5f );
		var c2 = new Vector3( +0.5f, +0.5f, -0.5f );
		var c3 = new Vector3( +0.5f, -0.5f, +0.5f );
		var c4 = new Vector3( -0.5f, +0.5f, +0.5f );

		// FIRST DRAFT, build as two triangles

		vertices.Add( c1 );
		vertices.Add( c2 );
		vertices.Add( c3 );
		vertices.Add( c4 );

		triangles.Add(1);	triangles.Add(0);	triangles.Add(2);	
		triangles.Add(3);	triangles.Add(0);	triangles.Add(2);	

		return new Mesh(){
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray()
		};

	}

	internal static Material FindSimpleShaderMaterial(){

		var found = AssetDatabase.FindAssets("URP Simple Shader Material");
		var path = AssetDatabase.GUIDToAssetPath( found[0] );
		return (Material)AssetDatabase.LoadAssetAtPath( path, typeof(Material) );
	}

}