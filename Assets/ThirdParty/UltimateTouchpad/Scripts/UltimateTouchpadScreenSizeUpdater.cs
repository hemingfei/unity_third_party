/* Written by Kaz Crowe */
/* UltimateTouchpadScreenSizeUpdater.cs */
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UltimateTouchpadScreenSizeUpdater : UIBehaviour
{
	protected override void OnRectTransformDimensionsChange ()
	{
		if( gameObject == null || !gameObject.activeInHierarchy )
			return;
		
		StartCoroutine( "YieldPositioning" );
	}

	IEnumerator YieldPositioning ()
	{
		yield return new WaitForEndOfFrame();
		
		UltimateTouchpad[] allTouchpads = FindObjectsOfType( typeof( UltimateTouchpad ) ) as UltimateTouchpad[];
		
		for( int i = 0; i < allTouchpads.Length; i++ )
			allTouchpads[ i ].UpdatePositioning();
	}
}