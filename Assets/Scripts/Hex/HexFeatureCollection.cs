using UnityEngine;

[System.Serializable]
public struct HexFeatureCollection {

	public Mesh[] meshes;

	public Mesh Pick (float choice) {
		return meshes[(int)(choice * meshes.Length)];
	}
}