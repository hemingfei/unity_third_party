/* UltimateTouchpadReadme.cs */
/* Written by Kaz Crowe */
using UnityEngine;
using System.Collections.Generic;

//[CreateAssetMenu( fileName = "README", menuName = "ScriptableObjects/UltimateTouchpad", order = 1 )]
public class UltimateTouchpadReadme : ScriptableObject
{
	public Texture2D icon;
	public Texture2D scriptReference;
	public Texture2D settings;

	// GIZMO COLORS //
	[HideInInspector]
	public Color colorValueChanged = new Color( 1.0f, 1.0f, 0.0f, 1.0f );

	// VERSION CHANGES //
	public static int ImportantChange = 1;
	public class VersionHistory
	{
		public string versionNumber = "";
		public string[] changes;
	}
	public VersionHistory[] versionHistory = new VersionHistory[]
	{
		// VERSION 1.5.0 // IMPORTANT CHANGE 2
		new VersionHistory ()
		{
			versionNumber = "1.5.0",
			changes = new string[]
			{
				// General Changes //
				"Simplified the positioning options of the touchpad",
				"Changed the Display Base option to be a simple boolean to enable/disable the option",
				"Updated the README file to be easier to work with",
				"Updated the README file to be able to stay on the same page, even after compiling scripts",
				"Updated the Style 02 source image as it was not displaying correctly",
				"Improved calculations to support various canvas setups",
				"Added new calculations for using Touch Input specifically instead of Unity's EventSystem",
				// Editor Improvements //
				"Reorganized the inspector to improve workflow",
				"Updated the inspector to make it easier to add and remove options",
				"Added an option for developers that want to expand on the Ultimate Touchpad code. The option is located in the Settings of the README window. To access it, select the README file and click the gear icon in the top right. There will be an option at the bottom for Enable Development Mode. Now the Ultimate Touchpad inspector will have a new section: Development Inspector",
			},
		},
		// VERSION 1.1.1
		new VersionHistory ()
		{
			versionNumber = "1.1.1",
			changes = new string[]
			{
				"Fixed an error that would occur when trying to use the Display Base option without assigning a Base Image",
			},
		},
		// VERSION 1.1.0 // IMPORTANT CHANGE 1
		new VersionHistory ()
		{
			versionNumber = "1.1.0",
			changes = new string[]
			{
				"Fixed a rare issue with the base image of the touchpad getting stuck",
				"Simplified the editor script internally",
				"Removed AnimBool functionality from the inspector to avoid errors with Unity 2019+",
				"Removed warning message about Touch Input Module for versions Unity higher than 5.6.0f",
				"Renamed the four float variables used for Custom Touch Size",
				"Added new script: UltimateTouchpadReadme.cs",
				"Added new script: UltimateTouchpadReadmeEditor.cs",
				"Added new file at the Ultimate Touchpad root folder: README. This file has all the documentation and how to information",
				"Removed the UltimateTouchpadWindow.cs file. All of that information is now located in the README file",
				"Removed the old README text file. All of that information is now located in the README file",
			},
		},
		// VERSION 1.0.1
		new VersionHistory ()
		{
			versionNumber = "1.0.1",
			changes = new string[]
			{
				"Added complete In-Engine Documentation Window",
			},
		},
		// VERSION 1.0
		new VersionHistory ()
		{
			versionNumber = "1.0",
			changes = new string[]
			{
				"Initial Release",
			},
		},
	};

	[HideInInspector]
	public List<int> pageHistory = new List<int>();
	[HideInInspector]
	public Vector2 scrollValue = new Vector2();
}