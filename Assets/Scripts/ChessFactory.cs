using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessFactory : MonoBehaviour
{
    static GameObject Pawn, Knight, Rook, Cannon, EnemyBase, IronGiant, Shuriken, Starfish, FireMan;
    public static void Init()
    {
        Debug.Log("Init ChessFactory");
        if (Pawn != null && Knight != null && Rook != null)
            return;
        Pawn = Resources.Load("Prefabs/Pawn") as GameObject;
        Knight = Resources.Load("Prefabs/Knight") as GameObject;
        Rook = Resources.Load("Prefabs/Rook") as GameObject;
        Cannon = Resources.Load("Prefabs/Cannon") as GameObject;
        EnemyBase = Resources.Load("Prefabs/EnemyBase") as GameObject;
        IronGiant = Resources.Load("Prefabs/铁人") as GameObject;
        Shuriken = Resources.Load("Prefabs/手里剑") as GameObject;
        Starfish = Resources.Load("Prefabs/海星") as GameObject;
        FireMan = Resources.Load("Prefabs/火人") as GameObject;
    }

    public static Chess CreateChess(char chessType, Transform parent)
    {
        Chess chess = null;
        GameObject chessObject = null;
        switch (chessType)
        {
            case 'p':
                chessObject = Instantiate(Pawn, Vector3.zero, Quaternion.identity);
                break;
            case 'k':
                chessObject = Instantiate(Knight, Vector3.zero, Quaternion.identity);
                break;
            case 'r':
                chessObject = Instantiate(Rook, Vector3.zero, Quaternion.identity);
                break;
            case 'c':
                chessObject = Instantiate(Cannon, Vector3.zero, Quaternion.identity);
                break;
            case 'E':
                chessObject = Instantiate(EnemyBase, Vector3.zero, Quaternion.identity);
                break;
            case 'I':
                chessObject = Instantiate(IronGiant, Vector3.zero, Quaternion.identity);
                break;
            case 'S':
                chessObject = Instantiate(Shuriken, Vector3.zero, Quaternion.identity);
                break;
            case 's':
                chessObject = Instantiate(Starfish, Vector3.zero, Quaternion.identity);
                break;
            case 'F':
                chessObject = Instantiate(FireMan, Vector3.zero, Quaternion.identity);
                break;
        }
        if (chessObject != null)
        {
            chess = chessObject.GetComponent<Chess>();
            chessObject.transform.SetParent(parent);
        }
        else
        {
            Debug.LogError("Failed to instantiate Chess prefab. Chess Type:" + chessType);
        }
        return chess;
    }
}
