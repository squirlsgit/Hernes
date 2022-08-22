using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class ManageFadeOut : MonoSingleton<ManageFadeOut>
{
    public ModifierStack<Modifier> Intensity = new ModifierStack<Modifier>()
    {
        baseEffect = 1
    };
    public ModifierStack<Modifier> MinFade = new ModifierStack<Modifier>();
    public ModifierStack<Modifier> MaxFade = new ModifierStack<Modifier>();
    [SerializeField]
    protected Volume _volume;
    public VolumeProfile Volume
    {
        get
        {
            return _volume.profile;
        }
    }
    public GrayScale FadeOut
    {
        get
        {
            if (Volume.TryGet(out GrayScale v))
            {
                return v;
            }
            else
            {
                return null;
            }
        }
    }
    public Color FadeColor
    {
        get
        {
            return FadeOut.color.value;
        }
        set
        {
            FadeOut.color.value = value;
        }
    }
    private void OnEnable()
    {
        _volume = GetComponent<Volume>();
    }
    // Update is called once per frame
    void Update()
    {
        FadeOut.intensity.value = Intensity.Evaluate();
        FadeOut.minFade.value = MinFade.Evaluate();
        FadeOut.maxFade.value = MaxFade.Evaluate();
        Debug.Log($"intensity.value={FadeOut.intensity.value}, minFade.value={FadeOut.minFade.value}, maxFade.value={FadeOut.maxFade.value}");
    }
}
