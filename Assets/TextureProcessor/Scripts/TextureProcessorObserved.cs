using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RatKing {

	public class TextureProcessorObserved : ScriptableObject {
#if UNITY_EDITOR
		public List<TextureProcessor> observed = null; // TODO non public?

		//

		// https://answers.unity.com/questions/1274030/unity-duplicate-event.html
		void OnValidate() {
			var e = Event.current;
			if (e != null && (e.type == EventType.ExecuteCommand || e.type == EventType.Used) && e.commandName == "Duplicate") {
				Debug.LogError("Only one TextureProcessorObserved asset per project supported!");
			}
		}

		void OnEnable() {
			CleanUp();
		}

		void CleanUp() {
			if (observed == null) { return; }
			for (int i = observed.Count - 1; i >= 0; --i) {
				if (observed[i] == null) { observed.RemoveAt(i); }
			}
		}

		public void Add(TextureProcessor tp) {
			if (observed == null) { observed = new List<TextureProcessor>(); }
			if (!observed.Contains(tp)) { observed.Add(tp); }
			CleanUp();
			UnityEditor.EditorUtility.SetDirty(this);
		}

		public void Remove(TextureProcessor tp) {
			if (observed == null) { return; }
			observed.Remove(tp);
			CleanUp();
			UnityEditor.EditorUtility.SetDirty(this);
		}
#endif
	}

}
