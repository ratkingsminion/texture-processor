using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RatKing {

	[CustomEditor(typeof(TextureProcessor))]
	[CanEditMultipleObjects]
	public class TextureProcessorEditor : Editor {

		SerializedProperty saveAsExtraFile;
		SerializedProperty sourceTexture;
		SerializedProperty result;
		SerializedProperty brightness;
		SerializedProperty contrast;
		SerializedProperty gamma;
		SerializedProperty saturation;
		SerializedProperty red;
		SerializedProperty green;
		SerializedProperty blue;
		SerializedProperty width;
		SerializedProperty height;
		SerializedProperty resizeMethod;

		enum Observe { Undecided, Invalid, Observe, DontObserve }
		Observe observe;

		//

		void OnEnable() {
			saveAsExtraFile = serializedObject.FindProperty("saveAsExtraFile");
			sourceTexture = serializedObject.FindProperty("sourceTexture");
			result = serializedObject.FindProperty("result");
			brightness = serializedObject.FindProperty("brightness");
			contrast = serializedObject.FindProperty("contrast");
			gamma = serializedObject.FindProperty("gamma");
			saturation = serializedObject.FindProperty("saturation");
			red = serializedObject.FindProperty("red");
			green = serializedObject.FindProperty("green");
			blue = serializedObject.FindProperty("blue");
			width = serializedObject.FindProperty("width");
			height = serializedObject.FindProperty("height");
			resizeMethod = serializedObject.FindProperty("resizeMethod");
		}

		public override void OnInspectorGUI() {

			var mayGenerate = true;

			if (!serializedObject.isEditingMultipleObjects) {
				var tp = serializedObject.targetObject as TextureProcessor;
				if (tp.SourceTexture != null) {
					GUILayout.Space(3f);
					var rect = GUILayoutUtility.GetAspectRect(tp.SourceTexture.height / (float)tp.SourceTexture.width);
					GUI.DrawTexture(rect, tp.SourceTexture);
				}
				if (tp.SourceTexture != null && !tp.SourceTexture.isReadable) {
					mayGenerate = false;
					var color = GUI.color;
					GUI.color = Color.red;
					GUILayout.Label("Texture is not readable!");
					GUI.color = color;
					if (GUILayout.Button("Make Source Texture Readable", GUILayout.Height(35f))) {
						TextureProcessorTool.MakeTextureReadable(tp.SourceTexture);
					}
				}
			}

			EditorGUIUtility.labelWidth = 100f;

			EditorGUILayout.ObjectField(sourceTexture, new GUIContent("Source Texture"));
			if (!serializedObject.isEditingMultipleObjects) {
				var tp = serializedObject.targetObject as TextureProcessor;
				if (tp.SourceTexture != null) {

					if (observe == Observe.Undecided) {
						var tpo = Resources.LoadAll<TextureProcessorObserved>("");
						if (tpo == null) { observe = Observe.Invalid; }
						else if (tpo.Length != 0 && tpo[0].observed.Count != 0) { observe = tpo[0].observed.Contains(tp) ? Observe.Observe : Observe.DontObserve; }
						else { observe = Observe.DontObserve; }
					}
					else if (observe != Observe.Invalid) {
						var oldObserve = observe == Observe.Observe;
						var newObserve = GUILayout.Toggle(oldObserve, "Observe Changes Of This Texture");
						if (newObserve != oldObserve) {
							var tpo = Resources.LoadAll<TextureProcessorObserved>("");
							if (newObserve) {
								observe = Observe.Observe;
								tpo[0].Add(tp);
							}
							else {
								observe = Observe.DontObserve;
								tpo[0].Remove(tp);
							}
						}
					}

					var w = tp.SourceTexture.width;
					var h = tp.SourceTexture.height;
					var color = GUI.color;
					GUI.color = Color.yellow;
					EditorGUILayout.LabelField("Source Size: " + w + "x" + h);
					GUI.color = color;
				}
				else {
					observe = Observe.Undecided;
				}
			}

			GUILayout.BeginHorizontal();
			EditorGUILayout.Slider(brightness, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { brightness.floatValue = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Slider(contrast, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { contrast.floatValue = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Slider(gamma, 0.01f, 7.99f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { gamma.floatValue = 1f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Slider(saturation, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { saturation.floatValue = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Slider(red, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { red.floatValue = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Slider(green, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { green.floatValue = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Slider(blue, -1f, 1f);
			if (GUILayout.Button("R", GUILayout.Width(20f))) { blue.floatValue = 0f; }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(width);
			if (GUILayout.Button("C", GUILayout.Width(20f))) {
				var st = sourceTexture.objectReferenceValue as Texture2D;
				if (st != null) { width.intValue = (int)(height.intValue * (st.width / (float)st.height)); }
			}
			if (GUILayout.Button("R", GUILayout.Width(20f))) { width.intValue = 0; GUI.FocusControl(null); }
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(height);
			if (GUILayout.Button("C", GUILayout.Width(20f))) {
				var st = sourceTexture.objectReferenceValue as Texture2D;
				if (st != null) { height.intValue = (int)(width.intValue / (st.width / (float)st.height)); }
			}
			if (GUILayout.Button("R", GUILayout.Width(20f))) { height.intValue = 0; GUI.FocusControl(null); }
			GUILayout.EndHorizontal();

			EditorGUILayout.PropertyField(resizeMethod);

			EditorGUILayout.PropertyField(saveAsExtraFile, new GUIContent("As Extra File"), false);

			GUILayout.Space(6f);
		
			if (mayGenerate && GUILayout.Button(result.objectReferenceValue == null ? "GENERATE" : "UPDATE", GUILayout.Height(25f))) {
				foreach (var to in serializedObject.targetObjects) {
					var tp = to as TextureProcessor;
					tp.Generate(result => {
						if (serializedObject.isEditingMultipleObjects) { serializedObject.SetIsDifferentCacheDirty(); }
						else { this.result.objectReferenceValue = result; }
					});
				}
				serializedObject.ApplyModifiedProperties();
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			if (!serializedObject.isEditingMultipleObjects) {
				GUILayout.Space(5f);
				var tp = serializedObject.targetObject as TextureProcessor;
				if (tp.Result != null) {
					var color = GUI.color;
					GUI.color = Color.green;
					EditorGUILayout.LabelField("Result Size: " + tp.Result.width + "x" + tp.Result.height);
					GUI.color = color;
					GUILayout.Space(3f);
					var rect = GUILayoutUtility.GetAspectRect(tp.Result.height / (float)tp.Result.width);
					GUI.DrawTexture(rect, tp.Result);
				}
			}
			else {
				GUILayout.Label("Select only ONE TextureProcessor asset to see the result.");
			}

			if (result.objectReferenceValue != null && GUILayout.Button("DELETE")) {
				foreach (var to in serializedObject.targetObjects) {
					var tp = to as TextureProcessor;
					tp.DeleteResultPlusAsset();
				}
				result.objectReferenceValue = null;
			}

			serializedObject.ApplyModifiedProperties();
		}
	}

}
