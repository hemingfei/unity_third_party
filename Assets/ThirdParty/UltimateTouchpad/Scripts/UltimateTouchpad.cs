/* Written by Kaz Crowe */
/* UltimateTouchpad.cs */
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[ExecuteInEditMode]
public class UltimateTouchpad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	// INTERNAL //
	RectTransform baseTrans;
	int _inputId = -10;
	bool inputActive = false;
	Rect touchpadRect;
	Vector2 previousPosition, currentPosition;
	float horizontalValue = 0.0f, verticalValue = 0.0f;
	float screenWidth = 0.0f, screenHeight = 0.0f;
	public Canvas ParentCanvas
	{
		get;
		private set;
	}
	RectTransform canvasRectTrans;

	// TOUCHPAD SETTINGS //
	public float touchSizeWidth = 50.0f, touchSizeHeight = 50.0f;
	public float touchSizePositionX = 0.0f, touchSizePositionY = 0.0f;
	// Sensitivities //
	public float screenWidthReference = 1280.0f;
	public float xSensitivity = 10.0f, ySensitivity = 2.5f;
	float _xSensitivity = 0.0f, _ySensitivity = 0.0f;
	public float xGravity = 0.5f, yGravity = 0.5f;
	// Display Base //
	public bool displayBase = false;
	public enum DisplayType
	{
		OnInput,
		Inactivity,
		Always
	}
	public DisplayType displayType = DisplayType.OnInput;
	public Image baseImage;
	public Color baseColor = Color.white;
	Color invisibleBaseColor;
	public float alphaEnabled = 1.0f, alphaDisabled = 0.0f;
	public float enabledDuration = 1.0f, disabledDuration = 1.0f;
	float enabledSpeed = 1.0f, disabledSpeed = 1.0f;
	public float inactiveTime = 5.0f;
	// Display Input //
	public bool displayInput;
	public Image inputImage;
	public Color inputColor = Color.white;
	Color invisibleInputColor;
	public enum ScaleReference
	{
		Height,
		Width
	}
	public ScaleReference scaleReference = ScaleReference.Width;
	public float imageSize = 0.5f;
	// Tap Count //
	public enum TapCountOption
	{
		NoCount,
		Accumulate,
		TouchRelease
	}
	public TapCountOption tapCountOption = TapCountOption.NoCount;
	public float tapCountDuration = 0.5f;
	public int targetTapCount = 2;
	bool tapCountAchieved = false;
	float currentTapTime = 0.0f;
	int tapCount = 0;
	// Touch Input //
	public bool useTouchInput = false;

	// SCRIPT REFERENCE //
	static Dictionary<string, UltimateTouchpad> UltimateTouchpads = new Dictionary<string, UltimateTouchpad>();
	public string touchpadName = "";


	void OnEnable ()
	{
		// If the user wants to calculate using touch input, then start the coroutine to catch the input.
		if( Application.isPlaying && useTouchInput )
			StartCoroutine( ProcessTouchInput() );
	}

	void OnDisable ()
	{
		// If the users was wanting to use touch input, then stop the coroutine.
		if( Application.isPlaying && useTouchInput )
			StopCoroutine( ProcessTouchInput() );
	}

	void Awake ()
	{
		// If the game is not being run and the touchpad name has been assigned...
		if( Application.isPlaying && touchpadName != string.Empty )
		{
			// If the static dictionary has this touchpad registered, then remove it from the list.
			if( UltimateTouchpads.ContainsKey( touchpadName ) )
				UltimateTouchpads.Remove( touchpadName );

			// Then register the touchpad.
			UltimateTouchpads.Add( touchpadName, GetComponent<UltimateTouchpad>() );
		}
	}

	void Start ()
	{
		// If the game is not running then return.
		if( !Application.isPlaying )
			return;

		// Configure the sensitivities.
		_xSensitivity = xSensitivity * screenWidthReference;
		_ySensitivity = ySensitivity * screenWidthReference;

		// If the parent canvas is null...
		if( ParentCanvas == null )
		{
			// Then try to get the parent canvas component.
			UpdateParentCanvas();

			// If it is still null, then log a error and return.
			if( ParentCanvas == null )
			{
				Debug.LogError( "Ultimate Touchpad\nThis component is not with a Canvas object. Disabling this component to avoid any errors." );
				enabled = false;
				return;
			}
		}

		// If the parent canvas does not have a screen size updater, then add it.
		if( !ParentCanvas.GetComponent<UltimateTouchpadScreenSizeUpdater>() )
			ParentCanvas.gameObject.AddComponent<UltimateTouchpadScreenSizeUpdater>();

		// Update the size and placement of the touchpad.
		UpdateTouchpadPositioning();

		// If the user wants to display input, and this image variable is assigned...
		if( displayInput && inputImage != null )
		{
			// Configure the disabled color.
			invisibleInputColor = inputColor;
			invisibleInputColor.a = 0.0f;
			inputImage.color = invisibleInputColor;
		}

		// If the user is wanting to use fade...
		if( displayBase )
		{
			// If the base image is null...
			if( baseImage == null )
			{
				// Send a log to the console for the user.
				Debug.LogWarning( "Ultimate Touchpad\nThe Base Image is unassigned. Setting Display Base to false to avoid errors. Please assign the Base Image component in the inspector." );

				// Set display base option to false to avoid errors and return.
				displayBase = false;
			}
			else
			{
				// Configure the fade speeds.
				enabledSpeed = 1.0f / enabledDuration;
				disabledSpeed = 1.0f / disabledDuration;

				// Assigned the disabled color.
				invisibleBaseColor = baseColor;
				invisibleBaseColor.a = alphaDisabled;

				// Apply the initial values according to the users options.
				if( displayType == DisplayType.OnInput )
					baseImage.color = invisibleBaseColor;
				else
					baseImage.color = baseColor;
			}
		}
	}

	// THIS IS FOR THE UNITY EVENT SYSTEM IF THE USER WANTS THAT //
	public void OnPointerDown ( PointerEventData touchInfo )
	{
		if( useTouchInput )
			return;

		ProcessOnInputDown( touchInfo.position, touchInfo.pointerId );
	}

	public void OnDrag ( PointerEventData touchInfo )
	{
		if( useTouchInput )
			return;

		ProcessOnInputMoved( touchInfo.position, touchInfo.pointerId );
	}

	public void OnPointerUp ( PointerEventData touchInfo )
	{
		if( useTouchInput )
			return;

		ProcessOnInputUp( touchInfo.position, touchInfo.pointerId );
	}
	// END FOR UNITY EVENT SYSTEM //

	/// <summary>
	/// The coroutine will process the touch input if the user has the useTouchInput boolean enabled.
	/// </summary>
	IEnumerator ProcessTouchInput ()
	{
		// Loop for as long as useTouchInput is true.
		while( useTouchInput )
		{
			// If there are touches on the screen...
			if( Input.touchCount > 0 )
			{
				// Loop through each finger on the screen...
				for( int fingerId = 0; fingerId < Input.touchCount; fingerId++ )
				{
					// If the input phase has begun...
					if( Input.GetTouch( fingerId ).phase == TouchPhase.Began )
					{
						// If the touch input position is within the bounds of the rect, then process the down input on the touchpad.
						if( touchpadRect.Contains( Input.GetTouch( fingerId ).position ) )
							ProcessOnInputDown( Input.GetTouch( fingerId ).position, fingerId );
					}
					// Else if the input has moved, then process the moved input.
					else if( Input.GetTouch( fingerId ).phase == TouchPhase.Moved )
						ProcessOnInputMoved( Input.GetTouch( fingerId ).position, fingerId );
					// Else if the input has ended or if it was canceled, then process the input being released.
					else if( Input.GetTouch( fingerId ).phase == TouchPhase.Ended || Input.GetTouch( fingerId ).phase == TouchPhase.Canceled )
						ProcessOnInputUp( Input.GetTouch( fingerId ).position, fingerId );
				}
			}
			// Else there are no touches on the screen.
			else
			{
				// If the inputId is not reset then reset the touchpad since there are no touches.
				if( _inputId > -10 )
					ResetTouchpad();
			}

			yield return null;
		}
	}

	void ProcessOnInputDown ( Vector2 inputPosition, int inputId )
	{
		// If this is already being interacted with, then return.
		if( inputActive )
			return;

		// Set the state to true and assign the current pointer ID.
		inputActive = true;
		_inputId = inputId;

		// Since this is the initial touch, assign the position values.
		currentPosition = inputPosition;
		previousPosition = currentPosition;

		// Start processing the input.
		StartCoroutine( "ProcessInput" );

		// If the user wants to see the input position, set the image to visible.
		if( displayInput && inputImage != null )
			inputImage.color = inputColor;

		// If the user wants to display the base on user input, do that here.
		if( displayBase && displayType == DisplayType.OnInput )
			StartCoroutine( "DisplayBaseOnInputLogic" );
		else if( displayBase && displayType == DisplayType.Inactivity )
			StartCoroutine( "DisplayBaseInactivityLogic" );

		// If the user is wanting to use any tap count...
		if( tapCountOption != TapCountOption.NoCount )
		{
			// If the user is accumulating taps...
			if( tapCountOption == TapCountOption.Accumulate )
			{
				// If the TapCountdown is not counting down...
				if( currentTapTime <= 0 )
				{
					// Set tapCount to 1 since this is the initial touch and start the TapCountdown.
					tapCount = 1;
					StartCoroutine( "TapCountdown" );
				}
				// Else the TapCountdown is currently counting down, so increase the current tapCount.
				else
					++tapCount;

				if( currentTapTime > 0 && tapCount >= targetTapCount )
				{
					// Set the current time to 0 to interrupt the coroutine.
					currentTapTime = 0;

					// Start the delay of the reference for one frame.
					StartCoroutine( "TapCountDelay" );
				}
			}
			// Else the user wants to touch and release, so start the TapCountdown timer.
			else
				StartCoroutine( "TapCountdown" );
		}
	}

	void ProcessOnInputMoved ( Vector2 inputPosition, int inputId )
	{
		// If the pointer ID is not the one that initiated, then return.
		if( inputId != _inputId )
			return;

		// Catch the current position of the input.
		currentPosition = inputPosition;
	}

	void ProcessOnInputUp ( Vector2 inputPosition, int inputId )
	{
		// If the pointer ID is not the one that initiated, then return.
		if( inputId != _inputId )
			return;
		
		// Reset state and _pointerId to default.
		inputActive = false;
		_inputId = -10;

		// Reset the horizontal and vertical values.
		horizontalValue = 0.0f;
		verticalValue = 0.0f;

		// If the user is wanting the input position visible, then disable that visually here.
		if( displayInput && inputImage != null )
			inputImage.color = invisibleInputColor;

		// If the user is wanting to use the TouchRelease tap count...
		if( tapCountOption == TapCountOption.TouchRelease )
		{
			// If the tapTime is still above zero, then start the delay function.
			if( currentTapTime > 0 )
				StartCoroutine( "TapCountDelay" );

			// Reset the current tap time to zero.
			currentTapTime = 0;
		}
	}
	
	/// <summary>
	/// Coroutine to run more often than the OnDrag function so that gravity can be applied.
	/// </summary>
	IEnumerator ProcessInput ()
	{
		// While the input is active...
		while( inputActive )
		{
			// Update the touchpad.
			UpdateTouchPad();
			yield return null;
		}
	}

	/// <summary>
	/// Updates the Touchpad horizontal and vertical values for reference.
	/// </summary>
	void UpdateTouchPad ()
	{
		// Configure the actual distance from the previous position.
		Vector2 actualValue = ( currentPosition - previousPosition ) / ParentCanvas.scaleFactor;

		// Modify the values to be consistent according to the base sensitivity.
		actualValue.x = actualValue.x / screenWidth * _xSensitivity;
		actualValue.y = actualValue.y / screenHeight * _ySensitivity;
		
		// Configure the horizontal and vertical values according to the users set gravity.
		horizontalValue = Mathf.Lerp( horizontalValue, actualValue.x, xGravity );
		verticalValue = Mathf.Lerp( verticalValue, actualValue.y, yGravity );

		// Since the values have been configured, set the previous position to be current.
		previousPosition = currentPosition;
		
		// If the user wants to display the input, then set the image's position.
		if( displayInput && inputImage != null )
			inputImage.rectTransform.localPosition = ( Vector2 )baseTrans.InverseTransformPoint( ParentCanvas.transform.TransformPoint( currentPosition / ParentCanvas.scaleFactor ) ) - ( canvasRectTrans.sizeDelta / 2 );
	}
	
	/// <summary>
	///  Confirms whether or not a Touchpad has been registered with the touchpadName string.
	/// </summary>
	static bool TouchpadConfirmed ( string touchPadName )
	{
		// If the dictionary does not contain this touchpadName...
		if( !UltimateTouchpads.ContainsKey( touchPadName ) )
		{
			// Warn the user.
			Debug.LogWarning( "No Ultimate Touch Pad has been registered with the name: " + touchPadName + "." );
			return false;
		}
		return true;
	}

	/// <summary>
	/// Coroutine to fade the image when the user interacts with the Touchpad.
	/// </summary>
	IEnumerator DisplayBaseOnInputLogic ()
	{
		// Store the current color.
		Color currentColor = baseImage.color;

		// If the enabledSpeed is NaN, then just apply the base color.
		if( float.IsInfinity( enabledSpeed ) )
			baseImage.color = baseColor;
		// Else run the loop to fade to the desired alpha over time.
		else
		{
			for( float fadeIn = 0.0f; fadeIn < 1.0f && inputActive; fadeIn += Time.deltaTime * enabledSpeed )
			{
				baseImage.color = Color.Lerp( currentColor, baseColor, fadeIn );
				yield return null;
			}
			if( inputActive )
				baseImage.color = baseColor;
		}

		// while loop for while state is true
		while( inputActive )
			yield return null;

		// Set the current fade value.
		currentColor = baseImage.color;

		// If the disabledSpeed value is NaN, then apply the disabled color.
		if( float.IsInfinity( disabledSpeed ) )
			baseImage.color = invisibleBaseColor;
		// Else run the loop for fading out.
		else
		{
			for( float fadeOut = 0.0f; fadeOut < 1.0f && !inputActive; fadeOut += Time.deltaTime * disabledSpeed )
			{
				baseImage.color = Color.Lerp( currentColor, invisibleBaseColor, fadeOut );
				yield return null;
			}
			if( !inputActive )
				baseImage.color = invisibleBaseColor;
		}
	}

	/// <summary>
	/// Coroutine to fade the image when the input has been inactive.
	/// </summary>
	IEnumerator DisplayBaseInactivityLogic ()
	{
		// Set the current fade value.
		Color currentColor = baseImage.color;

		// If the disabledSpeed value is NaN, then apply the disabled color.
		if( float.IsInfinity( disabledSpeed ) )
			baseImage.color = invisibleBaseColor;
		// Else run the loop for fading out.
		else
		{
			for( float fadeOut = 0.0f; fadeOut < 1.0f && inputActive; fadeOut += Time.deltaTime * disabledSpeed )
			{
				baseImage.color = Color.Lerp( currentColor, invisibleBaseColor, fadeOut );
				yield return null;
			}
			baseImage.color = invisibleBaseColor;
		}

		// While the input is down, stay here.
		while( inputActive )
			yield return null;

		// Create a float to hold the current inactive time.
		float currentInactiveTime = 0.0f;

		// Loop for as long as the current inactive time is less than the target, or until the input is active again.
		while( currentInactiveTime < inactiveTime && !inputActive )
		{
			// Increase the inactive time and return.
			currentInactiveTime += Time.deltaTime;
			yield return null;
		}

		// Store the current color of the base.
		currentColor = baseImage.color;

		// If the enabledSpeed is NaN, then just set the color to the base color.
		if( float.IsInfinity( enabledSpeed ) )
			baseImage.color = baseColor;
		// Else run the loop to fade to the desired alpha over time.
		else
		{
			for( float fadeIn = 0.0f; fadeIn < 1.0f && !inputActive; fadeIn += Time.deltaTime * enabledSpeed )
			{
				baseImage.color = Color.Lerp( currentColor, baseColor, fadeIn );
				yield return null;
			}

			// If the state is still inactive, then apply the final color.
			if( inputActive == false )
				baseImage.color = baseColor;
		}
	}

	/// <summary>
	/// This function delays for one frame so that it can be correctly referenced as soon as it is achieved.
	/// </summary>
	IEnumerator TapCountDelay ()
	{
		tapCountAchieved = true;
		yield return new WaitForEndOfFrame();
		tapCountAchieved = false;
	}

	/// <summary>
	/// This function counts down the tap count duration. The current tap time that is being modified is check within the input functions.
	/// </summary>
	IEnumerator TapCountdown ()
	{
		// Set the current tap time to the max.
		currentTapTime = tapCountDuration;
		while( currentTapTime > 0 )
		{
			// Reduce the current time.
			currentTapTime -= Time.deltaTime;
			yield return null;
		}
	}

	/// <summary>
	/// Resets the variables to allow the Touchpad to function correctly.
	/// </summary>
	void ResetTouchpad ()
	{
		// Reset state and _pointerId to default.
		inputActive = false;
		_inputId = -10;

		// Reset the horizontal and vertical values.
		horizontalValue = 0.0f;
		verticalValue = 0.0f;

		// If the user is wanting the input position visible, then disable that visually here.
		if( displayInput && inputImage != null )
			inputImage.color = invisibleInputColor;

		// If the user is wanting to calculate taps, then reset the tap count variables.
		if( tapCountOption != TapCountOption.NoCount )
		{
			currentTapTime = 0;
			tapCount = 0;
		}

		// If the user wants to display the base when the input is inactive...
		if( displayBase && displayType == DisplayType.Inactivity )
		{
			// Stop the coroutine for the inactivity logic since the touchpad has been reset.
			StopCoroutine( "DisplayBaseInactivityLogic" );

			// Reset the color.
			baseImage.color = baseColor;
		}
		// Else if the user wants to display the base with the input states...
		else if( displayBase && displayType == DisplayType.OnInput )
		{
			// Then stop the input logic coroutine since the touchpad is being reset.
			StopCoroutine( "DisplayBaseOnInputLogic" );

			// Reset the color.
			baseImage.color = invisibleBaseColor;
		}
	}

	/// <summary>
	/// This function is called by Unity when the parent of this transform changes.
	/// </summary>
	void OnTransformParentChanged ()
	{
		UpdateParentCanvas();
	}

	/// <summary>
	/// This function is called by Unity when the application changes focus.
	/// </summary>
	void OnApplicationFocus ( bool focus )
	{
		if( !Application.isPlaying || !inputActive || !focus )
			return;

		ResetTouchpad();
	}

	/// <summary>
	/// Updates the parent canvas if it has changed.
	/// </summary>
	public void UpdateParentCanvas ()
	{
		// Store the parent of this object.
		Transform parent = transform.parent;

		// If the parent is null, then just return.
		if( parent == null )
			return;

		// While the parent is assigned...
		while( parent != null )
		{
			// If the parent object has a Canvas component, then assign the ParentCanvas and transform.
			if( parent.transform.GetComponent<Canvas>() && parent.transform.GetComponent<CanvasScaler>())
			{
				ParentCanvas = parent.transform.GetComponent<Canvas>();
				canvasRectTrans = ParentCanvas.GetComponent<RectTransform>();
				return;
			}

			// If the parent does not have a canvas, then store it's parent to loop again.
			parent = parent.transform.parent;
		}
		
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			UpdateParentCanvasInEditorPrefab();
		}
