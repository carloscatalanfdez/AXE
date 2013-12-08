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

using bEngine;
using AXE.Game;
using AXE.Game.Screens;
using AXE.Game.Control;
using AXE.Game.Utils;

namespace AXE
{
    /// This is the main type for your game
    public class AxeGame : bGame
    {
        public Effect effect;
        Screen screen;

        public ResourceManager res;

        public int FramesPerSecond
        {
            get
            {
                return (int)(1.0f / millisecondsPerFrame * 1000);
            }
        }

        // Debug
        int currentTechnique;
        RenderTarget2D renderTarget;
        Texture2D renderResult;
        bool switchFullscreenThisStep;
        VisualDebugger vizdeb;
        bool shouldDebugStepByStep = false;

        protected override void initSettings()
        {
            base.initSettings();

            horizontalZoom = 3;
            verticalZoom = 3;

            width = 320;
            height = 256;
            fullscreen = true;

            bgColor = Color.DarkGray;

            currentTechnique = 3;
            switchFullscreenThisStep = false;

            res = ResourceManager.get();
            res.init(this);

            vizdeb = VisualDebugger.get(this);
        }

        protected void generateRenderTarget()
        {
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
        }

        protected override void LoadContent()
        {
            generateRenderTarget();

            base.LoadContent();

            res.loadContent();

            effect = res.effect;
            effect.CurrentTechnique = effect.Techniques[currentTechnique];
            Window.Title = "AXE (" + effect.CurrentTechnique.Name + ")";
            effect.Parameters["ImageHeight"].SetValue(GraphicsDevice.Viewport.Height);
            effect.Parameters["Contrast"].SetValue(1.0f);
            effect.Parameters["Brightness"].SetValue(0.2f);
            effect.Parameters["DesaturationAmount"].SetValue(1.0f);
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            renderResult.Dispose();
            renderTarget.Dispose();
        }

        protected override void Initialize()
        {
            Tools.step = 0;
            // switchFullScreen();
            Controller.getInstance().setGame(this);
            Controller.getInstance().onMenuStart();
            // changeWorld(new LogoScreen());

            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (!shouldDebugStepByStep)
                base.Update(gameTime);
            else
            {
                input.update();

                if (input.pressed(Keys.P))
                {
                    shouldDebugStepByStep = false;
                }
                else if (input.pressed(Keys.Right))
                {
                    base.Update(gameTime);
                }
            }
        }

        public override void update(GameTime gameTime)
        {
            if (input.pressed(Keys.P))
            {
                shouldDebugStepByStep = true;
            }

            Common.GameInput.getInstance(PlayerIndex.One).update();
            Common.GameInput.getInstance(PlayerIndex.Two).update();

            if (Common.GameInput.getInstance(PlayerIndex.One).pressed(Common.PadButton.coin))
            {
                if (GameData.get().coins > 0)
                {
                    GameData.get().credits++;
                    GameData.get().coins--;
                    GameData.saveGame();
                }
            }

            if (input.pressed(Keys.T))
            {
                Controller.getInstance().onGameOver();
            }

            if (Common.GameInput.getInstance(PlayerIndex.One).pressed(Common.PadButton.debug))
                bConfig.DEBUG = !bConfig.DEBUG;
            else if (input.pressed(Keys.D0))
            {
                GameData.get().coins += 100;
                GameData.saveGame();
            }

            if (input.pressed(Keys.Space))
            {
                currentTechnique = (currentTechnique + 1) % effect.Techniques.Count;
                effect.CurrentTechnique = effect.Techniques[currentTechnique];
                Window.Title = "AXE (" + effect.CurrentTechnique.Name + ")";
            }

            // Allows the game to exit
            if (input.pressed(Buttons.Back) || input.pressed(Keys.Escape))
                this.Exit();
            // Handles full screen mode
            else if (input.pressed(Keys.F4) && !switchFullscreenThisStep)
            {
                switchFullscreenThisStep = true;
            }
            // Increases milliseconds per frame (slows down game)
            else if (input.pressed(Keys.Add))
            {
                millisecondsPerFrame += 5.0;
            }
            // Decreases milliseconds per frame (speeds up game)
            else if (input.pressed(Keys.Subtract))
            {
                millisecondsPerFrame -= 5.0;
            }
            // Takes a screenshot
            else if (input.pressed(Keys.F12))
            {
                screenshot();
            }
            // 
            else if (input.pressed(Keys.D8))
            {
                horizontalZoom--;
                verticalZoom--;
            }
            else if (input.pressed(Keys.D9))
            {
                horizontalZoom++;
                verticalZoom++;
            }


            vizdeb.update();

            Tools.step++;
        }

        void switchFullScreen()
        {
            if (Controller.getInstance().canSwitchFullscreen())
            {
                int rw, rh;
                if (graphics.IsFullScreen)
                {
                    rw = width * (int)horizontalZoom;
                    rh = height * (int)verticalZoom;
                }
                else
                {
                    rw = GraphicsDevice.DisplayMode.Width;
                    rh = GraphicsDevice.DisplayMode.Height;
                }

                Resolution.SetResolution(rw, rh, !graphics.IsFullScreen);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);

            Resolution.BeginDraw();
            // Generate resolution render matrix 
            Matrix matrix = Resolution.getTransformationMatrix();

            spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null,
                    RasterizerState.CullCounterClockwise,
                    null,
                    matrix);
            // spriteBatch.Begin();

            GraphicsDevice.Clear(bgColor);

            render(gameTime);

            // Render world if available
            if (world != null)
                world.render(gameTime, spriteBatch, Matrix.Identity/*matrix*/);

            // Transition
            if (gamestateTransition != null)
                gamestateTransition.render(spriteBatch);

            spriteBatch.End();

            if (renderTarget != null && renderTarget.IsDisposed == false)
            {
                GraphicsDevice.SetRenderTarget(null);
                renderResult = (Texture2D)renderTarget;
                GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.DarkSlateBlue, 1.0f, 0);
                using (SpriteBatch sprite = new SpriteBatch(GraphicsDevice))
                {
                    try
                    {
                        // Background
                        sprite.Begin();
                        sprite.Draw(res.bgTest, new Rectangle(0, 0, GraphicsDevice.DisplayMode.Width, GraphicsDevice.DisplayMode.Height), Color.Wheat);
                        sprite.End();

                        // Main window
                        sprite.Begin(SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            SamplerState.PointClamp,
                            null,
                            RasterizerState.CullCounterClockwise,
                            effect);
                        Rectangle rect = new Rectangle(
                            (int) ((graphics.PreferredBackBufferWidth / 2) - (width * horizontalZoom / 2)),
                            (int) ((graphics.PreferredBackBufferHeight / 2) - (height * verticalZoom / 2)),
                            (int) (width * horizontalZoom),
                            (int) (height * verticalZoom));
                        sprite.Draw(renderResult, new Vector2(rect.X, rect.Y), rect, 
                            Color.White, 0, new Vector2(0, 0), 1f, 
                            SpriteEffects.None, 1);
                        sprite.End();

                        // Front
                        sprite.Begin();
                        vizdeb.render(sprite);
                        sprite.End();
                    }
                    catch (Exception e)
                    {
                        string n = e.Message;
                    }
                }
            }

            if (switchFullscreenThisStep)
            {
                switchFullscreenThisStep = false;
                switchFullScreen();
            }
        }

        public void changeWorld(Screen screen)
        {
            this.screen = screen;
            base.changeWorld(screen);
        }
    }
}
