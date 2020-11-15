using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapturedField : MonoBehaviour
{
    public class CapturedSentence
    {
        public ChessPiece piece;
        public int turnsLeft;

        public void SetPiece(ChessPiece piece)
        {
            this.piece = piece;
            turnsLeft = 2;
        }
    }
    [SerializeField]
    private Transform[] m_transformPoints;

    public CapturedSentence[] capturedObjects { get; private set; }



    private void Awake()
    {
        capturedObjects = new CapturedSentence[3];

        for(int i = 0; i < capturedObjects.Length; i++)
        {
            capturedObjects[i] = new CapturedSentence();
        }
    }

    public void CaptureTarget(ChessPiece obj)
    {
        for(int i = 0; i < capturedObjects.Length; i++)
        {
            if(capturedObjects[i].piece == null)
            {
                capturedObjects[i].SetPiece(obj);
                obj.transform.position = m_transformPoints[i].position;
                break;
            }
        }
    }


    public void TickTurn()
    {
        foreach(CapturedSentence pieces in capturedObjects)
        {
            if(pieces.turnsLeft > 0)
                pieces.turnsLeft--;
        }
    }

    public void ReleaseTarget(ChessPiece obj)
    {
        for(int i = 0; i < capturedObjects.Length; i++)
        {
            if(capturedObjects[i].piece == obj)
            {
                capturedObjects[i].piece = null;
                break;
            }
        }
    }

    public void ClearField()
    {
        for(int i = 0; i < capturedObjects.Length; i++)
        {
            capturedObjects[i].piece = null;
        }
    }


    private void OnDrawGizmos()
    {
        foreach(Transform point in m_transformPoints)
        {
            Gizmos.DrawWireCube(point.position, Vector3.one);
        }
    }
}
