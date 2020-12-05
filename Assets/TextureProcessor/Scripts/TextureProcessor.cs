using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Threading;
using UnityEditor;
#endif

namespace RatKing {

	[CreateAssetMenu(fileName = "TEX", menuName = "Rat King/New Texture Processor")]
	public class TextureProcessor : ScriptableObject, ITextureProcessor {

		public enum ResizeMethodType { Point, Bilinear }

		[SerializeField] bool saveAsExtraFile = true;
		public bool SaveAsExtraFile => saveAsExtraFile;
#if UNITY_EDITOR
		[SerializeField] Texture2D sourceTexture = null;
		public Texture2D SourceTexture => sourceTexture;
#else
		public Texture2D SourceTexture => null;
#endif
		[SerializeField] Texture2D result;
		public Texture2D Result => result;
		//
		[SerializeField] float brightness = 0f;
		public float Brightness => brightness;
		[SerializeField] float contrast = 0f;
		public float Contrast => contrast;
		[SerializeField] float gamma = 1f;
		public float Gamma => gamma;
		[SerializeField] float saturation = 0f;
		public float Saturation => saturation;
		[SerializeField] float red = 0f;
		public float Red => red;
		[SerializeField] float green = 0f;
		public float Green => green;
		[SerializeField] float blue = 0f;
		public float Blue => blue;
		[SerializeField] int width = 0;
		public int Width => width;
		[SerializeField] int height = 0;
		public int Height => height;
		[SerializeField] ResizeMethodType resizeMethod = ResizeMethodType.Bilinear;
		public ResizeMethodType ResizeMethod => resizeMethod;

		//

		public static Vector2Int GetResultSize(ITextureProcessor tp) {
			if (tp.SourceTexture == null) { return Vector2Int.zero; }
			if ((tp.Width > 0 && tp.Width != tp.SourceTexture.width) || (tp.Height > 0 && tp.Height != tp.SourceTexture.height)) {
				var w = tp.Width > 0 ? tp.Width : tp.SourceTexture.width;
				var h = tp.Height > 0 ? tp.Height : tp.SourceTexture.height;
				return new Vector2Int(w, h);
			}
			return new Vector2Int(tp.SourceTexture.width, tp.SourceTexture.height);
		}

#if UNITY_EDITOR
		// https://answers.unity.com/questions/1274030/unity-duplicate-event.html
		void OnValidate() {
			var e = Event.current;
			if (e != null && (e.type == EventType.ExecuteCommand || e.type == EventType.Used) && e.commandName == "Duplicate") {
				result = null;
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
			}
		}

		//

		public void SetResult(Texture2D result) {
			this.result = result;
		}

		public void DeleteResult() {
			if (result != null) {
				DestroyImmediate(result, true);
				result = null;
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssets();
			}
		}

		public void DeleteResultPlusAsset() {
			if (result == null) { return; }
			if (AssetDatabase.IsSubAsset(result)) {
				DeleteResult();
			}
			else {
				AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(result));
				SetResult(null);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}

