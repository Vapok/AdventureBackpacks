using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System;

[InitializeOnLoad]
public class Editor : MonoBehaviour
{
  static void OnSceneLoad(string path)
  {
    Debug.Log($"Opening scene {path}");
    switch (Path.GetFileName(path))
    {
      case "main.unity":
        WorldGenerator.Initialize(World.GetDevWorld());
        break;
      case "start.unity":
      default:
        WorldGenerator.Initialize(World.GetMenuWorld());
        break;
    }
  }

  static Editor()
  {
    /* Doesn't work, gives a blank scene
    Scene scene = EditorSceneManager.GetActiveScene();
    Debug.Log($"Assuming {scene.name} is loaded");
    OnSceneLoad(scene.path);
    */
    // No callback fires when the unity editor is loaded (last scene is opened with the editor)
    OnSceneLoad("");

    EditorSceneManager.sceneOpening += (path, mode) => {
      OnSceneLoad(path);
    };
  }
}
