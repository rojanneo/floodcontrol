using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class GameBoard
    {
        Random m_rand = new Random();
        public const int m_gameBoardWidth = 8;
        public const int m_gameBoardHeight = 10;

        private GamePiece[,] m_boardSquares = new GamePiece[m_gameBoardWidth, m_gameBoardHeight];

        private List<Vector2> m_waterTracker = new List<Vector2>();

        public Dictionary<string, FallingPiece> m_fallingPieces = new Dictionary<string, FallingPiece>();
        public Dictionary<string, RotatingPiece> m_rotatingPieces = new Dictionary<string, RotatingPiece>();
        public Dictionary<string, FadingPiece> m_fadingPieces = new Dictionary<string, FadingPiece>();

        public void ClearBoard()
        {
            for (int x = 0; x < m_gameBoardWidth; x++)
            {
                for (int y = 0; y < m_gameBoardHeight; y++)
                {
                    m_boardSquares[x, y] = new GamePiece("Empty");
                }
            }
        }

        public GameBoard()
        {
            ClearBoard();
        }

        public void RotatePiece(int x, int y, bool clockwise)
        {
            m_boardSquares[x,y].RotatePiece(clockwise);
        }

        public Rectangle GetSourceRect(int x, int y)
        {
            return m_boardSquares[x, y].GetSourceRect();
        }

        public string GetSquare(int x, int y)
        {
            return m_boardSquares[x, y].PieceType;
        }

        public void SetSquare(int x, int y, string pieceName)
        {
            m_boardSquares[x, y].SetPiece(pieceName);
        }

        public bool HasConnector(int x, int y, string direction)
        {
            return m_boardSquares[x, y].HasConnector(direction);
        }

        public void RandomPiece(int x, int y)
        {
            m_boardSquares[x, y].SetPiece(GamePiece.m_pieceTypes[m_rand.Next(0, GamePiece.m_maxPlayablePieceIndex + 1)]);
        }

        public void FillFromAbove(int x, int y)
        {
            int rowLookUp = y - 1;
            while (rowLookUp >= 0)
            {
                if (GetSquare(x, rowLookUp) != "Empty")
                {
                    SetSquare(x, y, GetSquare(x, rowLookUp));
                    SetSquare(x, rowLookUp, "Empty");
                    AddFallingPiece(x, y, GetSquare(x, y), GamePiece.m_pieceHeight * (y - rowLookUp));
                    rowLookUp = -1;
                }
                rowLookUp--;
            }
        }

        public void GenerateNewPieces(bool dropSquares)
        {
            if (dropSquares)
            {
                for (int x = 0; x < m_gameBoardWidth; x++)
                {
                    for (int y = m_gameBoardHeight - 1; y >= 0; y--)
                    {
                        if (GetSquare(x, y) == "Empty")
                        {
                            FillFromAbove(x, y);
                        }
                    }
                }
            }

            for (int y = 0; y < m_gameBoardHeight; y++)
            {
                for (int x = 0; x < m_gameBoardWidth; x++)
                {
                    if (GetSquare(x, y) == "Empty")
                    {
                        RandomPiece(x, y);
                        AddFallingPiece(x, y, GetSquare(x, y), m_gameBoardHeight * GamePiece.m_pieceHeight);
                    }
                }
            }
        }

        public void ResetWater()
        {
            for (int y = 0; y < m_gameBoardHeight; y++)
            {
                for (int x = 0; x < m_gameBoardWidth; x++)
                {
                    m_boardSquares[x, y].RemoveSuffix("W");
                }
            }
        }

        public void FillPiece(int x, int y)
        {
            m_boardSquares[x, y].AddSuffix("W");
        }

        public void PropagateWater(int x, int y, string fromDirection)
        {
            if ((y >= 0) && (y < m_gameBoardHeight) && (x >= 0) && (x < m_gameBoardWidth))
            {
                if (m_boardSquares[x, y].HasConnector(fromDirection) && !m_boardSquares[x, y].Suffix.Contains("W"))
                {
                    FillPiece(x, y);
                    m_waterTracker.Add(new Vector2(x, y));
                    foreach (string end in m_boardSquares[x, y].GetOtherEnds(fromDirection))
                    {
                        switch (end)
                        { 
                            case "Left":
                                PropagateWater(x - 1, y, "Right");
                                break;
                            case "Right":
                                PropagateWater(x + 1, y, "Left");
                                break;
                            case "Top":
                                PropagateWater(x, y - 1, "Bottom");
                                break;
                            case "Bottom":
                                PropagateWater(x, y + 1, "Top");
                                break;
                        }
                    }
                }
            }
        }

        public List<Vector2> GetWaterChain(int y)
        {
            m_waterTracker.Clear();
            PropagateWater(0, y, "Left");
            return m_waterTracker;
        }

        public void AddFallingPiece(int x, int y, string pieceName, int verticalOffset)
        {
            m_fallingPieces[x.ToString() + "_" + y.ToString()] = new FallingPiece(pieceName, verticalOffset);
        }

        public void AddRotatingPiece(int x, int y, string pieceName, bool clockwise)
        {
            m_rotatingPieces[x.ToString() + "_" + y.ToString()] = new RotatingPiece(pieceName, clockwise);
        }

        public void AddFadingPiece(int x, int y, string pieceName)
        {
            m_fadingPieces[x.ToString() + "_" + y.ToString()] = new FadingPiece(pieceName, "W");
        }

        public bool ArePieceAnimating()
        {
            if (m_fallingPieces.Count == 0 && m_rotatingPieces.Count == 0 && m_fadingPieces.Count == 0)
                return false;
            else
                return true;
        }

        private void UpdateFadingPieces()
        {
            Queue<string> removeKeys = new Queue<string>();
            foreach (string key in m_fadingPieces.Keys)
            {
                m_fadingPieces[key].UpdatePiece();
                if (m_fadingPieces[key].m_alphaLevel == 0.0f)
                    removeKeys.Enqueue(key.ToString());
            }

            while (removeKeys.Count > 0)
            {
                m_fadingPieces.Remove(removeKeys.Dequeue());
            }
        }

        private void UpdateFallingPieces()
        {
            Queue<string> removeKeys = new Queue<string>();
            foreach (string key in m_fallingPieces.Keys)
            {
                m_fallingPieces[key].UpdatePiece();
                if (m_fallingPieces[key].m_verticalOffset == 0)
                    removeKeys.Enqueue(key.ToString());
            }
            while (removeKeys.Count > 0)
            {
                m_fallingPieces.Remove(removeKeys.Dequeue());
            }
        }

        private void UpdateRotatingPieces()
        { 
            Queue<string> removeKeys = new Queue<string>();
            foreach (string key in m_rotatingPieces.Keys)
            {
                m_rotatingPieces[key].UpdatePiece();
                if (m_rotatingPieces[key].m_rotationTicksRemaining == 0)
                    removeKeys.Enqueue(key.ToString());
            }
            while (removeKeys.Count > 0)
            {
                m_rotatingPieces.Remove(removeKeys.Dequeue());
            }
        }

        public void UpdateAnimatedPieces()
        {
            if (m_fadingPieces.Count == 0)
            {
                UpdateRotatingPieces();
                UpdateFallingPieces();
            }
            else 
            {
                UpdateFadingPieces();
            }
        }
    }
}
