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

namespace AXE
{
    /// This is the main type for your game
    public class AxeGame : bGame
    {
        public Effect effect;
        Screen screen;

        // Debug
        int currentTechnique;

        protected override void initSettings()
        {
            base.initSettings();

            horizontalZoom = 3;
            verticalZoom = 3;

            width = 320;
            height = 256;

            bgColor = Color.Black;

            currentTechnique = 0;
        }

        protected override void Initialize()
        {
            effect = Content.Load<Effect>("Assets/scanlines");
            effect.CurrentTechnique = effect.Techniques[0];
            Controller.getInstance().setGame(this);
            Controller.getInstance().onMenuStart();
            /*changeWorld(new LogoScreen());*/
            base.Initialize();
        }

        public override void update(GameTime gameTime)
        {
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
                effect.Parameters["ImageHeight"].SetValue(GraphicsDevice.Viewport.Height);
                effect.Parameters["Contrast"].SetValue(1.0f);
                effect.Parameters["Brightness"].SetValue(0.2f);
                effect.Parameters["DesaturationAmount"].SetValue(1.0f);
            }

            base.update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            Resolution.BeginDraw();
            // Generate resolution render matrix 
            Matrix matrix = Resolution.getTransformationMatrix();

            spriteBatch.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null,
                    RasterizerState.CullCounterClockwise,
                    effect,
                    matrix);

            GraphicsDevice.Clear(bgColor);

            render(gameTime);

            // Render world if available
            if (world != null)
                world.render(gameTime, spriteBatch, matrix);

            // Transition
            if (gamestateTransition != null)
                gamestateTransition.render(spriteBatch);

            spriteBatch.End();
        }

        public void changeWorld(Screen screen)
        {
            this.screen = screen;
            base.changeWorld(screen);
        }
    }
}
