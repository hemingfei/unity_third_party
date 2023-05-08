/* Written by Kaz Crowe */
/* UltimateTouchpadReadmeEditor.cs */
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
[CustomEditor( typeof( UltimateTouchpadReadme ) )]
public class UltimateTouchpadReadmeEditor : Editor
{
	static UltimateTouchpadReadme readme;

	// LAYOUT STYLES //
	const string Indent = "    ";
	int sectionSpace = 20;
	int itemHeaderSpace = 10;
	int paragraphSpace = 5;
	GUIStyle titleStyle = new GUIStyle();
	GUIStyle sectionHeaderStyle = new GUIStyle();
	GUIStyle itemHeaderStyle = new GUIStyle();
	GUIStyle paragraphStyle = new GUIStyle();
	GUIStyle versionStyle = new GUIStyle();

	// PAGE INFORMATION //
	class PageInformation
	{
		public string pageName = "";
		public delegate void TargetMethod ();
		public TargetMethod targetMethod;
	}
	static List<PageInformation> pageHistory = new List<PageInformation>();
	static PageInformation[] AllPages = new PageInformation[]
	{
		// MAIN MENU - 0 //
		new PageInformation()
		{
			pageName = "Product Manual"
		},
		// Getting Started - 1 //
		new PageInformation()
		{
			pageName = "Getting Started"
		},
		// Documentation - 2 //
		new PageInformation()
		{
			pageName = "Documentation"
		},
		// Version History - 3 //
		new PageInformation()
		{
			pageName = "Version History"
		},
		// Important Change - 4 //
		new PageInformation()
		{
			pageName = "Important Change"
		},
		// Thank You! - 5 //
		new PageInformation()
		{
			pageName = "Thank You!"
		},
		// Settings - 6 //
		new PageInformation()
		{
			pageName = "Settings"
		},
	};
	
