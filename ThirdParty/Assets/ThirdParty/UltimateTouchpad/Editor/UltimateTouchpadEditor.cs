/* Written by Kaz Crowe */
/* UltimateTouchpadEditor.cs */
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[CustomEditor( typeof( UltimateTouchpad ) )]
public class UltimateTouchpadEditor : Editor
{
	UltimateTouchpad targ;
	bool isPrefabInProjectWindow = false;
	const int afterIndentSpace = 5;

	Canvas parentCanvas;
	//SerializedProperty anchor;
	SerializedProperty touchSizeWidth, touchSizeHeight;
	SerializedProperty touchSizePositionX, touchSizePositionY;
	SerializedProperty screenWidthReference;
	SerializedProperty xSensitivity, ySensitivity;
	SerializedProperty xGravity, yGravity;
	SerializedProperty touchpadName;
	SerializedProperty displayBase, displayType;
	SerializedProperty baseImage, baseColor;
	SerializedProperty alphaEnabled, alphaDisabled;
	SerializedProperty enabledDuration, disabledDuration;
	SerializedProperty inactiveTime;
	SerializedProperty displayInput;
	SerializedProperty inputImage, inputColor;
	SerializedProperty scaleReference, imageSize;
	SerializedProperty tapCountOption;
	SerializedProperty tapCountDuration, targetTapCount;
	SerializedProperty useTouchInput;

	Sprite baseImageSprite, inputImageSprite;

	// DEVELOPMENT MODE //
	public bool showDefaultInspector = false;

	// ----->>> EXAMPLE CODE //
	class ExampleCode
	{
		public string optionName = "";
		public string optionDescription = "";
		public string basicCode = "";
	}
	ExampleCode[] exampleCodes = new ExampleCode[]
	{
		new ExampleCode() { optionName = "GetHorizontalAxis ( string touchpadName )", optionDescription = "Returns the horizontal axis value of the targeted Ultimate Touchpad.", basicCode = "float h = UltimateTouchpad.GetHorizontalAxis( \"{0}\" );" },
		new ExampleCode() { optionName = "GetVerticalAxis ( string touchpadName )", optionDescription = "Returns the vertical axis value of the targeted Ultimate Touchpad.", basicCode = "float v = UltimateTouchpad.GetVerticalAxis( \"{0}\" );" },
		new ExampleCode() { optionName = "GetTouchpadState ( string touchpadName )", optionDescription = "Returns the bool value of the current state of interaction of the targeted Ultimate Touchpad.", basicCode = "if( UltimateTouchpad.GetTouchpadState( \"{0}\" ) )" },
		new ExampleCode() { optionName = "GetTapCount ( string touchpadName )", optionDescription = "Returns the bool value of the current state of taps of the targeted Ultimate Touchpad.", basicCode = "if( UltimateTouchpad.GetTapCount( \"{0}\" ) )" },
		new ExampleCode() { optionName = "DisableTouchpad ( string touchpadName )", optionDescription = "Disables the targeted Ultimate Touchpad.", basicCode = "UltimateTouchpad.DisableTouchpad( \"{0}\" );" },
		new ExampleCode() { optionName = "EnableTouchpad ( string touchpadName )", optionDescription = "Enables the targeted Ultimate Touchpad.", basicCode = "UltimateTouchpad.EnableTouchpad( \"{0}\" );" },
		new ExampleCode() { optionName = "GetUltimateTouchpad ( string touchpadName )", optionDescription = "Returns the Ultimate Touchpad component that has been registered with the targeted name.", basicCode = "UltimateTouchpad cameraTouchpad = UltimateTouchpad.GetUltimateTouchpad( \"{0}\" );" },
	};
	List<string> exampleCodeOptions = new List<string>();
	int exampleCodeIndex = 0;

	// SCENE GUI //
	class DisplaySceneGizmo
	{
		public int frames = maxFrames;
		public bool hover = false;

		public bool HighlightGizmo
		{
			get
			{
				return hover || frames < maxFrames;
			}
		}