		public void Generate(System.Action<Texture2D> onSave) {
			var pathTP = AssetDatabase.GetAssetPath(this);

			var filenameTP = System.IO.Path.GetFileName(pathTP);
			var folderTP = pathTP.Substring(0, pathTP.Length - filenameTP.Length - 1);
			var pathST = AssetDatabase.GetAssetPath(sourceTexture);
			var extensionST = System.IO.Path.GetExtension(pathST);
			var filenameST = System.IO.Path.GetFileName(pathST);
			var filenameExtensionlessST =  System.IO.Path.GetFileNameWithoutExtension(pathST);
			var pathResult = folderTP + "/GeneratedTextures/" + filenameExtensionlessST + " (Processed by " + name + ")" + extensionST;

			var size = TextureProcessor.GetResultSize(this);
			
			Texture2D resTex = null;
			void CreateResTex() {
				resTex = new Texture2D(size.x, size.y, TextureFormat.ARGB32, true, false);
				resTex.name = filenameExtensionlessST + " (Processed by " + name + ")";
				resTex.alphaIsTransparency = sourceTexture.alphaIsTransparency;
				resTex.filterMode = sourceTexture.filterMode;
				resTex.anisoLevel = sourceTexture.anisoLevel;
				resTex.wrapMode = sourceTexture.wrapMode;
			}

			if (result == null || (AssetDatabase.IsSubAsset(result) == saveAsExtraFile)) {
				DeleteResultPlusAsset();
				CreateResTex();

				if (saveAsExtraFile) {
					if (!AssetDatabase.IsValidFolder(folderTP + "/GeneratedTextures")) { AssetDatabase.CreateFolder(folderTP, "GeneratedTextures"); }
					if (!AssetDatabase.CopyAsset(pathST, pathResult)) { Debug.LogError("could not copy file"); return; }
					AssetDatabase.SaveAssets();
				}
				else {
					AssetDatabase.AddObjectToAsset(resTex, this);
					var newFile = "___TEMP___.asset"; // TODO hack	
					AssetDatabase.RenameAsset(pathTP, newFile); // TODO hack
					AssetDatabase.SaveAssets(); // TODO hack
					AssetDatabase.RenameAsset(pathTP.Substring(0, pathTP.Length - filenameTP.Length) + newFile, filenameTP); // TODO hack
					result = resTex;
				}
			}
			else {
				if (saveAsExtraFile) {
					CreateResTex(); // have to create anew, in case original is compressed
					pathResult = AssetDatabase.GetAssetPath(result);
					AssetDatabase.RenameAsset(pathResult, filenameExtensionlessST + " (Processed by " + name + ")" + extensionST);
				}
				else {
					resTex = result;
					if (resTex.width != size.x || resTex.height != size.y) { resTex.Resize(size.x, size.y); }
				}
			}

			Generate(this, resTex);

			if (saveAsExtraFile) {
				var fullPathResult = Application.dataPath + "/../" + pathResult;
				switch (extensionST) {
					case ".tga":				System.IO.File.WriteAllBytes(fullPathResult, resTex.EncodeToTGA()); break;
					case ".jpg": case ".jpeg":	System.IO.File.WriteAllBytes(fullPathResult, resTex.EncodeToJPG(90)); break;
					case ".exr":				System.IO.File.WriteAllBytes(fullPathResult, resTex.EncodeToEXR()); break;
					default:					System.IO.File.WriteAllBytes(fullPathResult, resTex.EncodeToPNG()); break;
				}
				AssetDatabase.ImportAsset(pathResult, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUncompressedImport);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				result = AssetDatabase.LoadAssetAtPath<Texture2D>(pathResult);
			}
			else {
				AssetDatabase.ImportAsset(pathTP, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUncompressedImport);
			}
			
			if (result != null) {
				if (onSave != null) { onSave(result); }
				EditorUtility.SetDirty(result);
			}
			EditorUtility.SetDirty(this);
		}

		//

		public static void Generate(ITextureProcessor tp, Texture2D result) {
			Color32[] pixels;
			var size = GetResultSize(tp);
			if (size.x != tp.SourceTexture.width || size.y != tp.SourceTexture.height) {
				pixels = (tp.ResizeMethod == TextureProcessor.ResizeMethodType.Point) ? Scale.Point(tp.SourceTexture, size) : Scale.Bilinear(tp.SourceTexture, size);	
			}
			else {
				pixels = tp.SourceTexture.GetPixels32();
			}
			
			// BRIGHTNESS https://www.dfstudios.co.uk/articles/programming/image-programming-algorithms/image-processing-algorithms-part-4-brightness-adjustment/
			var brightness = tp.Brightness * 255.0;
			// CONTRAST https://www.dfstudios.co.uk/articles/programming/image-programming-algorithms/image-processing-algorithms-part-5-contrast-adjustment/
			var contrast = tp.Contrast * 255.0;
			var contrastFactor = (259.0 * (contrast + 255.0)) / (255.0 * (259.0 - contrast));
			// GAMMA https://www.dfstudios.co.uk/articles/programming/image-programming-algorithms/image-processing-algorithms-part-6-gamma-correction/
			var gammaCorrection = 1.0 / (double)tp.Gamma;
			// SATURATION https://www.dfstudios.co.uk/articles/programming/image-programming-algorithms/image-processing-algorithms-part-3-greyscale-conversion/
			var saturation = (double)tp.Saturation;
			// COLOR
			var red = (double)tp.Red;
			var green = (double)tp.Green;
			var blue = (double)tp.Blue;

			double Remap(double value, double oldMin, double oldMax, double newMin, double newMax) {
				return ((newMax - newMin) * (value - oldMin) / (oldMax - oldMin)) + newMin;
			}

			for (int i = 0; i < pixels.Length; ++i) {
				var pr = (double)pixels[i].r;
				var pg = (double)pixels[i].g;
				var pb = (double)pixels[i].b;

				if (brightness != 0.0) {
					pr = pr + brightness;
					pg = pg + brightness;
					pb = pb + brightness;
				}
				if (contrast != 0.0) {
					pr = contrastFactor * (pr - 128.0) + 128.0;
					pg = contrastFactor * (pg - 128.0) + 128.0;
					pb = contrastFactor * (pb - 128.0) + 128.0;
				}
				if (gammaCorrection != 1.0) {
					pr = (255.0 * System.Math.Pow(pr / 255.0, gammaCorrection));
					pg = (255.0 * System.Math.Pow(pg / 255.0, gammaCorrection));
					pb = (255.0 * System.Math.Pow(pb / 255.0, gammaCorrection));
				}
				var gray = 0.299 * pr + 0.587 * pg + 0.144 * pb;
				if (saturation != 0.0) {
					pr = Remap(saturation, -1f, 0f, gray, pr);
					pg = Remap(saturation, -1f, 0f, gray, pg);
					pb = Remap(saturation, -1f, 0f, gray, pb);
				}
				if (tp.Red != 0.0 || green != 0.0 || blue != 0.0) {
					pr = Remap(tp.Red, 0f, -1f, Remap(green, 0f, -1f, Remap(blue, 0f, -1f, pr, gray), gray), 1.0);
					pg = Remap(green, 0f, -1f, Remap(tp.Red, 0f, -1f, Remap(blue, 0f, -1f, pg, gray), gray), 1.0);
					pb = Remap(blue, 0f, -1f, Remap(tp.Red, 0f, -1f, Remap(green, 0f, -1f, pb, gray), gray), 1.0);
				}

				pixels[i] = new Color32((byte)(pr > 255.0 ? 255.0 : pr < 0.0 ? 0.0 : pr),
										(byte)(pg > 255.0 ? 255.0 : pg < 0.0 ? 0.0 : pg),
										(byte)(pb > 255.0 ? 255.0 : pb < 0.0 ? 0.0 : pb),
										pixels[i].a);
			}
			
			result.SetPixels32(pixels, 0);
			result.Apply(true, false);
		}

