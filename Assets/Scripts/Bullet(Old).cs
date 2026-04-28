using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Direction = ChessManager.Direction;
public class BulletOld : MonoBehaviour
{
    LineRenderer lineRenderer;
    public float Speed = 10f;
    public float Duration = 1f;
    public float MaxDistance = 100f;
    public float Damage = 10f;
    public bool IsActive = false;
    [Header("发射者")]
    public Player shooter;
    Chess hitChess;
    [Header("节点位置")]
    [SerializeField] Vector3 head, mid, tail;
    [SerializeField] Vector2 headVelocity, tailVelocity;
    [Header("反弹边界位置")]
    [SerializeField] Vector2 maxBorder;
    [SerializeField] Vector2 minBorder;
    [Header("测试")]
    [SerializeField] Vector2 shootDirction;
    [SerializeField] bool testShoot = false;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        head = mid = tail = transform.position;
    }

    IEnumerator Fly(Vector2 direction)
    {
        headVelocity = direction.normalized * Speed;
        tailVelocity = Vector2.zero;
        Vector3 lastHead = head,lastTail = tail;
        float timer = Duration;
        List<Vector3> midPosition = new List<Vector3>();
        while (true)
        {
            head += (Vector3)headVelocity * Time.deltaTime;
            Vector2 hitPoint;
            int borderResult = BorderDetect(head, lastHead, out hitPoint);
            if(borderResult != -1)//头反弹
            {
                switch(borderResult)
                {
                    case (int)Direction.Up:
                    case (int)Direction.Down:
                        headVelocity.y = -headVelocity.y;
                        head = hitPoint + headVelocity.normalized * (hitPoint - (Vector2)head).magnitude;
                        break;
                    case (int)Direction.Left:
                    case (int)Direction.Right:
                        headVelocity.x = -headVelocity.x;
                        head = hitPoint + headVelocity.normalized * (hitPoint - (Vector2)head).magnitude;
                        break;
                }
                midPosition.Add(hitPoint);
            }
            tail += (Vector3)tailVelocity * Time.deltaTime;
            borderResult = BorderDetect(tail, lastTail, out hitPoint);
            if (borderResult != -1)//尾反弹
            {
                switch (borderResult)
                {
                    case (int)Direction.Up:
                    case (int)Direction.Down:
                        tailVelocity.y = -tailVelocity.y;
                        tail = hitPoint + tailVelocity.normalized * (hitPoint - (Vector2)tail).magnitude;
                        break;
                    case (int)Direction.Left:
                    case (int)Direction.Right:
                        tailVelocity.x = -tailVelocity.x;
                        tail = hitPoint + tailVelocity.normalized * (hitPoint - (Vector2)tail).magnitude;
                        break;
                }
                if(midPosition.Count > 0)
                    midPosition.RemoveAt(0);
            }
            lastHead = head;
            lastTail = tail;
            lineRenderer.positionCount = midPosition.Count + 2;
            lineRenderer.SetPosition(0, head);
            int positionIndex = 1;
            for (int i = midPosition.Count - 1; i >= 0; i--)
            {
                lineRenderer.SetPosition(positionIndex, midPosition[i]);
                positionIndex++;
            }
            lineRenderer.SetPosition(positionIndex, tail);
            if(timer > 0)
            {
                timer -= Time.deltaTime;
                if(timer <= 0)
                {
                    tailVelocity = direction.normalized * Speed;
                }
            }
            hitChess = BulletHit();
            if (hitChess != null){
                hitChess.AddForce(headVelocity);
                hitChess.ForcedMove();
                break;
            }
            yield return null;
        }
        while (hitChess.isActing)
        {

        }
        End();
    }

    Chess BulletHit()
    {
        Chess hitChess = null;
        Vector2Int cellPos = ChessBoard.GetCell(head);
        hitChess = ChessBoard.GetChess(cellPos);
        return hitChess;
    }
    public void Shoot(Vector2 direction)
    {
        IsActive = true;
        StartCoroutine(Fly(direction));
    }

    public void End()
    {
        IsActive = false;
        if(shooter!=null)
        {
            shooter.EndShoot();
        }
        StopAllCoroutines();
    }   

    /*private void Update()
    {
        if (testShoot)
        {
            if (!IsActive)
            {
                IsActive = true;
                Shoot(shootDirction);
            }
            //testShoot = false;
        }
    }*/

    //Vector2[] crossPoints = new Vector2[4];
    //[Header("测试")]
    //[SerializeField] Vector2 curPos;
    //[SerializeField] Vector2 lastPos;
    int BorderDetect(Vector2 currentPosition, Vector2 lastPosition, out Vector2 crossPoint)
    {
        crossPoint = Vector2.zero;
        if(currentPosition == lastPosition)
        {
            return -1;
        }
        if (currentPosition.x < maxBorder.x && currentPosition.x > minBorder.x && currentPosition.y < maxBorder.y && currentPosition.y > minBorder.y)
        {
            return -1;
        } 
        Vector2 dispalcemant = currentPosition - lastPosition;
        //if(currentPosition.x > minBorder.x && currentPosition.x < maxBorder.x)
        //{
        //    if(currentPosition.y > 0)
        //    {

        //        return (int)Direction.Up;
        //    }
        //    else
        //    {
        //        return (int)Direction.Down;
        //    }
        //}
        //else if(currentPosition.y > minBorder.y && currentPosition.y < maxBorder.y)
        //{
        //    if (currentPosition.x > 0)
        //    {
        //        return (int)Direction.Right;
        //    }
        //    else
        //    {
        //        return (int)Direction.Left;
        //    }
        //}
        Vector2[] crossPoints = new Vector2[4];
        crossPoints[(int)Direction.Up] = new Vector2(lastPosition.x + dispalcemant.x * (maxBorder.y - lastPosition.y) / dispalcemant.y, maxBorder.y);
        crossPoints[(int)Direction.Down] = new Vector2(lastPosition.x + dispalcemant.x * (minBorder.y - lastPosition.y) / dispalcemant.y, minBorder.y);
        crossPoints[(int)Direction.Left] = new Vector2(minBorder.x, lastPosition.y + dispalcemant.y * (minBorder.x - lastPosition.x) / dispalcemant.x);
        crossPoints[(int)Direction.Right] = new Vector2(maxBorder.x, lastPosition.y + dispalcemant.y * (maxBorder.x - lastPosition.x) / dispalcemant.x);
        int result = -1;
        float minDistance = dispalcemant.magnitude;
        for (int i = 0; i < 4; i++)
        {
            if (Vector2.Dot(dispalcemant, crossPoints[i] - lastPosition) > 0 && (crossPoints[i] - lastPosition).magnitude < minDistance)
            {
                result = i;
                minDistance = (crossPoints[i] - lastPosition).magnitude;
                crossPoint = crossPoints[i];
            }
        }
        return result;
    }
    [SerializeField] Direction borderHit = Direction.Up;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(maxBorder + Vector2.left * 100, maxBorder + Vector2.right * 100);
        Gizmos.DrawLine(minBorder + Vector2.left * 100, minBorder + Vector2.right * 100);
        Gizmos.DrawLine(maxBorder + Vector2.up * 100, maxBorder + Vector2.down * 100);
        Gizmos.DrawLine(minBorder + Vector2.up * 100, minBorder + Vector2.down * 100);
        Gizmos.color = Color.blue;
        //Gizmos.DrawSphere(curPos, 0.1f);
        //Gizmos.DrawSphere(lastPos, 0.1f);
        //Gizmos.DrawLine(lastPos, curPos);
        //int borderResult = BorderDetect(curPos, lastPos);
        //if (borderResult != -1)
        //{
        //    borderHit = (Direction)borderResult;
        //}
        
        //Gizmos.color = Color.yellow;
        //foreach(Vector2 p in crossPoints)
        //{
        //    Gizmos.DrawSphere(p, 0.1f);
        //}
    }
}
