using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Flood_Control
{
    class ScoreZoom
    {
        public string m_text;
        public Color m_drawColor;
        private int m_displayCounter;
        private int m_maximumDisplayCounter = 30;
        private float m_scale = 0.4f;
        private float m_lastScaleAmount = 0.0f;
        private float m_scaleAmount = 0.4f;

        public float Scale
        {
            get { return m_scaleAmount * m_displayCounter; }
        }

        public bool IsCompleted
        {
            get { return (m_displayCounter> m_maximumDisplayCounter);}
        }

        public ScoreZoom(string displayText, Color color)
        {
            m_text = displayText;
            m_drawColor = color;
            m_displayCounter = 0;
        }

        public void Update()
        {
            m_scale += m_lastScaleAmount + m_scaleAmount;
            m_lastScaleAmount += m_scaleAmount;
            m_displayCounter++;
        }
    }
}