	// DOCUMENTATION //
	class DocumentationInfo
	{
		public string functionName = "";
		public bool showMore = false;
		public string[] parameter;
		public string returnType = "";
		public string description = "";
		public string codeExample = "";
	}
	DocumentationInfo[] PublicFunctions = new DocumentationInfo[]
	{
		new DocumentationInfo()
		{
			functionName = "UpdatePositioning()",
			description = "Updates the size and positioning of the Ultimate Touchpad. This function can be used to update any options that may have been changed prior to Start().",
			codeExample = "touchpad.UpdatePositioning();"
		},
		new DocumentationInfo()
		{
			functionName = "GetHorizontalAxis()",
			returnType = "float",
			description = "Returns the current horizontal value of the touchpad's position.",
			codeExample = "float h = touchpad.GetHorizontalAxis();"
		},
		new DocumentationInfo()
		{
			functionName = "GetVerticalAxis()",
			returnType = "float",
			description = "Returns the current vertical value of the touchpad's position.",
			codeExample = "float v = touchpad.GetVerticalAxis();"
		},
		new DocumentationInfo()
		{
			functionName = "GetTouchpadState()",
			returnType = "bool",
			description = "Returns the state that the touchpad is currently in. This function will return true when the touchpad is being interacted with, and false when not.",
			codeExample = "if( touchpad.GetTouchpadState() )\n{\n    Debug.Log( \"The user is interacting with the Ultimate Touchpad!\" );\n}"
		},
		new DocumentationInfo()
		{
			functionName = "GetTapCount()",
			returnType = "bool",
			description = "Returns the current state of the touchpad's Tap Count according to the options set. The boolean returned will be true only after the Tap Count options have been achieved from the users input.",
			codeExample = "if( touchpad.GetTapCount() )\n{\n    Debug.Log( \"The user has achieved the Tap Count!\" );\n}"
		},
		new DocumentationInfo()
		{
			functionName = "DisableTouchpad()",
			description = "This function will reset the Ultimate Touchpad and disable the gameObject. Use this function when wanting to disable the Ultimate Touchpad from being used.",
			codeExample = "touchpad.DisableTouchpad();"
		},
		new DocumentationInfo()
		{
			functionName = "EnableTouchpad()",
			description = "This function will ensure that the Ultimate Touchpad is completely reset before enabling itself to be used again.",
			codeExample = "touchpad.EnableTouchpad();"
		},
	};
	DocumentationInfo[] StaticFunctions = new DocumentationInfo[] 
	{
		new DocumentationInfo()
		{
			functionName = "GetUltimateTouchpad()",
			parameter = new string[ 1 ]
			{
				"string touchpadName - The name that the targeted Ultimate Touchpad has been registered with."
			},
			returnType = "UltimateTouchpad",
			description = "Returns the Ultimate Touchpad component that has been registered with the targeted touchpad name.",
			codeExample = "UltimateTouchpad lookTouchpad = UltimateTouchpad.GetUltimateTouchpad( \"Camera\" );"
		},
		new DocumentationInfo()
		{
			functionName = "GetHorizontalAxis()",
			parameter = new string[ 1 ]
			{
				"string touchpadName - The name that the targeted Ultimate Touchpad has been registered with."
			},
			returnType = "float",
			description = "Returns the current horizontal value of the targeted touchpad's position.",
			codeExample = "float h = UltimateTouchpad.GetHorizontalAxis( \"Camera\" );"
		},
		new DocumentationInfo()
		{
			functionName = "GetVerticalAxis()",
			parameter = new string[ 1 ]
			{
				"string touchpadName - The name that the targeted Ultimate Touchpad has been registered with."
			},
			returnType = "float",
			description = "Returns the current vertical value of the targeted touchpad's position.",
			codeExample = "float v = UltimateTouchpad.GetVerticalAxis( \"Camera\" );"
		},
		new DocumentationInfo()
		{
			functionName = "GetTouchpadState()",
			parameter = new string[ 1 ]
			{
				"string touchpadName - The name that the targeted Ultimate Touchpad has been registered with."
			},
			returnType = "bool",
			description = "Returns the state that the touchpad is currently in. This function will return true when the touchpad is being interacted with, and false when not.",
			codeExample = "if( UltimateTouchpad.GetTouchpadState( \"Camera\" ) )\n{\n    Debug.Log( \"The user is interacting with the Ultimate Touchpad!\" );\n}"
		},
		new DocumentationInfo()
		{
			functionName = "GetTapCount()",
			parameter = new string[ 1 ]
			{
				"string touchpadName - The name that the targeted Ultimate Touchpad has been registered with."
			},
			returnType = "bool",
			description = "Returns the current state of the touchpad's Tap Count according to the options set. The boolean returned will be true only after the Tap Count options have been achieved from the users input.",
			codeExample = "if( UltimateTouchpad.GetTapCount( \"Camera\" ) )\n{\n    Debug.Log( \"The user has achieved the Tap Count!\" );\n}"
		},
		new DocumentationInfo()
		{
			functionName = "DisableTouchpad()",
			parameter = new string[ 1 ]
			{
				"string touchpadName - The name that the targeted Ultimate Touchpad has been registered with."
			},
			description = "This function will reset the Ultimate Touchpad and disable the gameObject. Use this function when wanting to disable the Ultimate Touchpad from being used.",
			codeExample = "UltimateTouchpad.DisableTouchpad( \"Camera\" );"
		},
		new DocumentationInfo()
		{
			functionName = "EnableTouchpad()",
			parameter = new string[ 1 ]
			{
				"string touchpadName - The name that the targeted Ultimate Touchpad has been registered with."
			},
			description = "This function will ensure that the Ultimate Touchpad is completely reset before enabling itself to be used again.",
			codeExample = "UltimateTouchpad.EnableTouchpad( \"Camera\" );"
		},
	};

	class EndPageComment
	{
		public string comment = "";
		public string url = "";
	}
	EndPageComment[] endPageComments = new EndPageComment[]
	{
		new EndPageComment()
		{
			comment = "Enjoying the Ultimate Touchpad? Leave us a review on the <b><color=blue>Unity Asset Store</color></b>!",
			url = "https://assetstore.unity.com/packages/slug/108921"
		},
		new EndPageComment()
		{
			comment = "Looking for a button to match the Ultimate Touchpad and Ultimate Joystick? Check out the <b><color=blue>Ultimate Button</color></b>!",
			url = "https://www.tankandhealerstudio.com/ultimate-button.html"
		},
		new EndPageComment()
		{
			comment = "Looking for a radial menu for your game? Check out the <b><color=blue>Ultimate Radial Menu</color></b>!",
			url = "https://www.tankandhealerstudio.com/ultimate-radial-menu.html"
		},
		new EndPageComment()
		{
			comment = "Looking for a health bar for your game? Check out the <b><color=blue>Ultimate Status Bar</color></b>!",
			url = "https://www.tankandhealerstudio.com/ultimate-status-bar.html"
		},
		new EndPageComment()
		{
			comment = "Check out our <b><color=blue>other products</color></b>!",
			url = "https://www.tankandhealerstudio.com/assets.html"
		},
	};
	int randomComment = 0;


