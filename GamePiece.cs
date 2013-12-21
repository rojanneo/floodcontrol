using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class GamePiece
    {
        public static string[] m_pieceTypes =  
        {
            "Left,Right",
            "Top,Bottom",
            "Left,Top",
            "Top,Right",
            "Right,Bottom",
            "Bottom,Left",
            "Empty"
        };

        public const int m_pieceHeight = 40;
        public const int m_pieceWidth = 40;

        public const int m_maxPlayablePieceIndex = 5;
        public const int m_emptyPieceIndex = 6;

        private const int m_textureOffsetX = 1;
        private const int m_textureOffsetY = 1;
        private const int m_texturePaddingX = 1;
        private const int m_texturePaddingY = 1;

        private string m_pieceType = "";
        private string m_pieceSuffix = "";

        public GamePiece(string type, string suffix)
        {
            m_pieceType = type;
            m_pieceSuffix = suffix;
        }

        public GamePiece(string type)
        {
            m_pieceType = type;
            m_pieceSuffix = "";
        }

        public void SetPiece(string type, string suffix)
        {
            m_pieceType = type;
            m_pieceSuffix = suffix;
        }

        public void SetPiece(string type)
        {
            SetPiece(type, "");
        }

        public void AddSuffix(string suffix)
        {
            if (!m_pieceSuffix.Contains(suffix))
            {
                m_pieceSuffix += suffix;
            }
        }

        public void RemoveSuffix(string suffix)
        {
            m_pieceSuffix = m_pieceSuffix.Replace(suffix, "");
        }

        public string PieceType
        {
            get { return m_pieceType;}
        }

        public string Suffix
        {
            get { return m_pieceSuffix; }
        }

        public void RotatePiece(bool clockwise)
        {
            switch (m_pieceType)
            { 
                case "Left,Right":
                    m_pieceType = "Top,Bottom";
                    break;
                case "Top,Bottom":
                    m_pieceType = "Left,Right";
                    break;
                case "Left,Top":
                    if (clockwise) m_pieceType = "Top,Right";
                    else m_pieceType = "Bottom,Left";
                    break;
                case "Top,Right":
                    if (clockwise) m_pieceType = "Right,Bottom";
                    else m_pieceType = "Left,Top";
                    break;
                case "Right,Bottom":
                    if (clockwise) m_pieceType = "Bottom,Left";
                    else m_pieceType = "Top,Right";
                    break;
                case "Bottom,Left":
                    if (clockwise) m_pieceType = "Left,Top";
                    else m_pieceType = "Right,Bottom";
                    break;
                case "Empty":
                    break;
            }
        }

        public string[] GetOtherEnds(string startingEnd)
        {
            List<string> opposites = new List<string>();
            foreach (string end in m_pieceType.Split(','))
            {
                if (end != startingEnd)
                {
                    opposites.Add(end);
                }
            }
            return opposites.ToArray();
        }

        public bool HasConnector(string direction)
        {
            return m_pieceType.Contains(direction);
        }

        public Rectangle GetSourceRect()
        {
            int x = m_textureOffsetX;
            int y = m_textureOffsetY;

            if (m_pieceSuffix.Contains("W"))
            {
                x += m_pieceWidth + m_texturePaddingX;
            }

            y += (Array.IndexOf(m_pieceTypes, m_pieceType) * (m_pieceHeight + m_texturePaddingY));

            return new Rectangle(x, y, m_pieceWidth, m_pieceHeight);
        }
    }
}
