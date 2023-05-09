/* Written by Kaz Crowe */
/* SimpleTouchpadExample.cs */
using UnityEngine;

[RequireComponent( typeof( Rigidbody ) )]
public class SimpleTouchpadExample : MonoBehaviour
{
	public Transform camTrans;
	public GameObject[] cubes;
	Rigidbody myRigidbody;
	public float jumpForce = 250.0f;


	void Start ()
	{
		// Store the Rigidbody component.
		myRigidbody = GetComponent<Rigidbody>();
	}

	void Update ()
	{
		// Catch the horizontal and vertical values of our input.
		float h = UltimateTouchpad.GetHorizontalAxis( "Camera" );
		float v = UltimateTouchpad.GetVerticalAxis( "Camera" );

		// Rotate the camera by the h and v variables.
		camTrans.Rotate( Vector3.up, h * Time.deltaTime );
		camTrans.Rotate( Vector3.left, v * Time.deltaTime );

		// Finalize the rotation so that there is no Z rotation.
		Quaternion finalRot = Quaternion.Euler( camTrans.rotation.eulerAngles.x, camTrans.rotation.eulerAngles.y, 0 );
		camTrans.rotation = finalRot;

		// Store a new color to apply to the cubes.
		Color newColor = UltimateTouchpad.GetTouchpadState( "Camera" ) ? Color.green : Color.white;
		for( int i = 0; i < cubes.Length; i++ )
			cubes[ i ].GetComponent<Renderer>().material.color = newColor;

		// If the user has double tapped the camera Touchpad, then jump the character.
		if( UltimateTouchpad.GetTapCount( "Camera" ) )
			myRigidbody.AddForce( Vector3.up * jumpForce );
    }

	void OnGUI ()
	{
		// Simply display the values of the Touchpad.
		GUI.Label( new Rect( 50, 50, 300, 150 ), "Horizontal: " + UltimateTouchpad.GetHorizontalAxis( "Camera" ).ToString( "F1" ) + " \nVertical: " + UltimateTouchpad.GetVerticalAxis( "Camera" ).ToString( "F1" ), GUI.skin.label );
	}
}