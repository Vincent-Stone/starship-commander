using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Chess shooter;
    Chess hitChess;
    Chess lastHitChess;
    [Header("速度")]
    [SerializeField] float speed;
    [SerializeField] float lifeTime = 1;
    Vector2 velocity;
    Vector2Int cellPos;
    [Header("反弹边界位置")]
    [SerializeField] Vector2 maxBorder;
    [SerializeField] Vector2 minBorder;
    [Header("Debug")]
    [SerializeField] Vector2 debugVelocity;
    
    public void Shoot(Vector2 shootDirection)
    {
        velocity = shootDirection.normalized * speed;
        transform.position = shooter.transform.position;
        lastHitChess = shooter;
        StartCoroutine(Flying());
    }
    IEnumerator Flying()
    {
        float timer = 0;
        
        while (timer < lifeTime)
        {
            transform.position += (Vector3)velocity * Time.deltaTime;
            if(transform.position.x>maxBorder.x)
            {
                velocity.x = -velocity.x;
                transform.position = new Vector3(maxBorder.x * 2 - transform.position.x, transform.position.y, transform.position.z);
            }else if (transform.position.x < minBorder.x)
            {
                velocity.x = -velocity.x;
                transform.position = new Vector3(minBorder.x * 2 - transform.position.x, transform.position.y, transform.position.z);
            }
            if (transform.position.y > maxBorder.y)
            {
                velocity.y = -velocity.y;
                transform.position = new Vector3(transform.position.x, maxBorder.y * 2 - transform.position.y, transform.position.z);
            }
            else if (transform.position.y < minBorder.y)
            {
                velocity.y = -velocity.y;
                transform.position = new Vector3(transform.position.x, minBorder.y * 2 - transform.position.y, transform.position.z);
            }
            debugVelocity = velocity;
            cellPos = ChessBoard.GetCell(transform.position);
            hitChess = ChessBoard.GetChess(cellPos);
            if(hitChess != null && hitChess.canBeForcedMoved)
            {
                if(hitChess != shooter || lastHitChess != shooter && hitChess == shooter)
                {
                    hitChess.AddForce(velocity);
                    hitChess.isActing = true;
                    hitChess.Freeze(2);
                    ChessManager.instance.PushActingChess(hitChess);
                    hitChess.ForcedMove();
                    //Debug.Log("hit chess is " + hitChess+", last hit chess is" + lastHitChess);
                    break;
                }
            }
            lastHitChess = hitChess;
            timer += Time.deltaTime;
            velocity *= 1.001f;
            yield return null;
        }
        shooter.ActEnd();
        this.gameObject.SetActive(false);
    }

    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (maxBorder.x < minBorder.x || maxBorder.y < minBorder.y)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;
        Gizmos.DrawLine(maxBorder + Vector2.left * 100, maxBorder + Vector2.right * 100);
        Gizmos.DrawLine(maxBorder + Vector2.up * 100, maxBorder + Vector2.down * 100);
        Gizmos.DrawLine(minBorder + Vector2.left * 100, minBorder + Vector2.right * 100);
        Gizmos.DrawLine(minBorder + Vector2.up * 100, minBorder + Vector2.down * 100);
    }
}
