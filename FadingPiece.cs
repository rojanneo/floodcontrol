using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class FadingPiece:GamePiece
    {
        public float m_alphaLevel = 1.0f;
        public static float m_alphaChangeRate = 0.02f;

        public FadingPiece(string pieceType, string suffix)
            : base(pieceType, suffix)
        { 
                
        }

        public void UpdatePiece()
        {
            m_alphaLevel = (float)MathHelper.Max(0, m_alphaLevel - m_alphaChangeRate);
        }
    }
}
