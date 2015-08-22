using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace Ferr {
	public class SuperCubeCheatSheet : EditorWindow {
		string  mText   = "";

		[MenuItem ("Tools/Ferr SuperCube/Cheat sheet", false, 200)]
		public static void  ShowCheatSheet () {
			SuperCubeCheatSheet sheet = EditorWindow.GetWindow<SuperCubeCheatSheet>("Cheat Sheet!");
			sheet.Start();
		}

		void Start() {
			StreamReader reader = new StreamReader(Ferr.EditorTools.GetFerrDirectory() + "Ferr/Docs/FerrSuperCube - Cheat sheet.txt");
			mText = reader.ReadToEnd();
			reader.Close();
		}
		void OnGUI() {
			GUILayout.TextArea(mText);
		}
	}
}