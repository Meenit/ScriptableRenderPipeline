using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    // This enum is just here to centralize UniqueID values for skies provided with HDRP
    public enum SkyType
    {
        HDRISky = 1,
        ProceduralSky = 2,
        Gradient = 3,
    }

    // Keep this class first in the file. Otherwise it seems that the script type is not registered properly.
    [Serializable]
    public sealed class VisualEnvironment : VolumeComponent
    {
        public IntParameter skyType = new IntParameter(0);
        public FogTypeParameter fogType = new FogTypeParameter(FogType.None);

        public void PushFogShaderParameters(HDCamera hdCamera, CommandBuffer cmd)
        {
            if ((fogType.value != FogType.Volumetric) || (!hdCamera.frameSettings.IsEnable(FrameSettingsField.Volumetrics)))
            {
                // If the volumetric fog is not used, we need to make sure that all rendering passes
                // (not just the atmospheric scattering one) receive neutral parameters.
                VolumetricFog.PushNeutralShaderParameters(cmd);
            }

            if (!hdCamera.frameSettings.IsEnable(FrameSettingsField.AtmosphericScattering))
            {
                cmd.SetGlobalInt(HDShaderIDs._AtmosphericScatteringType, (int)FogType.None);
                return;
            }

            switch (fogType.value)
            {
                case FogType.None:
                {
                    cmd.SetGlobalInt(HDShaderIDs._AtmosphericScatteringType, (int)FogType.None);
                    break;
                }
                case FogType.Linear:
                {
                    var fogSettings = VolumeManager.instance.stack.GetComponent<LinearFog>();
                    fogSettings.PushShaderParameters(hdCamera, cmd);
                    break;
                }
                case FogType.Exponential:
                {
                    var fogSettings = VolumeManager.instance.stack.GetComponent<ExponentialFog>();
                    fogSettings.PushShaderParameters(hdCamera, cmd);
                    break;
                }
                case FogType.Volumetric:
                {
                    if (hdCamera.frameSettings.IsEnable(FrameSettingsField.Volumetrics))
                    {
                        var fogSettings = VolumeManager.instance.stack.GetComponent<VolumetricFog>();
                        fogSettings.PushShaderParameters(hdCamera, cmd);
                    }
                    break;
                }
            }
        }
    }
}
