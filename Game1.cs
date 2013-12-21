using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Flood_Control
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont Runes;

        Texture2D playingPieces;
        Texture2D backgroundScreen;
        Texture2D currentScreen;
       // Texture2D titleScreen;

        GameBoard m_gameBoard;
        Vector2 m_gameBoardDisplayOrigin = new Vector2(70, 89);
        int m_playerScore = 0;
        enum GameState {TitleScreen, Playing, GameOver };
        GameState m_gameStates = GameState.TitleScreen;

        Rectangle m_emptyPiece = new Rectangle(1, 247, 40, 40);
        const float m_minTimeSinceLastInput = 0.25f;
        float m_timeSinceLastInput = 0.0f;

        Vector2 m_scorePosition = new Vector2(605, 215);
        Vector2 m_gameOverLocation = new Vector2(200, 260);
        float m_gameOverTime;

        Queue<ScoreZoom> m_scoreZooms = new Queue<ScoreZoom>();

        const float m_maxFloodCounter = 100.0f;
        float m_floodCount = 0.0f;
        float m_timeSinceLastFloodIncrease = 0.0f;
        float m_timeBetweenFloodIncrease = 1.0f;
        float m_floodIncreaseAmount = 0.5f;

        const int m_maxWaterHeight = 244;
        const int m_WaterWidth = 297;

        Vector2 m_waterOverlayStart = new Vector2(85, 245);
        Vector2 m_waterPosition = new Vector2(478, 338);

        int m_currentLevel = 0;
        int m_linesCompletedThisLevel = 0;
        const float m_floodAccelerationPerLevel = 0.5f;
        Vector2 m_levelTextPosition = new Vector2(512, 215);


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            graphics.ApplyChanges();
            m_gameBoard = new GameBoard();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

            playingPieces = Content.Load<Texture2D>(@"Textures\Tile_Sheet");
            backgroundScreen = Content.Load<Texture2D>(@"Textures\Background");
            currentScreen = Content.Load<Texture2D>(@"Textures\TitleScreen");
            Runes = Content.Load<SpriteFont>(@"Fonts\Runes");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            switch (m_gameStates)
            { 
                case GameState.TitleScreen:
                    if (Keyboard.GetState().IsKeyDown(Keys.Space))
                    {
                        m_gameBoard.ClearBoard();
                        m_gameBoard.GenerateNewPieces(false);
                        m_playerScore = 0;
                        m_currentLevel = 0;
                        m_floodIncreaseAmount = 0.0f;
                        StartNewLevel();
                        m_gameStates = GameState.Playing;
                    }
                    break;
                    
                case GameState.Playing:
                    m_timeSinceLastInput += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    m_timeSinceLastFloodIncrease += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (m_timeSinceLastFloodIncrease >= m_timeBetweenFloodIncrease)
                    {
                        m_floodCount += m_floodIncreaseAmount;
                        m_timeSinceLastFloodIncrease = 0.0f;
                        if (m_floodCount >= m_maxFloodCounter)
                        {
                            m_gameOverTime = 8.0f;
                            m_gameStates = GameState.GameOver;
                        }
                    }

                    if (m_gameBoard.ArePieceAnimating())
                    {
                        m_gameBoard.UpdateAnimatedPieces();
                    }
                    else
                    {
                        m_gameBoard.ResetWater();
                        for (int y = 0; y < GameBoard.m_gameBoardHeight; y++)
                        { 
                            CheckScoringChain(m_gameBoard.GetWaterChain(y));
                        }
                        m_gameBoard.GenerateNewPieces(true);
                        if (m_timeSinceLastInput >= m_minTimeSinceLastInput)
                        {
                            HandleMouseInput(Mouse.GetState());
                        }
                    }
                    UpdateScoreZoom();
                    break;
                case GameState.GameOver:
                    m_gameOverTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if(m_gameOverTime <= 0)
                    {
                        m_gameStates = GameState.TitleScreen;
                    }
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            if (m_gameStates == GameState.TitleScreen)
            {
                spriteBatch.Begin();
                spriteBatch.Draw(currentScreen, new Rectangle(0, 0, this.Window.ClientBounds.Width, this.Window.ClientBounds.Height), Color.White);
                spriteBatch.End();
            }

            if (m_gameStates == GameState.Playing|| m_gameStates == GameState.GameOver)
            {
                spriteBatch.Begin();
                        spriteBatch.Draw(backgroundScreen, new Rectangle(0, 0, this.Window.ClientBounds.Width, this.Window.ClientBounds.Height), Color.White);

                        for (int x = 0; x < GameBoard.m_gameBoardWidth; x++)
                        {
                            for (int y = 0; y < GameBoard.m_gameBoardHeight; y++)
                            {
                                int pixelX = (int)m_gameBoardDisplayOrigin.X + (x * GamePiece.m_pieceWidth);
                                int pixelY = (int)m_gameBoardDisplayOrigin.Y + (y * GamePiece.m_pieceHeight);

                                DrawEmptyPiece(pixelX, pixelY);
                                bool pieceDrawn = false;
                                string positionName = x.ToString() + "_" + y.ToString();
                                if (m_gameBoard.m_rotatingPieces.ContainsKey(positionName))
                                {
                                    DrawRotatingPieces(pixelX, pixelY, positionName);
                                    pieceDrawn = true;
                                }

                                if (m_gameBoard.m_fadingPieces.ContainsKey(positionName))
                                {
                                    DrawFadingPiece(pixelX, pixelY, positionName);
                                    pieceDrawn = true;
                                }

                                if (m_gameBoard.m_fallingPieces.ContainsKey(positionName))
                                {
                                    DrawFallingPiece(pixelX, pixelY, positionName);

                                    pieceDrawn = true;
                                }

                                if (!pieceDrawn)
                                {
                                    DrawStandardPiece(x, y, pixelX, pixelY);
                                }
                            }
                        }
                        foreach (ScoreZoom zoom in m_scoreZooms)
                        {
                            spriteBatch.DrawString(Runes, zoom.m_text, 
                                new Vector2(this.Window.ClientBounds.Width / 2, this.Window.ClientBounds.Height / 2), 
                                zoom.m_drawColor, 
                                0.0f, 
                                new Vector2(Runes.MeasureString(zoom.m_text).X / 2, Runes.MeasureString(zoom.m_text).Y / 2), 
                                zoom.Scale, 
                                SpriteEffects.None, 
                                0.0f);
                        }
                        spriteBatch.DrawString(Runes, m_playerScore.ToString(), m_scorePosition, Color.Black);
                        int m_waterHeight = (int)(m_maxWaterHeight * (m_floodCount / 100));

                        spriteBatch.DrawString(Runes, m_currentLevel.ToString(), m_levelTextPosition, Color.Black);

                        spriteBatch.Draw(backgroundScreen,
                            new Rectangle((int)m_waterPosition.X, (int)m_waterPosition.Y + (m_maxWaterHeight - m_waterHeight), m_WaterWidth, m_waterHeight),
                            new Rectangle((int)m_waterOverlayStart.X, (int)m_waterOverlayStart.Y + (m_maxWaterHeight - m_waterHeight), m_WaterWidth, m_waterHeight),
                            new Color(255,255,255,180));
                spriteBatch.End();
            }
            if (m_gameStates == GameState.GameOver)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(Runes, "G A M E  O V E R", m_gameOverLocation, Color.Yellow);
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }

        private void StartNewLevel()
        {
            m_currentLevel++;
            m_floodCount = 0.0f;
            m_linesCompletedThisLevel = 0;
            m_floodIncreaseAmount += m_floodAccelerationPerLevel;
            m_gameBoard.ClearBoard();
            m_gameBoard.GenerateNewPieces(false);
        }

        private int DetermineScore(int squareCount)
        {
            return (int)((Math.Pow((squareCount / 5), 2) + squareCount) * 10);
        }

        private void CheckScoringChain(List<Vector2> waterChain)
        {
            if (waterChain.Count > 0)
            {
                Vector2 lastPipe = waterChain[waterChain.Count - 1];
                if (lastPipe.X == GameBoard.m_gameBoardWidth - 1)
                {
                    if (m_gameBoard.HasConnector((int)lastPipe.X, (int)lastPipe.Y, "Right"))
                    {
                        m_playerScore += DetermineScore(waterChain.Count);
                        m_linesCompletedThisLevel++;
                        m_floodCount = MathHelper.Clamp(m_floodCount - (DetermineScore(waterChain.Count) / 10), 0.0f, 100.0f);
                        m_scoreZooms.Enqueue(new ScoreZoom("+" + DetermineScore(waterChain.Count).ToString(), new Color(1.0f, 0.0f, 0.0f, 0.4f)));
                        foreach (Vector2 scoringSquare in waterChain)
                        {
                            m_gameBoard.AddFadingPiece((int)scoringSquare.X, (int)scoringSquare.Y, m_gameBoard.GetSquare((int)scoringSquare.X, (int)scoringSquare.Y));
                            m_gameBoard.SetSquare((int)scoringSquare.X, (int)scoringSquare.Y, "Empty");
                        }

                        if (m_linesCompletedThisLevel >= 8)
                        {
                            StartNewLevel();
                        }
                    }
                }
            }
        }

        private void HandleMouseInput(MouseState mouseState)
        {
            int x = ((mouseState.X - (int)m_gameBoardDisplayOrigin.X) / GamePiece.m_pieceWidth);
            int y = ((mouseState.Y - (int)m_gameBoardDisplayOrigin.Y) / GamePiece.m_pieceHeight);

            if ((x >= 0) && (x < GameBoard.m_gameBoardWidth) && (y >= 0) && (y < GameBoard.m_gameBoardHeight))
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    m_gameBoard.AddRotatingPiece(x, y, m_gameBoard.GetSquare(x, y), false);
                    m_gameBoard.RotatePiece(x, y, false);
                    m_timeSinceLastInput = 0.0f;
                }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    m_gameBoard.AddRotatingPiece(x, y, m_gameBoard.GetSquare(x, y), true);
                    m_gameBoard.RotatePiece(x, y, true);
                    m_timeSinceLastInput = 0.0f;
                }
            }
        }

        private void DrawEmptyPiece(int pixelX, int pixelY)
        {
            spriteBatch.Draw(playingPieces, 
                new Rectangle(pixelX, pixelY, GamePiece.m_pieceWidth, GamePiece.m_pieceHeight), 
                m_emptyPiece, 
                Color.White);
        }

        private void DrawStandardPiece(int x, int y, int pixelX, int pixelY)
        {
            spriteBatch.Draw(playingPieces, 
                new Rectangle(pixelX, pixelY, GamePiece.m_pieceWidth, GamePiece.m_pieceHeight), 
                m_gameBoard.GetSourceRect(x, y), 
                Color.White);
        }

        private void DrawFallingPiece(int pixelX, int pixelY, string positionName)
        {
            spriteBatch.Draw(playingPieces, 
                new Rectangle(pixelX, pixelY - m_gameBoard.m_fallingPieces[positionName].m_verticalOffset, GamePiece.m_pieceWidth, GamePiece.m_pieceHeight), 
                m_gameBoard.m_fallingPieces[positionName].GetSourceRect(), 
                Color.White);    
        }

        private void DrawFadingPiece(int pixelX, int pixelY, string positionName)
        {
            spriteBatch.Draw(playingPieces, 
                new Rectangle(pixelX, pixelY, GamePiece.m_pieceWidth, GamePiece.m_pieceHeight), 
                m_gameBoard.m_fadingPieces[positionName].GetSourceRect(), 
                Color.White * m_gameBoard.m_fadingPieces[positionName].m_alphaLevel);
        }

        private void DrawRotatingPieces(int pixelX, int pixelY, string positionName)
        {
            spriteBatch.Draw(playingPieces, 
                new Rectangle(pixelX + (GamePiece.m_pieceWidth / 2), pixelY + (GamePiece.m_pieceHeight / 2), GamePiece.m_pieceWidth, GamePiece.m_pieceHeight),
                m_gameBoard.m_rotatingPieces[positionName].GetSourceRect(), 
                Color.White, 
                m_gameBoard.m_rotatingPieces[positionName].RotationAmount, 
                new Vector2(GamePiece.m_pieceWidth / 2, GamePiece.m_pieceHeight / 2), 
                SpriteEffects.None, 
                0.0f);
        }

        private void UpdateScoreZoom()
        {
            int dequeueCounter = 0;
            foreach (ScoreZoom zoom in m_scoreZooms)
            {
                zoom.Update();
                if (zoom.IsCompleted)
                {
                    dequeueCounter++;
                }
            }
            for (int d = 0; d < dequeueCounter; d++)
            {
                m_scoreZooms.Dequeue();
            }
        }
    }
}
