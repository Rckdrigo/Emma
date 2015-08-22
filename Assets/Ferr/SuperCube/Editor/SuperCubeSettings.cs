using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Ferr {
	public static class SuperCubeSettings {
		
		public static bool  HideXRing     {get{return EditorPrefs.GetBool ("Ferr_SuperCube_HideXRing",     true );} set {EditorPrefs.SetBool ("Ferr_SuperCube_HideXRing",     value);}}
		public static bool  HideYRing     {get{return EditorPrefs.GetBool ("Ferr_SuperCube_HideYRing",     false);} set {EditorPrefs.SetBool ("Ferr_SuperCube_HideYRing",     value);}}
		public static bool  HideZRing     {get{return EditorPrefs.GetBool ("Ferr_SuperCube_HideZRing",     true );} set {EditorPrefs.SetBool ("Ferr_SuperCube_HideZRing",     value);}}
		public static bool  ShowGuideX    {get{return EditorPrefs.GetBool ("Ferr_SuperCube_ShowGuideX",    true );} set {EditorPrefs.SetBool ("Ferr_SuperCube_ShowGuideX",    value);}}
		public static bool  ShowGuideY    {get{return EditorPrefs.GetBool ("Ferr_SuperCube_ShowGuideY",    false);} set {EditorPrefs.SetBool ("Ferr_SuperCube_ShowGuideY",    value);}}
		public static bool  ShowGuideZ    {get{return EditorPrefs.GetBool ("Ferr_SuperCube_ShowGuideZ",    true );} set {EditorPrefs.SetBool ("Ferr_SuperCube_ShowGuideZ",    value);}}
		public static float GuideAlpha    {get{return EditorPrefs.GetFloat("Ferr_SuperCube_GuideAlpha",    0.3f );} set {EditorPrefs.SetFloat("Ferr_SuperCube_GuideAlpha",    value);}}
		public static bool  SnapAsDefault {get{return EditorPrefs.GetBool ("Ferr_SuperCube_SnapAsDefault", true );} set {EditorPrefs.SetBool ("Ferr_SuperCube_SnapAsDefault", value);}}
		public static float MaxSize       {get{return EditorPrefs.GetFloat("Ferr_SuperCube_MaxSize",       1000 );} set {EditorPrefs.SetFloat("Ferr_SuperCube_MaxSize",       value);}}
		
		[PreferenceItem("Ferr SuperCube")]
		static void PreferencesGUI() {
			HideXRing     = EditorGUILayout.Toggle("Hide handles on the X axis", HideXRing);
			HideYRing     = EditorGUILayout.Toggle("Hide handles on the Y axis", HideYRing);
			HideZRing     = EditorGUILayout.Toggle("Hide handles on the Z axis", HideZRing);
			ShowGuideX    = EditorGUILayout.Toggle("Show axis guide X",          ShowGuideX);
			ShowGuideY    = EditorGUILayout.Toggle("Show axis guide Y",          ShowGuideY);
			ShowGuideZ    = EditorGUILayout.Toggle("Show axis guide Z",          ShowGuideZ);
			GuideAlpha    = EditorGUILayout.Slider("Axis guide alpha",           GuideAlpha, 0, 1);
			SnapAsDefault = EditorGUILayout.Toggle("Snap as default movement",   SnapAsDefault);
			MaxSize       = EditorGUILayout.FloatField("Max Size",               MaxSize);
			
			if (GUI.changed) {
				SceneView.RepaintAll();
			}
		}
	}
}