		public void PropertyUpdated ()
		{
			frames = 0;
		}
	}
	DisplaySceneGizmo DisplayTouchWidth = new DisplaySceneGizmo();
	DisplaySceneGizmo DisplayTouchHeight = new DisplaySceneGizmo();
	const int maxFrames = 200;

	// Gizmo Colors //
	Color colorValueChanged = Color.black;

	// EDITOR STYLES //
	GUIStyle collapsableSectionStyle = new GUIStyle();


	bool CanvasErrors
	{
		get
		{
			// If the selection is currently empty, then return false.
			if( Selection.activeGameObject == null )
				return false;

			// If the selection is actually the prefab within the Project window, then return no errors.
			if( AssetDatabase.Contains( Selection.activeGameObject ) )
				return false;

			// If parentCanvas is unassigned, then get a new canvas and return no errors.
			if( parentCanvas == null )
			{
				parentCanvas = GetParentCanvas();
				return false;
			}

			// If the parentCanvas is not enabled, then return true for errors.
			if( parentCanvas.enabled == false )
				return true;

			// If the canvas' renderMode is not the needed one, then return true for errors.
			if( parentCanvas.renderMode == RenderMode.WorldSpace )
				return true;

			return false;
		}
	}

	void OnEnable ()
	{
		// Store the references to all variables.
		StoreReferences();

		// Register the UndoRedoCallback function to be called when an undo/redo is performed.
		Undo.undoRedoPerformed += UndoRedoCallback;

		if( EditorPrefs.HasKey( "UT_ColorHexSetup" ) )
			ColorUtility.TryParseHtmlString( EditorPrefs.GetString( "UT_ColorValueChangedHex" ), out colorValueChanged );

		parentCanvas = GetParentCanvas();
	}

	void OnDisable ()
	{
		// Remove the UndoRedoCallback function from the Undo event.
		Undo.undoRedoPerformed -= UndoRedoCallback;
	}

	Canvas GetParentCanvas ()
	{
		if( Selection.activeGameObject == null )
			return null;

		// Store the current parent.
		Transform parent = Selection.activeGameObject.transform.parent;

		// Loop through parents as long as there is one.
		while( parent != null )
		{
			// If there is a Canvas component, return the component.
			if( parent.transform.GetComponent<Canvas>() && parent.transform.GetComponent<Canvas>().enabled == true )
				return parent.transform.GetComponent<Canvas>();

			// Else, shift to the next parent.
			parent = parent.transform.parent;
		}
		if( parent == null && !AssetDatabase.Contains( Selection.activeGameObject ) )
			RequestCanvas( Selection.activeGameObject );

		return null;
	}

	void UndoRedoCallback ()
	{
		// Re-reference all variables on undo/redo.
		StoreReferences();
	}

	void DisplayHeaderDropdown ( string headerName, string editorPref )
	{
		EditorGUILayout.Space();

		GUIStyle toolbarStyle = new GUIStyle( EditorStyles.toolbarButton ) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 11 };
		GUILayout.BeginHorizontal();
		GUILayout.Space( -10 );
		EditorPrefs.SetBool( editorPref, GUILayout.Toggle( EditorPrefs.GetBool( editorPref ), ( EditorPrefs.GetBool( editorPref ) ? "▼ " : "► " ) + headerName, toolbarStyle ) );
		GUILayout.EndHorizontal();

