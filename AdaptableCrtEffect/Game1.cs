using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace AdaptableCrtEffect
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        BloomComponent _bloom = new BloomComponent();
        CrtComponent _crt = new CrtComponent();
        Texture2D _gameTexture;
        SpriteFont _font;
        RenderTarget2D _renderTarget;
        string _text;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
        }

        protected override void Initialize()
        {
            PostProcessingHelper.BaseWidth = 320;
            PostProcessingHelper.BaseHeight = 180;
            PostProcessingHelper.PresentationWidth = 1280;
            PostProcessingHelper.PresentationHeight = 720;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            GameHelper.Content = Content;
            GameHelper.GraphicsDevice = GraphicsDevice;
            GameHelper.SpriteBatch = _spriteBatch;

            _bloom.LoadContent();
            _bloom.ApplySettings();
            _crt.LoadContent();
            _crt.ApplySettings();

            _gameTexture = Content.Load<Texture2D>("GameImage");
            _font = Content.Load<SpriteFont>("DefaultFont");

            PostProcessingHelper.CreateRenderTarget(ref _renderTarget, _gameTexture.Width, _gameTexture.Height);
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();

            bool settingsChanged = false;

            if (InputManager.KeyPushed(Keys.D1))
            {
                PostProcessingSettings.IsBloomEnabled = !PostProcessingSettings.IsBloomEnabled;
                settingsChanged = true;
            }

            if (InputManager.KeyPushed(Keys.D2))
            {
                PostProcessingSettings.IsSmoothingFilterEnabled = !PostProcessingSettings.IsSmoothingFilterEnabled;
                settingsChanged = true;
            }

            if (InputManager.KeyPushed(Keys.D3))
            {
                int enumMembers = Enum.GetNames(typeof(CrtModeOption)).Length;

                PostProcessingSettings.CrtMode += 1;

                if ((int)PostProcessingSettings.CrtMode > enumMembers - 1)
                    PostProcessingSettings.CrtMode = 0;

                settingsChanged = true;
            }

            if (InputManager.KeyPushed(Keys.D4))
            {
                PostProcessingSettings.IsChromaticAberrationEnabled = !PostProcessingSettings.IsChromaticAberrationEnabled;
                settingsChanged = true;
            }

            if (settingsChanged)
            {
                _bloom.ApplySettings();
                _crt.ApplySettings();
            }

            _text = "\n" + string.Format("Base resolution: {0}x{1}", PostProcessingHelper.BaseWidth, PostProcessingHelper.BaseHeight);
            _text += "\n" + string.Format("Presentation resolution: {0}x{1}", PostProcessingHelper.PresentationWidth, PostProcessingHelper.PresentationHeight);
            _text += "\n" + string.Format("Bloom enabled (1): {0}", PostProcessingSettings.IsBloomEnabled);
            _text += "\n" + string.Format("Smoothing filter enabled (2): {0}", PostProcessingSettings.IsSmoothingFilterEnabled);
            _text += "\n" + string.Format("Crt mode (3): {0}", PostProcessingSettings.CrtMode);
            _text += "\n" + string.Format("Chromatic aberration enabled (4): {0}", PostProcessingSettings.IsChromaticAberrationEnabled);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            DrawEffects();
            DrawText();

            base.Draw(gameTime);
        }

        void DrawEffects()
        {
            var output = _gameTexture;

            if (PostProcessingSettings.IsBloomEnabled)
            {
                _bloom.Update(_gameTexture, _renderTarget);
                output = _renderTarget;
            }

            output = _crt.Update(output);

            _spriteBatch.Begin();
            _spriteBatch.Draw(output, new Rectangle(0, 0, _renderTarget.Width, _renderTarget.Height), Color.White);
            _spriteBatch.End();
        }

        void DrawText()
        {
            if (!string.IsNullOrEmpty(_text))
            {
                _spriteBatch.Begin();
                _spriteBatch.DrawString(_font, _text, new Vector2(40, 40), Color.White);
                _spriteBatch.End();
            }
        }
    }
}