	static UltimateTouchpadReadmeEditor ()
	{
		EditorApplication.update += WaitForCompile;
	}

	static void WaitForCompile ()
	{
		if( EditorApplication.isCompiling )
			return;

		EditorApplication.update -= WaitForCompile;
		
		// If this is the first time that the user has downloaded the Ultimate Touchpad...
		if( !EditorPrefs.HasKey( "UltimateTouchpadVersion" ) )
		{
			// Set the version to current so they won't see these version changes.
			EditorPrefs.SetInt( "UltimateTouchpadVersion", UltimateTouchpadReadme.ImportantChange );

			// Select the readme file.
			SelectReadmeFile();

			// If the readme file is assigned, then navigate to the Thank You page.
			if( readme != null )
				NavigateForward( 5 );
		}
		else if( EditorPrefs.GetInt( "UltimateTouchpadVersion" ) < UltimateTouchpadReadme.ImportantChange )
		{
			// Set the version to current so they won't see this page again.
			EditorPrefs.SetInt( "UltimateTouchpadVersion", UltimateTouchpadReadme.ImportantChange );

			// Select the readme file.
			SelectReadmeFile();

			// If the readme file is assigned, then navigate to the Version Changes page.
			if( readme != null )
				NavigateForward( 4 );
		}
	}

	void OnEnable ()
	{
		readme = ( UltimateTouchpadReadme )target;

		if( !EditorPrefs.HasKey( "UT_ColorHexSetup" ) )
		{
			EditorPrefs.SetBool( "UT_ColorHexSetup", true );
			ResetColors();
		}
		
		ColorUtility.TryParseHtmlString( EditorPrefs.GetString( "UT_ColorValueChangedHex" ), out readme.colorValueChanged );
		
		AllPages[ 0 ].targetMethod = MainPage;
		AllPages[ 1 ].targetMethod = GettingStarted;
		AllPages[ 2 ].targetMethod = Documentation;
		AllPages[ 3 ].targetMethod = VersionHistory;
		AllPages[ 4 ].targetMethod = ImportantChange;
		AllPages[ 5 ].targetMethod = ThankYou;
		AllPages[ 6 ].targetMethod = Settings;

		pageHistory = new List<PageInformation>();
		for( int i = 0; i < readme.pageHistory.Count; i++ )
			pageHistory.Add( AllPages[ readme.pageHistory[ i ] ] );

		if( !pageHistory.Contains( AllPages[ 0 ] ) )
		{
			pageHistory.Insert( 0, AllPages[ 0 ] );
			readme.pageHistory.Insert( 0, 0 );
		}

		randomComment = Random.Range( 0, endPageComments.Length );

		Undo.undoRedoPerformed += UndoRedoCallback;
	}

	void OnDisable ()
	{
		// Remove the UndoRedoCallback from the Undo event.
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}

	void UndoRedoCallback ()
	{
		if( pageHistory[ pageHistory.Count - 1 ] != AllPages[ 6 ] )
			return;
		
		EditorPrefs.SetString( "UT_ColorValueChangedHex", "#" + ColorUtility.ToHtmlStringRGBA( readme.colorValueChanged ) );
	}

