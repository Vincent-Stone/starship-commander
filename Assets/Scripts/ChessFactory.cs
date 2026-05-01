using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessFactory : MonoBehaviour
{
    static GameObject Pawn,Knight,Rook,Cannon;
    public static void Init()
    {
        Debug.Log("Init ChessFactory");
        if (Pawn != null && Knight != null && Rook != null)
            return;
        Pawn = Resources.Load("Prefabs/Pawn") as GameObject;
        Knight = Resources.Load("Prefabs/Knight") as GameObject;
        Rook = Resources.Load("Prefabs/Rook") as GameObject;
        Cannon = Resources.Load("Prefabs/Cannon") as GameObject;
        if (Pawn == null && Knight == null && Rook == null && Cannon == null)
        {
            Debug.LogError("Failed to load Chess prefabs. Please check the path and prefab names.");
        }
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
