using UnityEngine;
using System.Collections;

public class OptionsGUI : MonoBehaviour {

	public float hSliderVolumeValue = 0.0f;
	public float hSliderFXValue = 0.0f;


	void Update()
	{
		//agregar el audioListener
	}



	void OnGUI()
	{
		hSliderVolumeValue = GUI.HorizontalSlider(new Rect(25,75,200,30),
		                                          hSliderVolumeValue,0.0f,10.0f);
		
		hSliderFXValue = GUI.HorizontalSlider(new Rect(25,230,200,30),
		                                      hSliderFXValue,0.0f,10.0f);
	}
}
