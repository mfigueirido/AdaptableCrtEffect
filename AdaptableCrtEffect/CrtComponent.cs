using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AdaptableCrtEffect
{
    public class CrtComponent
    {
        RenderTarget2D _intermediatePass;
        RenderTarget2D _output;

        Effect _effect;

        EffectTechnique _effectTechniqueBase;
        EffectTechnique _effectTechniqueBaseAndSmoothing;
        EffectTechnique _effectTechniqueCrtBaseAndSmoothingPass1;
        EffectTechnique _effectTechniqueCrtScan;
        EffectTechnique _effectTechniqueCrtScanCa;
        EffectTechnique _effectTechniqueCrtSmoothingPass2;

        EffectTechnique _baseTechniqueToUse;
        EffectTechnique _scanTechniqueToUse;

        EffectParameter _originalSizeParameter;
        EffectParameter _outputSizeParameter;
        EffectParameter _pixelWidthParameter;
        EffectParameter _pixelHeightParameter;

        EffectParameter _exposureParameter;
        EffectParameter _vibranceParameter;

        EffectParameter _smoothingWeightBParameter;
        EffectParameter _smoothingWeightSParameter;

        EffectParameter _crtSmoothingWeightP1BParameter;
        EffectParameter _crtSmoothingWeightP1HParameter;
        EffectParameter _crtSmoothingWeightP1VParameter;
        EffectParameter _crtSmoothingWeightP2BParameter;
        EffectParameter _crtSmoothingWeightP2HParameter;
        EffectParameter _crtSmoothingWeightP2VParameter;

        EffectParameter _scanMaskStrenghtParameter;
        EffectParameter _scanScaleParameter;
        EffectParameter _scanKernelShapeParameter;
        EffectParameter _scanBrightnessBoostParameter;

        EffectParameter _warpXParameter;
        EffectParameter _warpYParameter;

        EffectParameter _caRedOffsetParameter;
        EffectParameter _caBlueOffsetParameter;

        public void LoadContent()
        {
            if (_effect == null || _effect.IsDisposed)
            {
                _effect = GameHelper.Content.Load<Effect>(@"Crt");

                _effectTechniqueBase = _effect.Techniques["Base"];
                _effectTechniqueBaseAndSmoothing = _effect.Techniques["BaseAndSmoothing"];
                _effectTechniqueCrtBaseAndSmoothingPass1 = _effect.Techniques["CrtBaseAndSmoothingPass1"];
                _effectTechniqueCrtScan = _effect.Techniques["CrtScan"];
                _effectTechniqueCrtScanCa = _effect.Techniques["CrtScanCa"];
                _effectTechniqueCrtSmoothingPass2 = _effect.Techniques["CrtSmoothingPass2"];

                _originalSizeParameter = _effect.Parameters["OriginalSize"];
                _outputSizeParameter = _effect.Parameters["OutputSize"];
                _pixelWidthParameter = _effect.Parameters["PixelWidth"];
                _pixelHeightParameter = _effect.Parameters["PixelHeight"];

                _exposureParameter = _effect.Parameters["Exposure"];
                _vibranceParameter = _effect.Parameters["Vibrance"];

                _smoothingWeightBParameter = _effect.Parameters["SmoothingWeightB"];
                _smoothingWeightSParameter = _effect.Parameters["SmoothingWeightS"];

                _crtSmoothingWeightP1BParameter = _effect.Parameters["CrtSmoothingWeightP1B"];
                _crtSmoothingWeightP1HParameter = _effect.Parameters["CrtSmoothingWeightP1H"];
                _crtSmoothingWeightP1VParameter = _effect.Parameters["CrtSmoothingWeightP1V"];
                _crtSmoothingWeightP2BParameter = _effect.Parameters["CrtSmoothingWeightP2B"];
                _crtSmoothingWeightP2HParameter = _effect.Parameters["CrtSmoothingWeightP2H"];
                _crtSmoothingWeightP2VParameter = _effect.Parameters["CrtSmoothingWeightP2V"];

                _scanMaskStrenghtParameter = _effect.Parameters["ScanMaskStrenght"];
                _scanScaleParameter = _effect.Parameters["ScanScale"];
                _scanKernelShapeParameter = _effect.Parameters["ScanKernelShape"];
                _scanBrightnessBoostParameter = _effect.Parameters["ScanBrightnessBoost"];

                _warpXParameter = _effect.Parameters["WarpX"];
                _warpYParameter = _effect.Parameters["WarpY"];

                _caRedOffsetParameter = _effect.Parameters["CaRedOffset"];
                _caBlueOffsetParameter = _effect.Parameters["CaBlueOffset"];
            }
        }

        public void ApplySettings()
        {
            PostProcessingHelper.CreateRenderTarget(
                ref _intermediatePass,
                PostProcessingHelper.PresentationWidth,
                PostProcessingHelper.PresentationHeight,
                SurfaceFormat.HalfVector4);

            PostProcessingHelper.CreateRenderTarget(
                ref _output,
                PostProcessingHelper.PresentationWidth,
                PostProcessingHelper.PresentationHeight,
                SurfaceFormat.HalfVector4);

            _originalSizeParameter.SetValue(new Vector2(PostProcessingHelper.BaseWidth, PostProcessingHelper.BaseHeight));
            _outputSizeParameter.SetValue(new Vector2(PostProcessingHelper.PresentationWidth, PostProcessingHelper.PresentationHeight));
            _pixelWidthParameter.SetValue(1f / PostProcessingHelper.PresentationWidth);
            _pixelHeightParameter.SetValue(1f / PostProcessingHelper.PresentationHeight);

            _exposureParameter.SetValue(PostProcessingSettings.Exposure);
            _vibranceParameter.SetValue(PostProcessingSettings.Vibrance);

            _smoothingWeightBParameter.SetValue(0.68f);
            _smoothingWeightSParameter.SetValue(0.32f);

            _crtSmoothingWeightP1BParameter.SetValue(0.7f);
            _crtSmoothingWeightP1HParameter.SetValue(0.15f);
            _crtSmoothingWeightP1VParameter.SetValue(0.15f);
            _crtSmoothingWeightP2BParameter.SetValue(0.3f);
            _crtSmoothingWeightP2HParameter.SetValue(0.5f);
            _crtSmoothingWeightP2VParameter.SetValue(0.2f);

            float scanMaskStrenght = 0.525f;
            float scanBrightnessBoost = PostProcessingSettings.ScanBrightnessBoost;

            if (PostProcessingSettings.CrtMode == CrtModeOption.SoftScans)
            {
                scanMaskStrenght *= .58f;
                scanBrightnessBoost = 1f + (scanBrightnessBoost - 1f) * .4f;
            }
            else if (PostProcessingSettings.CrtMode == CrtModeOption.NoScans || PostProcessingSettings.CrtMode == CrtModeOption.NoScansNoCurvature)
            {
                scanMaskStrenght = 0f;
                scanBrightnessBoost = 1f;
            }

            _scanMaskStrenghtParameter.SetValue(scanMaskStrenght);
            _scanScaleParameter.SetValue(-8.0f);
            _scanKernelShapeParameter.SetValue(2.0f);
            _scanBrightnessBoostParameter.SetValue(scanBrightnessBoost);

            if (PostProcessingSettings.CrtMode == CrtModeOption.NoScansNoCurvature)
            {
                _warpXParameter.SetValue(0f);
                _warpYParameter.SetValue(0f);
            }
            else
            {
                _warpXParameter.SetValue(0.01f);
                _warpYParameter.SetValue(0.02f);
            }

            _caRedOffsetParameter.SetValue(0.0006f);
            _caBlueOffsetParameter.SetValue(0.0006f);

            _baseTechniqueToUse = PostProcessingSettings.IsSmoothingFilterEnabled ? _effectTechniqueBaseAndSmoothing : _effectTechniqueBase;
            _scanTechniqueToUse = PostProcessingSettings.IsChromaticAberrationEnabled ? _effectTechniqueCrtScanCa : _effectTechniqueCrtScan;
        }

        public RenderTarget2D Update(Texture2D source)
        {
            var device = GameHelper.GraphicsDevice;
            var spriteBatch = GameHelper.SpriteBatch;

            if (PostProcessingSettings.CrtMode == CrtModeOption.Disabled)
            {
                // Base ////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////
                device.SetRenderTarget(_output);
                device.Clear(Color.Transparent);

                _effect.CurrentTechnique = _baseTechniqueToUse;
                spriteBatch.Begin(0, BlendState.AlphaBlend, null, null, null, _effect);
                spriteBatch.Draw(source, new Rectangle(0, 0, _output.Width, _output.Height), Color.White);
                spriteBatch.End();

                device.SetRenderTarget(null);
                ////////////////////////////////////////////////////////////////////////////////////////////////
            }
            else
            {
                // Base and smoothing pass 1 ///////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////
                device.SetRenderTarget(_output);
                device.Clear(Color.Transparent);

                _effect.CurrentTechnique = _effectTechniqueCrtBaseAndSmoothingPass1;
                spriteBatch.Begin(0, BlendState.AlphaBlend, null, null, null, _effect);
                spriteBatch.Draw(source, new Rectangle(0, 0, _output.Width, _output.Height), Color.White);
                spriteBatch.End();

                device.SetRenderTarget(null);
                ////////////////////////////////////////////////////////////////////////////////////////////////

                // Scan ////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////
                device.SetRenderTarget(_intermediatePass);
                device.Clear(Color.Transparent);

                _effect.CurrentTechnique = _scanTechniqueToUse;
                spriteBatch.Begin(0, BlendState.AlphaBlend, null, null, null, _effect);
                spriteBatch.Draw(_output, new Rectangle(0, 0, _output.Width, _output.Height), Color.White);
                spriteBatch.End();

                device.SetRenderTarget(null);
                ////////////////////////////////////////////////////////////////////////////////////////////////

                // Smoothing pass 2 ////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////
                device.SetRenderTarget(_output);
                device.Clear(Color.Transparent);

                _effect.CurrentTechnique = _effectTechniqueCrtSmoothingPass2;
                spriteBatch.Begin(0, BlendState.AlphaBlend, null, null, null, _effect);
                spriteBatch.Draw(_intermediatePass, new Rectangle(0, 0, _output.Width, _output.Height), Color.White);
                spriteBatch.End();

                device.SetRenderTarget(null);
                ////////////////////////////////////////////////////////////////////////////////////////////////
            }

            return _output;
        }
    }
}