#endif
	}
	
	public void UpdateParentCanvasInEditorPrefab ()
	{
		// Store the parent of this object.
		Transform parent = transform.parent;

		// If the parent is null, then just return.
		if( parent == null )
			return;

		// While the parent is assigned...
		while( parent != null )
		{
			// If the parent object has a Canvas component, then assign the ParentCanvas and transform.
			if( parent.transform.GetComponent<Canvas>())
			{
				ParentCanvas = parent.transform.GetComponent<Canvas>();
				canvasRectTrans = ParentCanvas.GetComponent<RectTransform>();
			}

			// If the parent does not have a canvas, then store it's parent to loop again.
			parent = parent.transform.parent;
		}
	}

	/// <summary>
	/// Updates the transforms to be the correct size and position on the screen.
	/// </summary>
	void UpdateTouchpadPositioning ()
	{
		// If the parent canvas is null, then try to get the parent canvas component.
		if( ParentCanvas == null )
			UpdateParentCanvas();

		// If it is still null, then log a error and return.
		if( ParentCanvas == null )
		{
			Debug.LogError( "Ultimate Touchpad\nThere is no parent canvas object. Please make sure that the Ultimate Touchpad is placed within a canvas." );
			return;
		}

		// If baseTrans is null, store this object's RectTrans so that it can be positioned.
		if( baseTrans == null )
			baseTrans = GetComponent<RectTransform>();

		// Force the anchors and pivot so the touchpad will function correctly. This is also needed here for older versions of the Ultimate Touchpad that didn't use these rect transform settings.
		baseTrans.anchorMin = Vector2.zero;
		baseTrans.anchorMax = Vector2.zero;
		baseTrans.pivot = new Vector2( 0.5f, 0.5f );
		baseTrans.localScale = Vector3.one;
		
		// Store the canvas dimensions.
		screenWidth = canvasRectTrans.sizeDelta.x;
		screenHeight = canvasRectTrans.sizeDelta.y;
		
		// Create two Vector2 variables for the size and position to apply.
		Vector2 touchAreaSize = new Vector2( screenWidth / 2, 0 );
		
		touchAreaSize = new Vector2( screenWidth * touchSizeWidth / 100, screenHeight * touchSizeHeight / 100 );

		// Apply the size and position to the touch area.
		baseTrans.sizeDelta = touchAreaSize;
		
		// Configure the position that the user wants the touchpad to be located.
		Vector2 touchpadPosition = ( touchAreaSize / 2 ) - ( canvasRectTrans.sizeDelta / 2 );

		touchpadPosition = new Vector2( canvasRectTrans.sizeDelta.x * ( touchSizePositionX / 100 ) - ( touchAreaSize.x * ( touchSizePositionX / 100 ) ) + ( touchAreaSize.x / 2 ), canvasRectTrans.sizeDelta.y * ( touchSizePositionY / 100 ) - ( touchAreaSize.y * ( touchSizePositionY / 100 ) ) + ( touchAreaSize.y / 2 ) ) - ( canvasRectTrans.sizeDelta / 2 );
		
		baseTrans.localPosition = touchpadPosition;

		if( displayInput && inputImage != null )
		{
			// Set the anchors of the touchpad base. It is important to have the anchors centered for calculations.
			inputImage.rectTransform.anchorMin = new Vector2( 0.5f, 0.5f );
			inputImage.rectTransform.anchorMax = new Vector2( 0.5f, 0.5f );
			inputImage.rectTransform.pivot = new Vector2( 0.5f, 0.5f );

			// Apply the size.
			inputImage.rectTransform.sizeDelta = Vector2.one * ( ( scaleReference == ScaleReference.Height ? screenHeight : screenWidth ) * ( imageSize / 5 ) );
			
			// Apply the position of the input image.
			inputImage.rectTransform.localPosition = Vector3.zero;
		}

		// If the user wants to use touch input...
		if( useTouchInput )
		{
			// Configure the actual size delta and position of the base trans regardless of the canvas scaler setting.
			Vector2 baseSizeDelta = baseTrans.sizeDelta * ParentCanvas.scaleFactor;
			Vector2 baseLocalPosition = baseTrans.localPosition * ParentCanvas.scaleFactor;

			// Calculate the rect of the base trans.
			touchpadRect = new Rect( new Vector2( baseLocalPosition.x - ( baseSizeDelta.x / 2 ), baseLocalPosition.y - ( baseSizeDelta.y / 2 ) ) + ( ( canvasRectTrans.sizeDelta * ParentCanvas.scaleFactor ) / 2 ), baseSizeDelta );
		}
	}

	#if UNITY_EDITOR
	void Update ()
	{
		if( Application.isPlaying == false )
			UpdateTouchpadPositioning();
	}
	#endif
	
	/* --------------------------------------------- *** PUBLIC FUNCTIONS FOR THE USER *** --------------------------------------------- */
	/// <summary>
	/// Updates the Ultimate Touchpad's size and positioning. This function can be called after modifying the size or position values at runtime to apply the changes.
	/// </summary>
	public void UpdatePositioning ()
	{
		if( Application.isPlaying )
			ResetTouchpad();

		UpdateTouchpadPositioning();
	}
	
	/// <summary>
	/// Returns the current horizontal value of the Ultimate Touchpad.
	/// </summary>
	public float GetHorizontalAxis ()
	{
		return horizontalValue;
	}
	
	/// <summary>
	/// Returns the current vertical value of the Ultimate Touchpad.
	/// </summary>
	public float GetVerticalAxis ()
	{
		return verticalValue;
	}
	
	/// <summary>
	/// Returns the Ultimate Touchpad's current state of interaction.
	/// </summary>
	public bool GetTouchpadState ()
	{
		return inputActive;
	}

	/// <summary>
	/// Returns the tap count to the Ultimate Touchpad.
	/// </summary>
	public bool GetTapCount ()
	{
		return tapCountAchieved;
	}

	/// <summary>
	/// Disables the Ultimate Touchpad.
	/// </summary>
	public void DisableTouchpad ()
	{
		// Reset state and _pointerId to default.
		inputActive = false;
		_inputId = -10;

		// Reset the horizontal and vertical values.
		horizontalValue = 0.0f;
		verticalValue = 0.0f;

		// If the user is wanting the input position visible, then disable that visually here.
		if( displayInput && inputImage != null )
			inputImage.color = invisibleInputColor;

		// If the user is wanting to calculate taps...
		if( tapCountOption != TapCountOption.NoCount )
		{
			// Reset the current tap time to zero.
			currentTapTime = 0;
			tapCount = 0;
		}

		// If the user wants to display the base on inactivity, then stop the current coroutine and set the color.
		if( displayBase && displayType == DisplayType.Inactivity )
		{
			StopCoroutine( "DisplayBaseInactivityLogic" );
			baseImage.color = baseColor;
		}
		// Else if the user wants to display the base on input, then stop the current coroutine and set the color.
		else if( displayBase && displayType == DisplayType.OnInput )
		{
			StopCoroutine( "DisplayBaseOnInputLogic" );
			baseImage.color = invisibleBaseColor;
		}
		
		// Disable the gameObject.
		gameObject.SetActive( false );
	}

	/// <summary>
	/// Enables the Ultimate Touchpad.
	/// </summary>
	public void EnableTouchpad ()
	{
		// Enable the gameObject.
		gameObject.SetActive( true );
	}
	/* ------------------------------------------- *** END PUBLIC FUNCTIONS FOR THE USER *** ------------------------------------------- */

	/* --------------------------------------------- *** STATIC FUNCTIONS FOR THE USER *** --------------------------------------------- */
	/// <summary>
	/// Returns the Ultimate Touchpad component that has been registered with the targeted name.
	/// </summary>
	/// <param name="touchpadName">The name of the targeted Ultimate Touchpad.</param>
	public static UltimateTouchpad GetUltimateTouchpad ( string touchpadName )
	{
		if( !TouchpadConfirmed( touchpadName ) )
			return null;

		return UltimateTouchpads[ touchpadName ];
	}
	
	/// <summary>
	/// Returns the current horizontal value of the targeted Ultimate Touchpad.
	/// </summary>
	/// <param name="touchpadName">The name of the targeted Ultimate Touchpad.</param>
	public static float GetHorizontalAxis ( string touchpadName )
	{
		if( !TouchpadConfirmed( touchpadName ) )
			return 0.0f;

		return UltimateTouchpads[ touchpadName ].GetHorizontalAxis();
	}
	
	/// <summary>
	/// Returns the current vertical value of the targeted Ultimate Touchpad.
	/// </summary>
	/// <param name="touchpadName">The name of the targeted Ultimate Touchpad.</param>
	public static float GetVerticalAxis ( string touchpadName )
	{
		if( !TouchpadConfirmed( touchpadName ) )
			return 0.0f;

		return UltimateTouchpads[ touchpadName ].GetVerticalAxis();
	}

	/// <summary>
	/// Returns the current state of interaction of the targeted Ultimate Touchpad.
	/// </summary>
	/// <param name="touchpadName">The name of the targeted Ultimate Touchpad.</param>
	public static bool GetTouchpadState ( string touchpadName )
	{
		if( !TouchpadConfirmed( touchpadName ) )
			return false;

		return UltimateTouchpads[ touchpadName ].inputActive;
	}

	/// <summary>
	/// Returns the current tap count state of the targeted Ultimate Touchpad.
	/// </summary>
	/// <param name="touchpadName">The name of the targeted Ultimate Touchpad.</param>
	public static bool GetTapCount ( string touchpadName )
	{
		if( !TouchpadConfirmed( touchpadName ) )
			return false;

		return UltimateTouchpads[ touchpadName ].tapCountAchieved;
	}

	/// <summary>
	/// Calls the local DisableTouchpad function of the targeted Ultimate Touchpad.
	/// </summary>
	/// <param name="touchpadName">The name of the targeted Ultimate Touchpad.</param>
	public static void DisableTouchpad ( string touchpadName )
	{
		if( !TouchpadConfirmed( touchpadName ) )
			return;

		UltimateTouchpads[ touchpadName ].DisableTouchpad();
	}

	/// <summary>
	/// Calls the local EnableTouchpad function of the targeted Ultimate Touchpad.
	/// </summary>
	/// <param name="touchpadName">The name of the targeted Ultimate Touchpad.</param>
	public static void EnableTouchpad ( string touchpadName )
	{
		if( !TouchpadConfirmed( touchpadName ) )
			return;

		UltimateTouchpads[ touchpadName ].EnableTouchpad();
	}
	/* ------------------------------------------- *** END STATIC FUNCTIONS FOR THE USER *** ------------------------------------------- */
}