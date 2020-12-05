using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RatKing {

	public class TextureProcessorTool : EditorWindow, ITextureProcessor {
		[SerializeField] Texture2D sourceTexture = null;
		[SerializeField] float brightness = 0f;
		[SerializeField] float contrast = 0f;
		[SerializeField] float gamma = 1f;
		[SerializeField] float saturation = 0f;
		[SerializeField] float red = 0f;
		[SerializeField] float green = 0f;
		[SerializeField] float blue = 0f;
		[SerializeField] int width = 0;
		[SerializeField] int height = 0;
		[SerializeField] TextureProcessor.ResizeMethodType resizeMethod = TextureProcessor.ResizeMethodType.Bilinear;
		//
		Texture2D result;
		Vector2 scrollPos;

		
		public Texture2D SourceTexture => sourceTexture;
		public float Brightness => brightness;
		public float Contrast => contrast;
		public float Gamma => gamma;
		public float Saturation => saturation;
		public float Red => red;
		public float Green => green;
		public float Blue => blue;
		public int Width => width;
		public int Height => height;
		public TextureProcessor.ResizeMethodType ResizeMethod => resizeMethod;

		//

		[MenuItem("Tools/Texture Processor Tool")]
		static void Init() {
			var w = EditorWindow.GetWindow(typeof(TextureProcessorTool));
			w.titleContent.text = "Texture Processor Tool";
		}
        
		void OnGUI() {
			EditorGUIUtility.labelWidth = 100f;
			var color = GUI.color;

			var mayGenerate = true;

			scrollPos = GUILayout.BeginScrollView(scrollPos);

			//if (sourceTexture != null) {
			//	GUILayout.Space(3f);
			//	var rect = GUILayoutUtility.GetAspectRect(sourceTexture.height / (float)sourceTexture.width);
			//	GUI.DrawTexture(rect, sourceTexture);
			//}
			if (sourceTexture != null && !sourceTexture.isReadable) {
				mayGenerate = false;
				GUI.color = Color.red;
				GUILayout.Label("Texture is not readable!");
				GUI.color = color;
				if (GUILayout.Button("Make Source Texture Readable", GUILayout.Height(35f))) {
					MakeTextureReadable(sourceTexture);
				}
			}

			var newSourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), true);
			if (newSourceTexture != sourceTexture) {
				result = null;
				sourceTexture = newSourceTexture;
			}
			if (sourceTexture != null) {
				var w = sourceTexture.width;
				var h = sourceTexture.height;
				GUI.color = Color.yellow;
				EditorGUILayout.LabelField("Source Size: " + w + "x" + h);
				GUI.color = color;
			}

			GUILayout.BeginHorizontal();
			brightness = EditorGUILayout.Slider("Brightness", brightness, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { brightness = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			contrast = EditorGUILayout.Slider("Contrast", contrast, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { contrast = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			gamma = EditorGUILayout.Slider("Gamma", gamma, 0.01f, 7.99f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { gamma = 1f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			saturation = EditorGUILayout.Slider("Saturation", saturation, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { saturation = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			red = EditorGUILayout.Slider("Red", red, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { red = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			green = EditorGUILayout.Slider("Green", green, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { green = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			blue = EditorGUILayout.Slider("Blue", blue, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { blue = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			width = EditorGUILayout.IntField("Width", width);
			if (GUILayout.Button("C", GUILayout.Width(20f))) {
				if (sourceTexture != null) { width = (int)(height * (sourceTexture.width / (float)sourceTexture.height)); }
			}
			if (GUILayout.Button("R", GUILayout.Width(20f))) { width = 0; GUI.FocusControl(null); }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			height = EditorGUILayout.IntField("Height", height);
			if (GUILayout.Button("C", GUILayout.Width(20f))) {
				if (sourceTexture != null) { height = (int)(width / (sourceTexture.width / (float)sourceTexture.height)); }
			}
			if (GUILayout.Button("R", GUILayout.Width(20f))) { height = 0; GUI.FocusControl(null); }
			GUILayout.EndHorizontal();

			resizeMethod = (TextureProcessor.ResizeMethodType)EditorGUILayout.EnumPopup("Resize Method", resizeMethod);

			GUILayout.Space(6f);
		
			if (mayGenerate && GUILayout.Button(result == null ? "GENERATE" : "UPDATE", GUILayout.Height(25f))) {

				var pathST = AssetDatabase.GetAssetPath(sourceTexture);
				var filenameST = System.IO.Path.GetFileName(pathST);
				var filenameExtensionlessST =  System.IO.Path.GetFileNameWithoutExtension(pathST);
				var folderST = pathST.Substring(0, pathST.Length - filenameST.Length - 1);
				var extensionST = System.IO.Path.GetExtension(pathST);
				var pathResult = folderST + "/GeneratedTextures/" + filenameExtensionlessST + " (Processed)" + extensionST;
				var size = TextureProcessor.GetResultSize(this);

				if (result == null) {
					if (!AssetDatabase.IsValidFolder(folderST + "/GeneratedTextures")) { AssetDatabase.CreateFolder(folderST, "GeneratedTextures"); }
					// copy the source texture to get all its properties (mipmaps etc)
					if (!AssetDatabase.CopyAsset(pathST, pathResult)) { Debug.LogError("could not copy source texture"); return; }
					AssetDatabase.SaveAssets();
					// result = AssetDatabase.LoadAssetAtPath<Texture2D>(pathResult);
				}
				else {
					AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(result), filenameExtensionlessST + " (Processed)" + extensionST);
				}
			
				result = new Texture2D(size.x, size.y, TextureFormat.ARGB32, true, false);
				result.name = filenameExtensionlessST + " (Processed by " + name + ")";
				result.alphaIsTransparency = sourceTexture.alphaIsTransparency;
				result.filterMode = sourceTexture.filterMode;
				result.anisoLevel = sourceTexture.anisoLevel;
				result.wrapMode = sourceTexture.wrapMode;

				TextureProcessor.Generate(this, result);

				var fullPathResult = Application.dataPath + "/../" + pathResult;
				switch (extensionST.ToLower()) {
					case ".tga":				System.IO.File.WriteAllBytes(fullPathResult, result.EncodeToTGA()); break;
					case ".jpg": case ".jpeg":	System.IO.File.WriteAllBytes(fullPathResult, result.EncodeToJPG(90)); break;
					case ".exr":				System.IO.File.WriteAllBytes(fullPathResult, result.EncodeToEXR()); break;
					default:					System.IO.File.WriteAllBytes(fullPathResult, result.EncodeToPNG()); break;
				}
		
				AssetDatabase.ImportAsset(pathResult, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUncompressedImport);
				result = AssetDatabase.LoadAssetAtPath<Texture2D>(pathResult);
				if (result != null) { EditorUtility.SetDirty(result); }
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				Selection.activeObject = result;
				EditorGUIUtility.PingObject(result);
			}

			GUILayout.Space(5f);
			if (result != null) {
				GUI.color = Color.green;
				EditorGUILayout.LabelField("Result Size: " + result.width + "x" + result.height);
				GUI.color = color;
				//GUILayout.Space(3f);
				//var rect = GUILayoutUtility.GetAspectRect(result.height / (float)result.width);
				//GUI.DrawTexture(rect, result);
			}

			if (result != null && GUILayout.Button("DELETE")) {
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(result));
				result = null;
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			GUILayout.EndScrollView();
		}

		//

		public static void MakeTextureReadable(Texture2D texture) {
			var makeReadable = TextureProcessorImporter.makeReadable = AssetDatabase.GetAssetPath(texture);
			EditorUtility.SetDirty(texture);
			AssetDatabase.ImportAsset(makeReadable, ImportAssetOptions.ForceUpdate);
		}
	}

}
