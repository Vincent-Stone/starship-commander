using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Cursor : MonoBehaviour
{
    Vector2Int position = Vector2Int.zero;
    [SerializeField] AnimationCurve movingCurve;
    //[SerializeField] AnimationCurve colorCurve;
    [Range(0,20)]
    [SerializeField] float moveSpeed = 0.5f;
    [Range(0,1)]
    [SerializeField] float maxMoveDuration = 1f;
    float moveDuration;
    float movingTimer = 0;
    Vector2 startPosition,targetPosition;
    [SerializeField]SpriteRenderer spriteRenderer;
    public void MoveTo(Vector2 targetPosition)
    {
        if(this.targetPosition == targetPosition)
        {
            return;
        }
        this.targetPosition = targetPosition;
        if(movingTimer < 1)
        {
            StopAllCoroutines();
        }
        movingTimer = 0;
        startPosition = transform.position;
        moveDuration = Vector2.Distance(startPosition, targetPosition) / moveSpeed;
        moveDuration = Mathf.Min(moveDuration, maxMoveDuration);
        StartCoroutine(AimmingCursor());
    }
    IEnumerator AimmingCursor()
    {
        for (; movingTimer < 1; movingTimer += Time.deltaTime / moveDuration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, movingCurve.Evaluate(movingTimer));
            
            yield return null;
        }
        movingTimer = 1;
    }
}
