using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class FallingPiece:GamePiece
    {
        public int m_verticalOffset = 0;
        public static int m_fallRate = 5;

        public FallingPiece(string pieceType, int verticalOffset)
            : base(pieceType)
        {
            this.m_verticalOffset = verticalOffset;
        }

        public void UpdatePiece()
        {
            m_verticalOffset = (int)MathHelper.Max(0, m_verticalOffset - m_fallRate);
        }
    }
}
