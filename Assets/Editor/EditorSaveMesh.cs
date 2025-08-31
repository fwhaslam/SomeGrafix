using UnityEditor;

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MeshSaverEditor {

	//[MenuItem("CONTEXT/MeshFilter/Save Mesh...")]
	//public static void SaveMeshInPlace (MenuCommand menuCommand) {
	//	MeshFilter mf = menuCommand.context as MeshFilter;
	//	Mesh m = mf.sharedMesh;
	//	SaveMesh(m, m.name, false, true);
	//}

	//[MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
	//public static void SaveMeshNewInstanceItem (MenuCommand menuCommand) {
	//	MeshFilter mf = menuCommand.context as MeshFilter;
	//	Mesh m = mf.sharedMesh;
	//	SaveMesh(m, m.name, true, true);
	//}

	[MenuItem("CONTEXT/MeshFilter/Save Mesh As New Instance...")]
	public static void SaveMeshNewInstanceItem (MenuCommand menuCommand) {
		//MeshFilter mf = menuCommand.context as MeshFilter;
		//Mesh m = mf.sharedMesh;
		SaveMesh( BuildOrigamoMesh(), "Origamo", true, false);
	}

	public static void SaveMesh (Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh) {
		string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
		if (string.IsNullOrEmpty(path)) return;
        
		path = FileUtil.GetProjectRelativePath(path);

		Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;
		
		if (optimizeMesh)
		     MeshUtility.Optimize(meshToSave);
        
		AssetDatabase.CreateAsset(meshToSave, path);
		AssetDatabase.SaveAssets();
	}

	public static Mesh BuildOrigamoMesh(){

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

		triangles.Add(1);	triangles.Add(2);	triangles.Add(4);	
		triangles.Add(3);	triangles.Add(2);	triangles.Add(4);	

		return new Mesh(){
			vertices = vertices.ToArray(),
			triangles = triangles.ToArray()
		};

	}
	
}