	protected override void OnHeaderGUI ()
	{
		UltimateTouchpadReadme readme = ( UltimateTouchpadReadme )target;

		var iconWidth = Mathf.Min( EditorGUIUtility.currentViewWidth, 350f );

		Vector2 ratio = new Vector2( readme.icon.width, readme.icon.height ) / ( readme.icon.width > readme.icon.height ? readme.icon.width : readme.icon.height );

		GUILayout.BeginHorizontal( "In BigTitle" );
		{
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.Label( readme.icon, GUILayout.Width( iconWidth * ratio.x ), GUILayout.Height( iconWidth * ratio.y ) );
			GUILayout.Space( -20 );
			if( GUILayout.Button( readme.versionHistory[ 0 ].versionNumber, versionStyle ) && !pageHistory.Contains( AllPages[ 3 ] ) )
				NavigateForward( 3 );
			var rect = GUILayoutUtility.GetLastRect();
			if( pageHistory[ pageHistory.Count - 1 ] != AllPages[ 3 ] )
				EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}
		GUILayout.EndHorizontal();
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();

		paragraphStyle = new GUIStyle( EditorStyles.label ) { wordWrap = true, richText = true, fontSize = 12 };
		itemHeaderStyle = new GUIStyle( paragraphStyle ) { fontSize = 12, fontStyle = FontStyle.Bold };
		sectionHeaderStyle = new GUIStyle( paragraphStyle ) { fontSize = 14, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
		titleStyle = new GUIStyle( paragraphStyle ) { fontSize = 16, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
		versionStyle = new GUIStyle( paragraphStyle ) { alignment = TextAnchor.MiddleCenter, fontSize = 10 };

		paragraphStyle.active.textColor = paragraphStyle.normal.textColor;

		// SETTINGS BUTTON //
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( readme.settings, versionStyle, GUILayout.Width( 24 ), GUILayout.Height( 24 ) ) && !pageHistory.Contains( AllPages[ 6 ] ) )
			NavigateForward( 6 );
		var rect = GUILayoutUtility.GetLastRect();
		if( pageHistory[ pageHistory.Count - 1 ] != AllPages[ 6 ] )
			EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		GUILayout.EndHorizontal();
		GUILayout.Space( -24 );
		GUILayout.EndVertical();

		// BACK BUTTON //
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginDisabledGroup( pageHistory.Count <= 1 );
		if( GUILayout.Button( "◄", titleStyle, GUILayout.Width( 24 ) ) )
			NavigateBack();
		if( pageHistory.Count > 1 )
		{
			rect = GUILayoutUtility.GetLastRect();
			EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		}
		EditorGUI.EndDisabledGroup();
		GUILayout.Space( -24 );

		// PAGE TITLE //
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField( pageHistory[ pageHistory.Count - 1 ].pageName, titleStyle );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		// DISPLAY PAGE //
		if( pageHistory[ pageHistory.Count - 1 ].targetMethod != null )
			pageHistory[ pageHistory.Count - 1 ].targetMethod();

		Repaint();
	}

	void StartPage ()
	{
		readme.scrollValue = EditorGUILayout.BeginScrollView( readme.scrollValue, false, false );
		GUILayout.Space( 15 );
	}

	void EndPage ()
	{
		EditorGUILayout.EndScrollView();
	}

	static void NavigateBack ()
	{
		readme.pageHistory.RemoveAt( readme.pageHistory.Count - 1 );
		pageHistory.RemoveAt( pageHistory.Count - 1 );
		GUI.FocusControl( "" );

		readme.scrollValue = Vector2.zero;

		if( readme.pageHistory.Count == 1 )
			EditorUtility.SetDirty( readme );
	}

	static void NavigateForward ( int menuIndex )
	{
		pageHistory.Add( AllPages[ menuIndex ] );
		GUI.FocusControl( "" );

		readme.pageHistory.Add( menuIndex );
		readme.scrollValue = Vector2.zero;
	}

	void MainPage ()
	{
		StartPage();

		EditorGUILayout.LabelField( "We hope that you are enjoying using the Ultimate Touchpad in your project! Here is a list of helpful resources for this asset:", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • Read the <b><color=blue>Getting Started</color></b> section of this README!", paragraphStyle );
		var rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 1 );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • Check out the <b><color=blue>Documentation</color></b> section!", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
			NavigateForward( 2 );

		EditorGUILayout.Space();

		if( GUILayout.Button( "  • Join our <b><color=blue>Discord Server</color></b> so that you can get live help from us and other community members.", paragraphStyle ) )
		{
			Debug.Log( "Ultimate Radial Menu\nOpening Tank & Healer Studio Discord Server" );
			Application.OpenURL( "https://discord.gg/YrtEHRqw6y" );
		}
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "  • <b><color=blue>Contact Us</color></b> directly with your issue! We'll try to help you out as much as we can.", paragraphStyle );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );
		if( Event.current.type == EventType.MouseDown && rect.Contains( Event.current.mousePosition ) )
		{
			Debug.Log( "Ultimate Touchpad\nOpening Online Contact Form" );
			Application.OpenURL( "https://www.tankandhealerstudio.com/contact-us.html" );
		}
		
		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Now you have the tools you need to get the Ultimate Touchpad working in your project. Now get out there and make your awesome game!", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Happy Game Making,\n" + Indent + "Tank & Healer Studio", paragraphStyle );

		GUILayout.FlexibleSpace();

		if( GUILayout.Button( endPageComments[ randomComment ].comment, paragraphStyle ) )
			Application.OpenURL( endPageComments[ randomComment ].url );
		rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );

		EndPage();
	}

	void GettingStarted ()
	{
		StartPage( );

		EditorGUILayout.LabelField( "How To Create", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( Indent + "To create an Ultimate Touchpad in your scene, simply find the Ultimate Touchpad prefab that you would like to add and click and drag the prefab into the scene. The Ultimate Touchpad prefab will automatically find or create a canvas in your scene for you.", paragraphStyle );

		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "How To Reference", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( Indent + "The Ultimate Touchpad is very easy to reference into your own scripts. Simply assign a name to the Ultimate Touchpad inside the Script Reference section of the inspector, and then copy the example code provided.", paragraphStyle );

		Vector2 ratio = new Vector2( readme.scriptReference.width, readme.scriptReference.height ) / ( readme.scriptReference.width > readme.scriptReference.height ? readme.scriptReference.width : readme.scriptReference.height );

		float imageWidth = readme.scriptReference.width > Screen.width - 50 ? Screen.width - 50 : readme.scriptReference.width;

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( readme.scriptReference, GUILayout.Width( imageWidth ), GUILayout.Height( imageWidth * ratio.y ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "Below is a brief list of the key functions.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Key Functions", itemHeaderStyle );
		EditorGUILayout.LabelField( "• GetHorizontalAxis - Returns the float value of the horizontal axis of the Ultimate Touchpad.", paragraphStyle );
		EditorGUILayout.LabelField( "• GetVerticalAxis - Returns the float value of the vertical axis of the Ultimate Touchpad.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "Many times, people use float values to catch the horizontal and vertical input values. Their code often looks like this:", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Coding Example:", itemHeaderStyle );
		EditorGUILayout.LabelField( "float h = Input.GetAxis( \"Mouse X\" );\nfloat v = Input.GetAxis( \"Mouse Y\" );", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "In cases like this, all you need to do is replace the default input with the Ultimate Touchpad's. The updated code would look like this:", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Coding Example:", itemHeaderStyle );
		EditorGUILayout.TextArea( "float h = UltimateTouchpad.GetHorizontalAxis( \"Camera\" );\nfloat v = UltimateTouchpad.GetVerticalAxis( \"Camera\" );", GUI.skin.GetStyle( "TextArea" ) );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "For more information about each function and how to use it, please see the Documentation section of this window.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "Understanding how to use the values from any input is important when creating character controllers, so experiment with the values and try to understand how user input can be used in different ways.", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EndPage();
	}
	
	void Documentation ()
	{
		StartPage();

		/* //// --------------------------- < STATIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Static Functions", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( Indent + "The following functions can be referenced from your scripts without the need for an assigned local Ultimate Touchpad variable. However, each function must have the targeted Ultimate Touchpad name in order to find the correct Ultimate Touchpad in the scene. Each example code provided uses the name 'Camera' as the touchpad name.", paragraphStyle );

		Vector2 ratio = new Vector2( readme.scriptReference.width, readme.scriptReference.height ) / ( readme.scriptReference.width > readme.scriptReference.height ? readme.scriptReference.width : readme.scriptReference.height );
		float imageWidth = readme.scriptReference.width > Screen.width - 50 ? Screen.width - 50 : readme.scriptReference.width;

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label( readme.scriptReference, GUILayout.Width( imageWidth ), GUILayout.Height( imageWidth * ratio.y ) );
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.LabelField( "Please click on the function name to learn more.", paragraphStyle );

		for( int i = 0; i < StaticFunctions.Length; i++ )
			ShowDocumentation( StaticFunctions[ i ] );

		GUILayout.Space( sectionSpace );

		/* //// --------------------------- < PUBLIC FUNCTIONS > --------------------------- \\\\ */
		EditorGUILayout.LabelField( "Public Functions", sectionHeaderStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( Indent + "All of the following public functions are only available from a reference to the Ultimate Touchpad. Each example provided relies on having a Ultimate Touchpad variable named 'touchpad' stored inside your script. When using any of the example code provided, make sure that you have a public Ultimate Touchpad variable like the one below:", paragraphStyle );

		EditorGUILayout.TextArea( "public UltimateTouchpad touchpad;", GUI.skin.textArea );

		GUILayout.Space( paragraphSpace );

		for( int i = 0; i < PublicFunctions.Length; i++ )
			ShowDocumentation( PublicFunctions[ i ] );

		GUILayout.Space( itemHeaderSpace );

		EndPage();
	}

	void VersionHistory ()
	{
		StartPage();

		for( int i = 0; i < readme.versionHistory.Length; i++ )
		{
			EditorGUILayout.LabelField( "Version " + readme.versionHistory[ i ].versionNumber, itemHeaderStyle );

			for( int n = 0; n < readme.versionHistory[ i ].changes.Length; n++ )
				EditorGUILayout.LabelField( "  • " + readme.versionHistory[ i ].changes[ n ] + ".", paragraphStyle );

			if( i < ( readme.versionHistory.Length - 1 ) )
				GUILayout.Space( itemHeaderSpace );
		}
		
		EndPage();
	}

	void ImportantChange ()
	{
		StartPage();

		EditorGUILayout.LabelField( Indent + "Thank you for downloading the most recent version of the Ultimate Touchpad. If you are experiencing any errors, please completely remove the Ultimate Touchpad from your project and re-import it. As always, if you run into any issues with the Ultimate Touchpad, please contact us at:", paragraphStyle );

		GUILayout.Space( paragraphSpace );
		EditorGUILayout.SelectableLabel( "tankandhealerstudio@outlook.com", itemHeaderStyle, GUILayout.Height( 15 ) );
		GUILayout.Space( sectionSpace );

		EditorGUILayout.LabelField( "NEW FILES", sectionHeaderStyle );
		EditorGUILayout.LabelField( "  • UltimateTouchpadReadme.cs", paragraphStyle );
		EditorGUILayout.LabelField( "  • UltimateTouchpadReadmeEditor.cs", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.LabelField( "OLD FILES", sectionHeaderStyle );
		EditorGUILayout.LabelField( "The file listed below is not longer used, and can (and should) be removed from your project. All the information that was previously inside this script is now included in the Ultimate Touchpad README.", paragraphStyle );
		EditorGUILayout.LabelField( "  • UltimateTouchpadWindow.cs", paragraphStyle );

		GUILayout.Space( itemHeaderSpace );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Got it!", GUILayout.Width( Screen.width / 2 ) ) )
			NavigateBack();

		var rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EndPage();
	}

	void ThankYou ()
	{
		StartPage();

		EditorGUILayout.LabelField( "The two of us at Tank & Healer Studio would like to thank you for purchasing the Ultimate Touchpad asset package from the Unity Asset Store.", paragraphStyle );

		GUILayout.Space( paragraphSpace );

		EditorGUILayout.LabelField( "We hope that the Ultimate Touchpad will be a great help to you in the development of your game. After clicking the <i>Continue</i> button below, you will be presented with information to assist you in getting to know the Ultimate Touchpad and getting it implementing into your project.", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "You can access this information at any time by clicking on the <b>README</b> file inside the Ultimate Touchpad folder.", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Again, thank you for downloading the Ultimate Touchpad. We hope that your project is a success!", paragraphStyle );

		EditorGUILayout.Space();

		EditorGUILayout.LabelField( "Happy Game Making,\n" + Indent + "Tank & Healer Studio", paragraphStyle );

		GUILayout.Space( 15 );

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Continue", GUILayout.Width( Screen.width / 2 ) ) )
			NavigateBack();

		var rect2 = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect2, MouseCursor.Link );

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EndPage();
	}

	void Settings ()
	{
		StartPage();

		EditorGUILayout.LabelField( "Gizmo Colors", sectionHeaderStyle );
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField( serializedObject.FindProperty( "colorValueChanged" ), new GUIContent( "Value Changed" ) );
		if( EditorGUI.EndChangeCheck() )
		{
			serializedObject.ApplyModifiedProperties();
			EditorPrefs.SetString( "UT_ColorValueChangedHex", "#" + ColorUtility.ToHtmlStringRGBA( readme.colorValueChanged ) );
		}

		EditorGUILayout.Space();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if( GUILayout.Button( "Reset", GUILayout.Width( EditorGUIUtility.currentViewWidth / 2 ) ) )
		{
			if( EditorUtility.DisplayDialog( "Reset Gizmo Color", "Are you sure that you want to reset the gizmo colors back to default?", "Yes", "No" ) )
			{
				ResetColors();
			}
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		if( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) )
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField( "Development Mode", sectionHeaderStyle );
			base.OnInspectorGUI();
			EditorGUILayout.Space();
		}

		GUILayout.FlexibleSpace();

		GUILayout.Space( sectionSpace );

		EditorGUI.BeginChangeCheck();
		GUILayout.Toggle( EditorPrefs.GetBool( "UUI_DevelopmentMode" ), ( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) ? "Disable" : "Enable" ) + " Development Mode", EditorStyles.radioButton );
		if( EditorGUI.EndChangeCheck() )
		{
			if( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) == false )
			{
				if( EditorUtility.DisplayDialog( "Enable Development Mode", "Are you sure you want to enable development mode for Tank & Healer Studio assets? This mode will allow you to see the default inspector for this asset which is useful when adding variables to this script without having to edit the custom editor script.", "Enable", "Cancel" ) )
				{
					EditorPrefs.SetBool( "UUI_DevelopmentMode", !EditorPrefs.GetBool( "UUI_DevelopmentMode" ) );
				}
			}
			else
				EditorPrefs.SetBool( "UUI_DevelopmentMode", !EditorPrefs.GetBool( "UUI_DevelopmentMode" ) );
		}

		EndPage();
	}

	void ResetColors ()
	{
		serializedObject.FindProperty( "colorValueChanged" ).colorValue = Color.yellow;
		serializedObject.ApplyModifiedProperties();
		
		EditorPrefs.SetString( "UT_ColorValueChangedHex", "#" + ColorUtility.ToHtmlStringRGBA( Color.yellow ) );
	}

	void ShowDocumentation ( DocumentationInfo info )
	{
		GUILayout.Space( paragraphSpace );

		if( GUILayout.Button( info.functionName, itemHeaderStyle ) )
		{
			info.showMore = !info.showMore;
			GUI.FocusControl( "" );
		}
		var rect = GUILayoutUtility.GetLastRect();
		EditorGUIUtility.AddCursorRect( rect, MouseCursor.Link );

		if( info.showMore )
		{
			EditorGUILayout.LabelField( Indent + "<i>Description:</i> " + info.description, paragraphStyle );

			if( info.parameter != null )
			{
				for( int i = 0; i < info.parameter.Length; i++ )
					EditorGUILayout.LabelField( Indent + "<i>Parameter:</i> " + info.parameter[ i ], paragraphStyle );
			}
			if( info.returnType != string.Empty )
				EditorGUILayout.LabelField( Indent + "<i>Return type:</i> " + info.returnType, paragraphStyle );

			if( info.codeExample != string.Empty )
				EditorGUILayout.TextArea( info.codeExample, GUI.skin.textArea );

			GUILayout.Space( paragraphSpace );
		}
	}

	public static void OpenReadmeDocumentation ()
	{
		SelectReadmeFile();

		if( !pageHistory.Contains( AllPages[ 2 ] ) )
			NavigateForward( 2 );
	}

	[MenuItem( "Window/Tank and Healer Studio/Ultimate Touchpad", false, 5 )]
	public static void SelectReadmeFile ()
	{
		var ids = AssetDatabase.FindAssets( "README t:UltimateTouchpadReadme" );
		if( ids.Length == 1 )
		{
			var readmeObject = AssetDatabase.LoadMainAssetAtPath( AssetDatabase.GUIDToAssetPath( ids[ 0 ] ) );
			Selection.objects = new Object[] { readmeObject };
			readme = ( UltimateTouchpadReadme )readmeObject;
		}
		else
			Debug.LogError( "There is no README object in the Ultimate Touchpad folder." );
	}
}