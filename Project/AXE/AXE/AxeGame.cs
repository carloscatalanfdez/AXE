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
using System.Reflection;
using System.Collections;

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

        bool needsReload = false;

        protected override void initSettings()
        {
            base.initSettings();

            width = 320;
            height = 320;

            uint scale = getHighestScale(3, 3);
            horizontalZoom = scale;
            verticalZoom = scale;

            fullscreen = true;

            bgColor = Color.DarkGray;

            currentTechnique = 3;
            switchFullscreenThisStep = false;

            res = ResourceManager.get();
            res.init(this);

            vizdeb = VisualDebugger.get(this);
        }

        /**
         * Returns the highest scale of the game (between [llimit, hlimit] 
         * that fits the screen
         */
        protected uint getHighestScale(uint llimit, uint hlimit)
        {
            DisplayMode screenRes = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            for (uint i = hlimit; i > llimit; i--)
            {
                if (width * i < screenRes.Width && height * i < screenRes.Height)
                    return i;
            }

            return llimit;
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

            if (needsReload)
            {
                List<object> traversedItems = new List<object>();
                traversedItems.Add(this);
                traverseClassTree(this.world, (x) =>
                {
                    if (x is IReloadable)
                    {
                        (x as IReloadable).reloadContent();
                        return true;
                    }
                    
                    return false;
                }, traversedItems);

                needsReload = false;
            }
        }

        public static void traverseClassTree(object root, Func<object, bool> visitFunction, List<object> traversedObjects)
        {
            // Avoid repetition (not the most efficient way, but it's good enough)
            if (root == null || traversedObjects.Contains(root))
                return;
            traversedObjects.Add(root);

            Type t = root.GetType();

            // Only check objects whithin our namespaces (otherwise we get a shit-ton of objects)
            if (!t.Namespace.StartsWith("bEngine") && !t.Namespace.StartsWith("AXE") && !t.Namespace.StartsWith("System.Collections.Generic"))
                return;

            visitFunction(root);

            // Is array? Probably not working, but who cares
            if (t.IsArray)
            {
                foreach (object element in (root as Array))
                {
                    traverseClassTree(element, visitFunction, traversedObjects);
                }
            } 
            // Is a collection?
            else if (root is ICollection)
            {
                foreach (object value in (root as ICollection))
                {
                    traverseClassTree(value, visitFunction, traversedObjects);
                }
            }
            // Is anything else?
            else
            {
                FieldInfo[] fieldInfos = t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach (FieldInfo info in fieldInfos)
                {
                    object node = info.GetValue(root);
                    traverseClassTree(node, visitFunction, traversedObjects);
                }
            }
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            renderResult.Dispose();
            renderTarget.Dispose();

            needsReload = true;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Tools.step = 0;
            // switchFullScreen();
            Controller.getInstance().setGame(this);
            Controller.getInstance().launch();
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
                        Rectangle rect = new Rectangle(0, 0, renderResult.Width, renderResult.Height);
                        //Rectangle rect = new Rectangle(
                        //    (int)((graphics.PreferredBackBufferWidth / 2) - (width * horizontalZoom / 2)),
                        //    (int)((graphics.PreferredBackBufferHeight / 2) - (height * verticalZoom / 2)),
                        //    (int)(width * horizontalZoom),
                        //    (int)(height * verticalZoom));
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