		// https://wiki.unity3d.com/index.php/TextureScale

		class Scale {
			class ThreadData {
				public int start, end;
				public ThreadData(int s, int e) { start = s; end = e; }
			}

			static Color32[] texColors, newColors;
			static int w, w2;
			static float ratioX, ratioY;		static int finishCount;
			static Mutex mutex;

			public static Color32[] Point(Texture2D tex, Vector2Int newSize) {
				return ThreadedScale(tex, newSize.x, newSize.y, false);
			}

			public static Color32[] Bilinear(Texture2D tex, Vector2Int newSize) {
				return ThreadedScale(tex, newSize.x, newSize.y, true);
			}

			static Color32[] ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear) {
				texColors = tex.GetPixels32();
				newColors = new Color32[newWidth * newHeight];
				w = tex.width;
				w2 = newWidth;
				if (useBilinear) {
					ratioX = 1.0f / ((float)w2 / (w - 1));
					ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
				}
				else {
					ratioX = ((float)w) / w2;
					ratioY = ((float)tex.height) / newHeight;
				}
				var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
				var slice = newHeight/cores;

				finishCount = 0;
				if (mutex == null) {
					mutex = new Mutex(false);
				}
				if (cores > 1) {
					int i = 0;
					ThreadData threadData;
					for (i = 0; i < cores - 1; i++) {
						threadData = new ThreadData(slice * i, slice * (i + 1));
						ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
						Thread thread = new Thread(ts);
						thread.Start(threadData);
					}
					threadData = new ThreadData(slice * i, newHeight);
					if (useBilinear) {
						BilinearScale(threadData);
					}
					else {
						PointScale(threadData);
					}
					while (finishCount < cores) {
						Thread.Sleep(1);
					}
				}
				else {
					ThreadData threadData = new ThreadData(0, newHeight);
					if (useBilinear) {
						BilinearScale(threadData);
					}
					else {
						PointScale(threadData);
					}
				}

				return newColors;
				//tex.Resize(newWidth, newHeight);
				//tex.SetPixels(newColors);
				//tex.Apply();
			}//

			static void BilinearScale(System.Object obj) {
				ThreadData threadData = (ThreadData) obj;
				for (var y = threadData.start; y < threadData.end; y++) {
					int yFloor = (int)Mathf.Floor(y * ratioY);
					var y1 = yFloor * w;
					var y2 = (yFloor+1) * w;
					var yw = y * w2;

					for (var x = 0; x < w2; x++) {
						int xFloor = (int)Mathf.Floor(x * ratioX);
						var xLerp = x * ratioX-xFloor;
						newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
							ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
							y * ratioY - yFloor);
					}
				}

				mutex.WaitOne();
				finishCount++;
				mutex.ReleaseMutex();
			}

			static void PointScale(System.Object obj) {
				ThreadData threadData = (ThreadData) obj;
				for (var y = threadData.start; y < threadData.end; y++) {
					var thisY = (int)(ratioY * y) * w;
					var yw = y * w2;
					for (var x = 0; x < w2; x++) {
						newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
					}
				}

				mutex.WaitOne();
				finishCount++;
				mutex.ReleaseMutex();
			}

			private static Color ColorLerpUnclamped(Color c1, Color c2, float value) {
				return new Color(c1.r + (c2.r - c1.r) * value,
								c1.g + (c2.g - c1.g) * value,
								c1.b + (c2.b - c1.b) * value,
								c1.a + (c2.a - c1.a) * value);
			}
		}
#endif
	}

}
