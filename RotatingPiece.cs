using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class RotatingPiece: GamePiece
    {
        public bool m_clockwise;

        public static float m_rotationRate = (MathHelper.PiOver2 / 10);
        private float m_rotationAmount = 0;
        public int m_rotationTicksRemaining = 10;

        public float RotationAmount
        {
            get 
            {
                if (m_clockwise)
                {
                    return m_rotationAmount;
                }
                else 
                {
                    return ((MathHelper.Pi * 2) - m_rotationAmount);
                }
            }
        }

        public RotatingPiece(string pieceType, bool clockwise)
            : base(pieceType)
        {
            this.m_clockwise = clockwise;
        }

        public void UpdatePiece()
        {
            m_rotationAmount += m_rotationRate;
            m_rotationTicksRemaining = (int)(MathHelper.Max(0, m_rotationTicksRemaining - 1));
        }
    }
}
