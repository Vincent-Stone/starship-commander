using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour , Projectile
{
    public Chess shooter;
    public Chess hitChess;
    internal Chess lastHitChess;
    [Header("ËÙ¶È")]
    public float flyingSpeed;
    public float lifeTime = 1;
    Vector2 velocity;
    public Vector2Int cellPos;
    Vector2 MaxBorder
    {
        get
        {
            return new Vector2(4.5f, 5 - 9 + ChessManager.instance.highestRow);
        }
    }
    Vector2 MinBorder
    {
        get
        {
            return new Vector2(-4.5f, -5 - 9 + ChessManager.instance.highestRow);
        }
    }
    public void Shoot(Vector2 shootDirection)
    {
        velocity = shootDirection.normalized * flyingSpeed;
        transform.position = shooter.transform.position;
        lastHitChess = shooter;
        StartCoroutine(Flying());
    }
    public IEnumerator Flying()
    {
        float timer = 0;

        while (timer < lifeTime)
        {
            transform.position += (Vector3)velocity * Time.deltaTime;
            if (transform.position.x > MaxBorder.x)
            {
                velocity.x = -velocity.x;
                transform.position = new Vector3(MaxBorder.x * 2 - transform.position.x, transform.position.y, transform.position.z);
            }
            else if (transform.position.x < MinBorder.x)
            {
                velocity.x = -velocity.x;
                transform.position = new Vector3(MinBorder.x * 2 - transform.position.x, transform.position.y, transform.position.z);
            }
            if (transform.position.y > MaxBorder.y)
            {
                velocity.y = -velocity.y;
                transform.position = new Vector3(transform.position.x, MaxBorder.y * 2 - transform.position.y, transform.position.z);
            }
            else if (transform.position.y < MinBorder.y)
            {
                velocity.y = -velocity.y;
                transform.position = new Vector3(transform.position.x, MinBorder.y * 2 - transform.position.y, transform.position.z);
            }
            cellPos = ChessBoard.GetCell(transform.position);
            hitChess = ChessBoard.GetChess(cellPos);
            if (hitChess != null)
            {
                if (hitChess != shooter || lastHitChess != shooter && hitChess == shooter)
                {
                    //hitChess.AddForce(velocity);
                    //hitChess.isActing = true;
                    if (hitChess != shooter)
                    {
                        ChessManager.instance.PushActingChess(hitChess);
                        hitChess.TakeDamage(1, shooter, new Vector2Int((int)(velocity.normalized.x * 10), (int)(velocity.normalized.y * 10)));
                    }
                    //ChessManager.instance.PushActingChess(hitChess);
                    //hitChess.ForcedMove();
                    //Debug.Log("hit chess is " + hitChess+", last hit chess is" + lastHitChess);
                    break;
                }
            }
            lastHitChess = hitChess;
            timer += Time.deltaTime;
            velocity *= 1.001f;
            yield return null;
        }
        shooter.isActing = false;
        //shooter.GetComponent<Player>().ShootEnd();
        this.gameObject.SetActive(false);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector2(MinBorder.x, MinBorder.y), new Vector2(MinBorder.x, MaxBorder.y));
        Gizmos.DrawLine(new Vector2(MinBorder.x, MaxBorder.y), new Vector2(MaxBorder.x, MaxBorder.y));
        Gizmos.DrawLine(new Vector2(MaxBorder.x, MaxBorder.y), new Vector2(MaxBorder.x, MinBorder.y));
        Gizmos.DrawLine(new Vector2(MaxBorder.x, MinBorder.y), new Vector2(MinBorder.x, MinBorder.y));
    }
}
