using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ManageHereditusDistortion : MonoSingleton<ManageHereditusDistortion>
{
    public static void AddDistortionMod(Modifier m)
    {
        instance.Intensity += m;
    }
    [SerializeField]
    protected Volume _volume;
    public VolumeProfile Volume
    {
        get
        {
            return _volume.profile;
        }
    }
    public PaniniProjection Panini
    {
        get
        {
            if (Volume.TryGet(out PaniniProjection v))
            {
                return v;
            }
            else
            {
                return null;
            }
        }
    }
    public FilmGrain Grain
    {
        get
        {
            if (Volume.TryGet(out FilmGrain v))
            {
                return v;
            }
            else
            {
                return null;
            }
        }
    }
    public ColorCurves Saturation
    {
        get
        {
            if (Volume.TryGet(out ColorCurves v))
            {
                return v;
            }
            else
            {
                return null;
            }
        }
    }
    public ModifierStack<Modifier> Intensity = new ModifierStack<Modifier>();
    void OnEnable()
    {
        _volume = GetComponent<Volume>();
    }

    // Update is called once per frame
    void Update()
    {
        //var e = Intensity.effect;
        var i = Intensity.Evaluate();
        SetSaturationIntensity(i);
        SetGrainIntensity(i);
        SetPaniniIntensity(i);
        
    }

    public float desaturationLimit = 0.1f;
    public float saturationLimit = 0.5f;
    public void SetSaturationIntensity(float intensity)
    {
        //Saturation.active = intensity > 0;
        var key = new Keyframe(0.5f, Mathf.Lerp(saturationLimit, Mathf.Min(desaturationLimit, saturationLimit), intensity));
        if (Saturation.hueVsSat.value.length > 0)
        {
            Saturation.hueVsSat.value.MoveKey(0, key);
        } else
        {
            Saturation.hueVsSat.value.AddKey(key.time, key.value);
        }
    }

    public void SetGrainIntensity(float intensity)
    {
        Grain.active = intensity > 0;
        Grain.intensity.value = intensity;
    }

    public void SetPaniniIntensity(float intensity)
    {
        Panini.active = intensity > 0 && saturationLimit != 0.5f;
        Panini.distance.value = intensity;
    }
}
