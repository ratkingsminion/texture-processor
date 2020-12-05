using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RatKing {

	public class TextureProcessorImporter : AssetPostprocessor {
		public static string makeReadable { private get; set; } = null;
		static List<TextureProcessor> observed = null;

		// http://answers.unity3d.com/questions/382545/changing-texture-import-settings-during-runtime.html
		void OnPreprocessTexture() {
			if (makeReadable != null && assetPath == makeReadable) {
				var importer = assetImporter as TextureImporter;
				importer.isReadable = true;
				makeReadable = null;
				var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
				if (asset != null) { EditorUtility.SetDirty(asset); }
				//else { importer.textureType = TextureImporterType.Default; }
			}
		}

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
			if (observed == null) { 
				var tpo = Resources.LoadAll<TextureProcessorObserved>("");
				if (tpo == null || tpo.Length == 0) { return; }
				observed = tpo[0].observed;
			}
			if (observed.Count == 0) { return; }

			foreach (string str in importedAssets) {
				//Debug.Log("Reimported Asset: " + str);
				foreach (var o in observed) {
					var path = AssetDatabase.GetAssetPath(o.SourceTexture);
					//Debug.Log(path + " " + (path == str));
					if (path == str) { o.Generate(null); }
				}
			}

			//foreach (string str in deletedAssets) {
			//	Debug.Log("Deleted Asset: " + str);
			//}

			//for (int i = 0; i < movedAssets.Length; i++) {
			//	Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
			//}
		}

	}

}