		if( EditorPrefs.GetBool( editorPref ) == true )
			EditorGUILayout.Space();
	}

	bool DisplayCollapsibleBoxSection ( string sectionTitle, string editorPref )
	{
		if( EditorPrefs.GetBool( editorPref ) )
			collapsableSectionStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.BeginHorizontal();
		
		if( GUILayout.Button( sectionTitle, collapsableSectionStyle ) )
			EditorPrefs.SetBool( editorPref, !EditorPrefs.GetBool( editorPref ) );

		EditorGUILayout.EndHorizontal();

		if( EditorPrefs.GetBool( editorPref ) )
			collapsableSectionStyle.fontStyle = FontStyle.Normal;

		return EditorPrefs.GetBool( editorPref );
	}

	bool DisplayCollapsibleBoxSection ( string sectionTitle, string editorPref, SerializedProperty enabledProp, ref bool valueChanged )
	{
		if( EditorPrefs.GetBool( editorPref ) && enabledProp.boolValue )
			collapsableSectionStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.BeginHorizontal();

		EditorGUI.BeginChangeCheck();
		enabledProp.boolValue = EditorGUILayout.Toggle( enabledProp.boolValue, GUILayout.Width( 25 ) );
		if( EditorGUI.EndChangeCheck() )
		{
			serializedObject.ApplyModifiedProperties();

			if( enabledProp.boolValue )
				EditorPrefs.SetBool( editorPref, true );
			else
				EditorPrefs.SetBool( editorPref, false );

			valueChanged = true;
		}

		GUILayout.Space( -25 );

		EditorGUI.BeginDisabledGroup( !enabledProp.boolValue );
		if( GUILayout.Button( sectionTitle, collapsableSectionStyle ) )
			EditorPrefs.SetBool( editorPref, !EditorPrefs.GetBool( editorPref ) );
		EditorGUI.EndDisabledGroup();
		
		EditorGUILayout.EndHorizontal();

		if( EditorPrefs.GetBool( editorPref ) )
			collapsableSectionStyle.fontStyle = FontStyle.Normal;

		return EditorPrefs.GetBool( editorPref ) && enabledProp.boolValue;
	}

	void StoreReferences ()
	{
		targ = ( UltimateTouchpad )target;

		if( targ == null )
			return;

		isPrefabInProjectWindow = AssetDatabase.Contains( targ.gameObject );

		// TOUCHPAD POSITION //
		touchSizeWidth = serializedObject.FindProperty( "touchSizeWidth" );
		touchSizeHeight = serializedObject.FindProperty( "touchSizeHeight" );
		touchSizePositionX = serializedObject.FindProperty( "touchSizePositionX" );
		touchSizePositionY = serializedObject.FindProperty( "touchSizePositionY" );

		// SENSITIVITIES //
		screenWidthReference = serializedObject.FindProperty( "screenWidthReference" );
		xSensitivity = serializedObject.FindProperty( "xSensitivity" );
		ySensitivity = serializedObject.FindProperty( "ySensitivity" );
		xGravity = serializedObject.FindProperty( "xGravity" );
		yGravity = serializedObject.FindProperty( "yGravity" );
		
		// DISPLAY BASE //
		displayBase = serializedObject.FindProperty( "displayBase" );
		displayType = serializedObject.FindProperty( "displayType" );
		baseImage = serializedObject.FindProperty( "baseImage" );
		if( targ.baseImage != null )
			baseImageSprite = targ.baseImage.sprite;
		baseColor = serializedObject.FindProperty( "baseColor" );
		alphaEnabled = serializedObject.FindProperty( "alphaEnabled" );
		alphaDisabled = serializedObject.FindProperty( "alphaDisabled" );
		enabledDuration = serializedObject.FindProperty( "enabledDuration" );
		disabledDuration = serializedObject.FindProperty( "disabledDuration" );
		inactiveTime = serializedObject.FindProperty( "inactiveTime" );

		// DISPLAY INPUT //
		displayInput = serializedObject.FindProperty( "displayInput" );
		inputImage = serializedObject.FindProperty( "inputImage" );
		if( targ.inputImage != null )
			inputImageSprite = targ.inputImage.sprite;
		inputColor = serializedObject.FindProperty( "inputColor" );
		scaleReference = serializedObject.FindProperty( "scaleReference" );
		imageSize = serializedObject.FindProperty( "imageSize" );

		// TAP COUNT //
		tapCountOption = serializedObject.FindProperty( "tapCountOption" );
		tapCountDuration = serializedObject.FindProperty( "tapCountDuration" );
		targetTapCount = serializedObject.FindProperty( "targetTapCount" );

		// TOUCH INPUT //
		useTouchInput = serializedObject.FindProperty( "useTouchInput" );

		// SCRIPT REFERENCE //
		touchpadName = serializedObject.FindProperty( "touchpadName" );
		exampleCodeOptions = new List<string>();
		for( int i = 0; i < exampleCodes.Length; i++ )
			exampleCodeOptions.Add( exampleCodes[ i ].optionName );
	}

	public override void OnInspectorGUI ()
	{
		serializedObject.Update();
		
		collapsableSectionStyle = new GUIStyle( EditorStyles.label ) { alignment = TextAnchor.MiddleCenter };
		collapsableSectionStyle.active.textColor = collapsableSectionStyle.normal.textColor;
		
		bool valueChanged = false;

		// PREFAB WARNINGS //
		if( isPrefabInProjectWindow )
		{
			bool showGenerateWarning = false;

			if( targ.displayInput && targ.inputImage == null )
				showGenerateWarning = true;
			
			if( showGenerateWarning )
				EditorGUILayout.HelpBox( "Objects cannot be generated while selecting a Prefab within the Project window. Please make sure to drag this prefab into the scene before trying to generate objects.", MessageType.Warning );
		}

		if( CanvasErrors )
		{
			EditorGUILayout.LabelField( "Canvas Error", EditorStyles.boldLabel );
			if( !parentCanvas.enabled )
				EditorGUILayout.HelpBox( "The Ultimate Touchpad needs to be placed inside a Canvas object that is enabled in order to function correctly.", MessageType.Error );
			else
				EditorGUILayout.HelpBox( "The Ultimate Touchpad cannot be placed inside a World Space Canvas.", MessageType.Error );

			return;
		}
		
		// TOUCHPAD SETTINGS //
		DisplayHeaderDropdown( "Touchpad Settings", "UT_Settings" );
		if( EditorPrefs.GetBool( "UT_Settings" ) )
		{
			// POSITION //
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Touchpad Position", "UT_Position" ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( touchSizeWidth, 0.0f, 100.0f, new GUIContent( "Touch Width", "The width of the touch area." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					DisplayTouchWidth.PropertyUpdated();
				}
				CheckPropertyHover( DisplayTouchWidth );

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( touchSizeHeight, 0.0f, 100.0f, new GUIContent( "Touch Height", "The height of the touch area." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					DisplayTouchHeight.PropertyUpdated();
				}
				CheckPropertyHover( DisplayTouchHeight );

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( touchSizePositionX, 0.0f, 100.0f, new GUIContent( "Horizontal Position", "The horizontal position of the touch area." ) );
				EditorGUILayout.Slider( touchSizePositionY, 0.0f, 100.0f, new GUIContent( "Vertical Position", "The vertical position of the touch area." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndVertical();
			
			// SENSITIVITIES //
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Sensitivities", "UT_Sensitivities" ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( screenWidthReference, new GUIContent( "Screen Width Reference", "The reference screen width for the sensitivities. Using a higher value here will allow the horizontal and vertical axis to be larger values." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					if( screenWidthReference.floatValue < 0 )
						screenWidthReference.floatValue = 0;

					serializedObject.ApplyModifiedProperties();
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Slider( xSensitivity, 1.0f, 10.0f, new GUIContent( "X Sensitivity", "The horizontal sensitivity of the touchpad." ) );
				EditorGUILayout.Slider( ySensitivity, 1.0f, 10.0f, new GUIContent( "Y Sensitivity", "The vertical sensitivity of the touchpad." ) );
				EditorGUILayout.Slider( xGravity, 0.01f, 1.0f, new GUIContent( "X Gravity", "Determines how much gravity should be applied to the horizontal value." ) );
				EditorGUILayout.Slider( yGravity, 0.01f, 1.0f, new GUIContent( "Y Gravity", "Determines how much gravity should be applied to the vertical value." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndVertical();
			
			// DISPLAY BASE //
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Display Base", "UT_DisplayBase", displayBase, ref valueChanged ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( baseImage, new GUIContent( "Base Image", "The image component to be used for the base." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					if( targ.baseImage != null )
					{
						// EDIT: Need to copy some values.
						baseImageSprite = targ.baseImage.sprite;

						baseColor.colorValue = targ.baseImage.color;
						serializedObject.ApplyModifiedProperties();

						CheckBaseImageSettings();
					}
				}

				EditorGUI.BeginChangeCheck();
				baseImageSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Image Sprite", baseImageSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
				if( EditorGUI.EndChangeCheck() )
				{
					if( targ.baseImage != null )
					{
						Undo.RecordObject( targ.baseImage, "Update Base Image Sprite" );
						targ.baseImage.sprite = baseImageSprite;
					}
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( baseColor, new GUIContent( "Base Color", "The color of the base image." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					alphaEnabled.floatValue = baseColor.colorValue.a;
					serializedObject.ApplyModifiedProperties();

					if( targ.baseImage != null )
					{
						Undo.RecordObject( targ.baseImage, "Update Display Base" );
						targ.baseImage.color = baseColor.colorValue;
					}
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( displayType, new GUIContent( "Display Type", "Determines how the base of the touchpad should be displayed, if at all." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				if( targ.displayType != UltimateTouchpad.DisplayType.Always )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider( alphaEnabled, 0.0f, 1.0f, new GUIContent( "Active Alpha", "The target alpha when the input is active." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						Color tempColor = baseColor.colorValue;
						tempColor.a = alphaEnabled.floatValue;
						baseColor.colorValue = tempColor;

						serializedObject.ApplyModifiedProperties();

						if( targ.baseImage != null )
						{
							Undo.RecordObject( targ.baseImage, "Update Display Alpha" );
							targ.baseImage.color = baseColor.colorValue;
						}
					}

					EditorGUI.BeginChangeCheck();
					EditorGUILayout.Slider( alphaDisabled, 0.0f, 1.0f, new GUIContent( "Inactive Alpha", "The target alpha when the input is inactive." ) );
					EditorGUILayout.PropertyField( enabledDuration, new GUIContent( "Enable Duration", "Time in seconds for the alpha to reach the active alpha." ) );
					EditorGUILayout.PropertyField( disabledDuration, new GUIContent( "Disable Duration", "Time in seconds for the alpha to reach the inactive alpha." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						if( enabledDuration.floatValue < 0 )
							enabledDuration.floatValue = 0;

						if( disabledDuration.floatValue < 0 )
							disabledDuration.floatValue = 0;

						serializedObject.ApplyModifiedProperties();
					}
				}

				if( targ.displayType == UltimateTouchpad.DisplayType.Inactivity )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( inactiveTime, new GUIContent( "Inactive Time", "Time in seconds for the touchpad to be inactive before enabling it's visuals." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						if( inactiveTime.floatValue < 0 )
							inactiveTime.floatValue = 0;

						serializedObject.ApplyModifiedProperties();
					}
				}
			}
			EditorGUILayout.EndVertical();
			if( valueChanged )
			{
				if( targ.displayBase )
				{
					if( targ.baseImage == null )
					{
						if( targ.gameObject.GetComponent<Image>() )
						{
							baseImage.objectReferenceValue = targ.gameObject.GetComponent<Image>();
							baseColor.colorValue = targ.GetComponent<Image>().color;
							serializedObject.ApplyModifiedProperties();
						}
						else if( !isPrefabInProjectWindow )
						{
							targ.gameObject.AddComponent<CanvasRenderer>();
							Image img = targ.gameObject.AddComponent<Image>();

							targ.gameObject.GetComponent<Image>().sprite = baseImageSprite;
							targ.gameObject.GetComponent<Image>().color = targ.baseColor;

							serializedObject.FindProperty( "baseImage" ).objectReferenceValue = img;
							serializedObject.ApplyModifiedProperties();

							Undo.RegisterCreatedObjectUndo( img, "Create Base Image Object" );
						}

						CheckBaseImageSettings();
					}

					if( targ.baseImage != null )
					{
						Undo.RecordObject( targ.baseImage, "Enable Base Image" );
						targ.baseImage.color = targ.baseColor;
					}
				}
				else
				{
					if( targ.baseImage != null )
					{
						Undo.RecordObject( targ.baseImage, "Disable Base Image" );
						targ.baseImage.color = Color.clear;
					}
				}
			}

			// DISPLAY INPUT //
			valueChanged = false;
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Display Input", "UT_DisplayInput", displayInput, ref valueChanged ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( inputImage, new GUIContent( "Input Image", "The image component to be used for the users input." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					if( targ.inputImage != null )
					{

						inputColor.colorValue = targ.inputImage.color;
						serializedObject.ApplyModifiedProperties();
					}
				}
				
				EditorGUI.BeginChangeCheck();
				inputImageSprite = ( Sprite )EditorGUILayout.ObjectField( "└ Image Sprite", inputImageSprite, typeof( Sprite ), true, GUILayout.Height( EditorGUIUtility.singleLineHeight ) );
				if( EditorGUI.EndChangeCheck() )
				{
					if( targ.inputImage != null )
					{
						Undo.RecordObject( targ.inputImage, "Update Input Image Sprite" );
						targ.inputImage.sprite = inputImageSprite;
					}
				}

				if( targ.inputImage == null && !isPrefabInProjectWindow )
				{
					EditorGUI.BeginDisabledGroup( inputImageSprite == null || Application.isPlaying );
					if( GUILayout.Button( "Generate Input Image", EditorStyles.miniButton ) )
					{
						GameObject newGameObject = new GameObject();
						newGameObject.AddComponent<RectTransform>();
						newGameObject.AddComponent<CanvasRenderer>();
						newGameObject.AddComponent<Image>();

						newGameObject.GetComponent<Image>().sprite = inputImageSprite;
						newGameObject.GetComponent<Image>().color = targ.inputColor;

						newGameObject.transform.SetParent( targ.transform );
						newGameObject.transform.SetAsFirstSibling();

						newGameObject.name = "Input Image";

						RectTransform trans = newGameObject.GetComponent<RectTransform>();

						trans.anchorMin = new Vector2( 0.5f, 0.5f );
						trans.anchorMax = new Vector2( 0.5f, 0.5f );
						trans.offsetMin = Vector2.zero;
						trans.offsetMax = Vector2.zero;
						trans.pivot = new Vector2( 0.5f, 0.5f );
						trans.anchoredPosition = Vector2.zero;
						trans.localScale = Vector3.one;
						trans.localPosition = Vector3.zero;
						trans.localRotation = Quaternion.identity;

						serializedObject.FindProperty( "inputImage" ).objectReferenceValue = newGameObject.GetComponent<Image>();
						serializedObject.ApplyModifiedProperties();

						Undo.RegisterCreatedObjectUndo( newGameObject, "Create Input Image Object" );
					}
					EditorGUI.EndDisabledGroup();
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( inputColor, new GUIContent( "Input Color", "The color of the input image." ) );
				if( EditorGUI.EndChangeCheck() )
				{
					serializedObject.ApplyModifiedProperties();

					if( targ.inputImage != null )
					{
						Undo.RecordObject( targ.inputImage, "Input Image Color" );
						targ.inputImage.color = targ.inputColor;
					}
				}

				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( scaleReference, new GUIContent( "Scale Reference", "Determines which screen axis should be used for scale reference." ) );
				EditorGUILayout.Slider( imageSize, 0.0f, 1.0f, new GUIContent( "Image Size", "The size of the input image." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();
			}
			EditorGUILayout.EndVertical();
			if( valueChanged )
			{
				if( targ.inputImage != null )
				{
					Undo.RecordObject( targ.inputImage.gameObject, ( targ.displayInput ? "Enable" : "Disable" ) + " Display Input" );
					targ.inputImage.gameObject.SetActive( targ.displayInput );
				}
			}

			// TAP COUNT //
			EditorGUILayout.BeginVertical( "Box" );
			if( DisplayCollapsibleBoxSection( "Tap Calculations", "UT_TapCount" ) )
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField( tapCountOption, new GUIContent( "Tap Count", "Determines whether or not the amount of taps on the touchpad should be calculated." ) );
				if( EditorGUI.EndChangeCheck() )
					serializedObject.ApplyModifiedProperties();

				if( targ.tapCountOption == UltimateTouchpad.TapCountOption.Accumulate )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( tapCountDuration, new GUIContent( "Time Window", "Determines how much time the user has to achieve the tap count." ) );
					EditorGUILayout.PropertyField( targetTapCount, new GUIContent( "Target Tap Count", "The target amount of taps for the user to reach before achieving the tap count." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						if( tapCountDuration.floatValue < 0.0f )
							tapCountDuration.floatValue = 0.0f;
						if( targetTapCount.intValue < 0 )
							targetTapCount.intValue = 0;

						serializedObject.ApplyModifiedProperties();
					}
				}

				if( targ.tapCountOption == UltimateTouchpad.TapCountOption.TouchRelease )
				{
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.PropertyField( tapCountDuration, new GUIContent( "Time Window", "Determines how much time the user has to release the input to achieve the tap count." ) );
					if( EditorGUI.EndChangeCheck() )
					{
						if( tapCountDuration.floatValue < 0.0f )
							tapCountDuration.floatValue = 0.0f;

						serializedObject.ApplyModifiedProperties();
					}
				}
			}
			EditorGUILayout.EndVertical();

			// TOUCH INPUT //
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( useTouchInput, new GUIContent( "Use Touch Input", "Determines if the touchpad should use input from the EventSystem or directly calculate from the touch input on the screen." ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			if( targ.useTouchInput )
				EditorGUILayout.HelpBox( "The Ultimate Touchpad will exclusively use touch input received on the screen for calculations.", MessageType.Warning );
		}
		
		// SCRIPT REFERENCE //
		DisplayHeaderDropdown( "Script Reference", "UUI_ScriptReference" );
		if( EditorPrefs.GetBool( "UUI_ScriptReference" ) )
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField( touchpadName, new GUIContent( "Touchpad Name", "The name for this Ultimate Touchpad to be registered as." ) );
			if( EditorGUI.EndChangeCheck() )
				serializedObject.ApplyModifiedProperties();

			if( targ.touchpadName == string.Empty )
			{
				EditorGUILayout.HelpBox( "Please assign a Touchpad Name in order to be able to get this touchpad's position dynamically.", MessageType.Warning );
			}
			else
			{
				EditorGUILayout.BeginVertical( "Box" );
				GUILayout.Space( 1 );
				EditorGUILayout.LabelField( "Example Code Generator", EditorStyles.boldLabel );

				exampleCodeIndex = EditorGUILayout.Popup( "Function", exampleCodeIndex, exampleCodeOptions.ToArray() );

				EditorGUILayout.LabelField( "Function Description", EditorStyles.boldLabel );
				GUIStyle wordWrappedLabel = new GUIStyle( GUI.skin.label ) { wordWrap = true };
				EditorGUILayout.LabelField( exampleCodes[ exampleCodeIndex ].optionDescription, wordWrappedLabel );

				EditorGUILayout.LabelField( "Example Code", EditorStyles.boldLabel );
				GUIStyle wordWrappedTextArea = new GUIStyle( GUI.skin.textArea ) { wordWrap = true };
				EditorGUILayout.TextArea( string.Format( exampleCodes[ exampleCodeIndex ].basicCode, touchpadName.stringValue ), wordWrappedTextArea );

				GUILayout.Space( 1 );
				EditorGUILayout.EndVertical();

				if( GUILayout.Button( "Open Documentation" ) )
					UltimateTouchpadReadmeEditor.OpenReadmeDocumentation();
			}
		}

		// DEVELOPMENT MODE //
		if( EditorPrefs.GetBool( "UUI_DevelopmentMode" ) )
		{
			EditorGUILayout.Space();
			GUIStyle toolbarStyle = new GUIStyle( EditorStyles.toolbarButton ) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 11, richText = true };
			GUILayout.BeginHorizontal();
			GUILayout.Space( -10 );
			showDefaultInspector = GUILayout.Toggle( showDefaultInspector, ( showDefaultInspector ? "▼ " : "► " ) + "<color=#ff0000ff>Development Inspector</color>", toolbarStyle );
			GUILayout.EndHorizontal();
			if( showDefaultInspector )
			{
				EditorGUILayout.Space();

				base.OnInspectorGUI();
			}
		}

		EditorGUILayout.Space();

		Repaint();
	}

	void CheckPropertyHover ( DisplaySceneGizmo displaySceneGizmo )
	{
		displaySceneGizmo.hover = false;
		var rect = GUILayoutUtility.GetLastRect();
		if( Event.current.type == EventType.Repaint && rect.Contains( Event.current.mousePosition ) )
			displaySceneGizmo.hover = true;
	}

	void CheckBaseImageSettings ()
	{
		if( targ != null && targ.baseImage != null )
		{
			Undo.RecordObject( targ.baseImage, "Update Base Image" );
			targ.baseImage.type = Image.Type.Sliced;
			targ.baseImage.fillCenter = true;
		}
	}

	void OnSceneGUI ()
	{
		if( targ == null || Selection.activeGameObject == null || Application.isPlaying || Selection.objects.Length > 1 || parentCanvas == null )
			return;
		
		Vector3 canvasScale = parentCanvas.transform.localScale;

		RectTransform trans = targ.transform.GetComponent<RectTransform>();

		if( trans == null )
			return;

		Vector3 transCenter = trans.position;
		float actualSize = trans.sizeDelta.x * canvasScale.x;
		float halfSize = ( actualSize / 2 ) - ( actualSize / 20 );

		Handles.color = colorValueChanged;
		DisplayTouchWidth.frames++;
		DisplayTouchHeight.frames++;

		if( EditorPrefs.GetBool( "UT_Settings" ) && EditorPrefs.GetBool( "UT_Position" ) )
		{
			if( DisplayTouchWidth.HighlightGizmo )
			{
				Handles.DrawLine( trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMin, trans.rect.yMax ) ), trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMin, trans.rect.yMin ) ) );
				Handles.DrawLine( trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMax, trans.rect.yMax ) ), trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMax, trans.rect.yMin ) ) );
			}
			
			if( DisplayTouchHeight.HighlightGizmo )
			{
				Handles.DrawLine( trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMin, trans.rect.yMax ) ), trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMax, trans.rect.yMax ) ) );
				Handles.DrawLine( trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMin, trans.rect.yMin ) ), trans.TransformPoint( trans.rect.center + new Vector2( trans.rect.xMax, trans.rect.yMin ) ) );
			}
		}

		SceneView.RepaintAll();
	}

	// ---------------------------------< CANVAS CREATOR FUNCTIONS >--------------------------------- //
	static void CreateNewCanvas ( GameObject child )
	{
		GameObject canvasObject = new GameObject( "Ultimate UI Canvas" );
		canvasObject.layer = LayerMask.NameToLayer( "UI" );
		Canvas canvas = canvasObject.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvasObject.AddComponent<GraphicRaycaster>();
		canvasObject.AddComponent<CanvasScaler>();
		Undo.RegisterCreatedObjectUndo( canvasObject, "Create " + canvasObject.name );
		Undo.SetTransformParent( child.transform, canvasObject.transform, "Request Joystick Canvas" );
		CreateEventSystem();
	}

	static void CreateEventSystem ()
	{
		Object esys = Object.FindObjectOfType<EventSystem>();
		if( esys == null )
		{
			GameObject eventSystem = new GameObject( "EventSystem" );
			esys = eventSystem.AddComponent<EventSystem>();
			eventSystem.AddComponent<StandaloneInputModule>();
			Undo.RegisterCreatedObjectUndo( eventSystem, "Create " + eventSystem.name );
		}
	}

	public static void RequestCanvas ( GameObject child )
	{
		Canvas[] allCanvas = Object.FindObjectsOfType( typeof( Canvas ) ) as Canvas[];

		for( int i = 0; i < allCanvas.Length; i++ )
		{
			if( allCanvas[ i ].enabled == true && allCanvas[ i ].renderMode != RenderMode.WorldSpace )
			{
				Undo.SetTransformParent( child.transform, allCanvas[ i ].transform, "Request Canvas" );
				CreateEventSystem();
				return;
			}
		}
		CreateNewCanvas( child );
	}
	// -------------------------------< END CANVAS CREATOR FUNCTIONS >------------------------------- //
}