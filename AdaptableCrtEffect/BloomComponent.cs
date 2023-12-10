using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace AdaptableCrtEffect
{
    public class BloomComponent
    {
        RenderTarget2D _bloomRenderTarget2DMip0;
        RenderTarget2D _bloomRenderTarget2DMip1;
        RenderTarget2D _bloomRenderTarget2DMip2;
        RenderTarget2D _bloomRenderTarget2DMip3;
        RenderTarget2D _bloomRenderTarget2DMip4;
        RenderTarget2D _bloomRenderTarget2DMip5;

        Effect _bloomEffect;

        EffectPass _bloomPassExtract;
        EffectPass _bloomPassExtractLuminance;
        EffectPass _bloomPassDownsample;
        EffectPass _bloomPassUpsample;
        EffectPass _bloomPassUpsampleLuminance;

        EffectParameter _bloomParameterScreenTexture;
        EffectParameter _bloomHalfPixelParameter;
        EffectParameter _bloomDownsampleOffsetParameter;
        EffectParameter _bloomUpsampleOffsetParameter;
        EffectParameter _bloomStrengthParameter;
        EffectParameter _bloomThresholdParameter;

        Effect _bloomCombineEffect;

        EffectTechnique _bloomTechniqueCombine;
        EffectTechnique _bloomTechniqueSaturate;

        EffectParameter _bloomCombineBLTextureParameter;
        EffectParameter _bloomCombineBLBaseTextureParameter;
        EffectParameter _bloomCombineBLIntensityParameter;
        EffectParameter _bloomCombineBLSaturationParameter;

        float _bloomRadius1 = 1.0f;
        float _bloomRadius2 = 1.0f;
        float _bloomRadius3 = 1.0f;
        float _bloomRadius4 = 1.0f;
        float _bloomRadius5 = 1.0f;

        float _bloomStrength1 = 1.0f;
        float _bloomStrength2 = 1.0f;
        float _bloomStrength3 = 1.0f;
        float _bloomStrength4 = 1.0f;
        float _bloomStrength5 = 1.0f;

        public float BloomStrengthMultiplier = 1.0f;

        float _radiusMultiplier = 1.0f;

        public bool BloomUseLuminance = true;
        public int BloomDownsamplePasses = 5;

        public BloomPresets BloomPreset
        {
            get { return _bloomPreset; }
            set
            {
                if (_bloomPreset == value) return;

                _bloomPreset = value;
                SetBloomPreset(_bloomPreset);
            }
        }
        BloomPresets _bloomPreset;


        Texture2D BloomScreenTexture { set { _bloomParameterScreenTexture.SetValue(value); } }

        Vector2 HalfPixel
        {
            get { return _halfPixel; }
            set
            {
                if (value != _halfPixel)
                {
                    _halfPixel = value;
                    _bloomHalfPixelParameter.SetValue(_halfPixel);
                }
            }
        }
        Vector2 _halfPixel;

        float BloomStrength
        {
            get { return _bloomStrength; }
            set
            {
                if (Math.Abs(_bloomStrength - value) > 0.001f)
                {
                    _bloomStrength = value;
                    _bloomStrengthParameter.SetValue(_bloomStrength * BloomStrengthMultiplier);
                }

            }
        }
        float _bloomStrength;

        public float BloomThreshold
        {
            get { return _bloomThreshold; }
            set
            {
                if (Math.Abs(_bloomThreshold - value) > 0.001f)
                {
                    _bloomThreshold = value;
                    _bloomThresholdParameter.SetValue(_bloomThreshold);
                }
            }
        }
        float _bloomThreshold;

        Vector2 BloomInverseResolution
        {
            get { return _bloomInverseResolution; }
            set
            {
                if (value != _bloomInverseResolution)
                    _bloomInverseResolution = value;
            }
        }
        Vector2 _bloomInverseResolution;

        public float BloomStreakLength
        {
            get { return _bloomStreakLength; }
            set
            {
                if (Math.Abs(_bloomStreakLength - value) > 0.001f)
                    _bloomStreakLength = value;
            }
        }
        float _bloomStreakLength;

        float BloomRadius
        {
            get { return _bloomRadius; }
            set
            {
                if (Math.Abs(_bloomRadius - value) > 0.001f)
                    _bloomRadius = value;
            }
        }
        float _bloomRadius;

        void UpdateDownsampleOffsetParameter()
        {
            var offset = _bloomInverseResolution;
            offset.X *= _bloomStreakLength;
            _bloomDownsampleOffsetParameter.SetValue(offset);
        }

        void UpdateUpsampleOffsetParameter()
        {
            var offset = _bloomInverseResolution;
            offset.X *= _bloomStreakLength;
            offset *= _bloomRadius * _radiusMultiplier;
            _bloomUpsampleOffsetParameter.SetValue(offset);
        }

        public void LoadContent()
        {
            if (_bloomEffect == null || _bloomEffect.IsDisposed)
            {
                _bloomEffect = GameHelper.Content.Load<Effect>("Bloom");
                _bloomHalfPixelParameter = _bloomEffect.Parameters["HalfPixel"];
                _bloomDownsampleOffsetParameter = _bloomEffect.Parameters["DownsampleOffset"];
                _bloomUpsampleOffsetParameter = _bloomEffect.Parameters["UpsampleOffset"];
                _bloomStrengthParameter = _bloomEffect.Parameters["Strength"];
                _bloomThresholdParameter = _bloomEffect.Parameters["Threshold"];

                _bloomParameterScreenTexture = _bloomEffect.Parameters["LinearSampler+ScreenTexture"];

                _bloomPassExtract = _bloomEffect.Techniques["Extract"].Passes[0];
                _bloomPassExtractLuminance = _bloomEffect.Techniques["ExtractLuminance"].Passes[0];
                _bloomPassDownsample = _bloomEffect.Techniques["Downsample"].Passes[0];
                _bloomPassUpsample = _bloomEffect.Techniques["Upsample"].Passes[0];
                _bloomPassUpsampleLuminance = _bloomEffect.Techniques["UpsampleLuminance"].Passes[0];
            }

            if (_bloomCombineEffect == null || _bloomCombineEffect.IsDisposed)
            {
                _bloomCombineEffect = GameHelper.Content.Load<Effect>("BloomCombine");

                _bloomTechniqueCombine = _bloomCombineEffect.Techniques["BloomCombine"];
                _bloomTechniqueSaturate = _bloomCombineEffect.Techniques["BloomSaturate"];

                _bloomCombineBLTextureParameter = _bloomCombineEffect.Parameters["BloomTexture"];
                _bloomCombineBLBaseTextureParameter = _bloomCombineEffect.Parameters["BaseTexture"];
                _bloomCombineBLIntensityParameter = _bloomCombineEffect.Parameters["BloomIntensity"];
                _bloomCombineBLSaturationParameter = _bloomCombineEffect.Parameters["BloomSaturation"];
            }
        }

        public void ApplySettings()
        {
            var bloomSettings = PostProcessingSettings.BloomSettings;

            int width = (int)(PostProcessingHelper.PresentationWidth * PostProcessingSettings.BloomSettings.Quality);
            int height = (int)(PostProcessingHelper.PresentationHeight * bloomSettings.Quality);

            var usage = bloomSettings.PreserveContents ? RenderTargetUsage.PreserveContents : RenderTargetUsage.DiscardContents;

            PostProcessingHelper.CreateRenderTarget(ref _bloomRenderTarget2DMip0, width, height, SurfaceFormat.HalfVector4);
            PostProcessingHelper.CreateRenderTarget(ref _bloomRenderTarget2DMip1, width / 2, height / 2, SurfaceFormat.HalfVector4, usage);
            PostProcessingHelper.CreateRenderTarget(ref _bloomRenderTarget2DMip2, width / 4, height / 4, SurfaceFormat.HalfVector4, usage);
            PostProcessingHelper.CreateRenderTarget(ref _bloomRenderTarget2DMip3, width / 8, height / 8, SurfaceFormat.HalfVector4, usage);
            PostProcessingHelper.CreateRenderTarget(ref _bloomRenderTarget2DMip4, width / 16, height / 16, SurfaceFormat.HalfVector4, usage);
            PostProcessingHelper.CreateRenderTarget(ref _bloomRenderTarget2DMip5, width / 32, height / 32, SurfaceFormat.HalfVector4, usage);

            SetBloomPreset(bloomSettings.Preset);
            _radiusMultiplier = bloomSettings.RadiusMultiplier;
            BloomThreshold = bloomSettings.Threshold;
            BloomStrengthMultiplier = bloomSettings.StrengthMultiplier;
            BloomUseLuminance = bloomSettings.UseLuminance;
            _bloomCombineBLIntensityParameter.SetValue(bloomSettings.Intensity);
            _bloomCombineBLSaturationParameter.SetValue(bloomSettings.Saturation);
        }

        void SetBloomPreset(BloomPresets preset)
        {
            switch (preset)
            {
                case BloomPresets.Wide:
                    {
                        _bloomStrength1 = 0.5f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 2;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 4.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 1.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.SuperWide:
                    {
                        _bloomStrength1 = 0.9f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 2;
                        _bloomStrength5 = 6;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 2.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 2.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Focussed:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 4.0f;
                        _bloomRadius4 = 2.0f;
                        _bloomRadius3 = 2.0f;
                        _bloomRadius2 = 2.0f;
                        _bloomRadius1 = 2.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Small:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 1;
                        _bloomRadius5 = 1;
                        _bloomRadius4 = 1;
                        _bloomRadius3 = 1;
                        _bloomRadius2 = 1;
                        _bloomRadius1 = 1;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
                case BloomPresets.Cheap:
                    {
                        _bloomStrength1 = 0.8f;
                        _bloomStrength2 = 2;
                        _bloomRadius2 = 2;
                        _bloomRadius1 = 2;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 2;
                        break;
                    }
                case BloomPresets.One:
                    {
                        _bloomStrength1 = 4f;
                        _bloomStrength2 = 1;
                        _bloomStrength3 = 1;
                        _bloomStrength4 = 1;
                        _bloomStrength5 = 2;
                        _bloomRadius5 = 1.0f;
                        _bloomRadius4 = 1.0f;
                        _bloomRadius3 = 1.0f;
                        _bloomRadius2 = 1.0f;
                        _bloomRadius1 = 1.0f;
                        BloomStreakLength = 1;
                        BloomDownsamplePasses = 5;
                        break;
                    }
            }
        }

        public void Update(Texture2D source, RenderTarget2D output)
        {
            var device = GameHelper.GraphicsDevice;

            device.RasterizerState = RasterizerState.CullNone;
            device.BlendState = BlendState.Opaque;

            // EXTRACT
            // We extract the bright values which are above the Threshold and save them to Mip0
            device.SetRenderTarget(_bloomRenderTarget2DMip0);

            BloomScreenTexture = source;

            int clientScale = (int)Math.Ceiling((float)PostProcessingHelper.PresentationHeight / PostProcessingHelper.BaseHeight);
            HalfPixel = new Vector2(1.0f / PostProcessingHelper.PresentationWidth, 1.0f / PostProcessingHelper.PresentationHeight) / clientScale;
            BloomInverseResolution = new Vector2(1.0f / PostProcessingHelper.PresentationWidth, 1.0f / PostProcessingHelper.PresentationHeight);

            device.SamplerStates[0] = SamplerState.LinearClamp;

            if (BloomUseLuminance) _bloomPassExtractLuminance.Apply();
            else _bloomPassExtract.Apply();
            QuadRenderer.RenderFitViewport();

            // Now downsample to the next lower mip texture
            if (BloomDownsamplePasses > 0)
            {
                //DOWNSAMPLE TO MIP1
                device.SetRenderTarget(_bloomRenderTarget2DMip1);

                BloomScreenTexture = _bloomRenderTarget2DMip0;
                UpdateDownsampleOffsetParameter();

                //Pass
                _bloomPassDownsample.Apply();
                QuadRenderer.RenderFitViewport();

                if (BloomDownsamplePasses > 1)
                {
                    //Our input resolution is halved, so our inverse 1/res. must be doubled
                    HalfPixel *= 2;
                    BloomInverseResolution *= 2;

                    //DOWNSAMPLE TO MIP2
                    device.SetRenderTarget(_bloomRenderTarget2DMip2);

                    BloomScreenTexture = _bloomRenderTarget2DMip1;
                    UpdateDownsampleOffsetParameter();

                    //Pass
                    _bloomPassDownsample.Apply();
                    QuadRenderer.RenderFitViewport();

                    if (BloomDownsamplePasses > 2)
                    {
                        HalfPixel *= 2;
                        BloomInverseResolution *= 2;

                        //DOWNSAMPLE TO MIP3
                        device.SetRenderTarget(_bloomRenderTarget2DMip3);

                        BloomScreenTexture = _bloomRenderTarget2DMip2;
                        UpdateDownsampleOffsetParameter();

                        //Pass
                        _bloomPassDownsample.Apply();
                        QuadRenderer.RenderFitViewport();

                        if (BloomDownsamplePasses > 3)
                        {
                            HalfPixel *= 2;
                            BloomInverseResolution *= 2;

                            //DOWNSAMPLE TO MIP4
                            device.SetRenderTarget(_bloomRenderTarget2DMip4);

                            BloomScreenTexture = _bloomRenderTarget2DMip3;
                            UpdateDownsampleOffsetParameter();

                            //Pass
                            _bloomPassDownsample.Apply();
                            QuadRenderer.RenderFitViewport();

                            if (BloomDownsamplePasses > 4)
                            {
                                HalfPixel *= 2;
                                BloomInverseResolution *= 2;

                                //DOWNSAMPLE TO MIP5
                                device.SetRenderTarget(_bloomRenderTarget2DMip5);

                                BloomScreenTexture = _bloomRenderTarget2DMip4;
                                UpdateDownsampleOffsetParameter();

                                //Pass
                                _bloomPassDownsample.Apply();
                                QuadRenderer.RenderFitViewport();

                                ChangeBlendState();

                                //UPSAMPLE TO MIP4
                                device.SetRenderTarget(_bloomRenderTarget2DMip4);

                                BloomScreenTexture = _bloomRenderTarget2DMip5;
                                BloomStrength = _bloomStrength5;
                                BloomRadius = _bloomRadius5;
                                UpdateUpsampleOffsetParameter();

                                if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                                else _bloomPassUpsample.Apply();
                                QuadRenderer.RenderFitViewport();

                                HalfPixel /= 2;
                                BloomInverseResolution /= 2;
                            }

                            ChangeBlendState();

                            //UPSAMPLE TO MIP3
                            device.SetRenderTarget(_bloomRenderTarget2DMip3);

                            BloomScreenTexture = _bloomRenderTarget2DMip4;
                            BloomStrength = _bloomStrength4;
                            BloomRadius = _bloomRadius4;
                            UpdateUpsampleOffsetParameter();

                            if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                            else _bloomPassUpsample.Apply();
                            QuadRenderer.RenderFitViewport();

                            HalfPixel /= 2;
                            BloomInverseResolution /= 2;
                        }

                        ChangeBlendState();

                        //UPSAMPLE TO MIP2
                        device.SetRenderTarget(_bloomRenderTarget2DMip2);

                        BloomScreenTexture = _bloomRenderTarget2DMip3;
                        BloomStrength = _bloomStrength3;
                        BloomRadius = _bloomRadius3;
                        UpdateUpsampleOffsetParameter();

                        if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                        else _bloomPassUpsample.Apply();
                        QuadRenderer.RenderFitViewport();

                        HalfPixel /= 2;
                        BloomInverseResolution /= 2;
                    }

                    ChangeBlendState();

                    //UPSAMPLE TO MIP1
                    device.SetRenderTarget(_bloomRenderTarget2DMip1);

                    BloomScreenTexture = _bloomRenderTarget2DMip2;
                    BloomStrength = _bloomStrength2;
                    BloomRadius = _bloomRadius2;
                    UpdateUpsampleOffsetParameter();

                    if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                    else _bloomPassUpsample.Apply();
                    QuadRenderer.RenderFitViewport();

                    HalfPixel /= 2;
                    BloomInverseResolution /= 2;
                }

                ChangeBlendState();

                //UPSAMPLE TO MIP0
                device.SetRenderTarget(_bloomRenderTarget2DMip0);

                BloomScreenTexture = _bloomRenderTarget2DMip1;
                BloomStrength = _bloomStrength1;
                BloomRadius = _bloomRadius1;
                UpdateUpsampleOffsetParameter();

                if (BloomUseLuminance) _bloomPassUpsampleLuminance.Apply();
                else _bloomPassUpsample.Apply();
                QuadRenderer.RenderFitViewport();
            }

            device.SamplerStates[1] = SamplerState.LinearClamp;

            // Combine base image and bloom image.
            _bloomCombineEffect.CurrentTechnique = _bloomTechniqueCombine;
            _bloomCombineBLBaseTextureParameter.SetValue(source);
            DrawFullscreenQuadRT(_bloomRenderTarget2DMip0, output, _bloomCombineEffect);
        }

        void ChangeBlendState()
        {
            GameHelper.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        /// <summary>
        /// Helper for drawing a texture into a render target, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuadRT(Texture2D texture, RenderTarget2D renderTarget, Effect effect)
        {
            var device = GameHelper.GraphicsDevice;
            var spriteBatch = GameHelper.SpriteBatch;
            device.SetRenderTarget(renderTarget);
            device.Clear(Color.Transparent);
            spriteBatch.Begin(0, BlendState.AlphaBlend, null, null, null, effect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.White);
            spriteBatch.End();
            device.SetRenderTarget(null);
        }
    }
}