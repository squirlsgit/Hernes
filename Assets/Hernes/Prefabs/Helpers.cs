using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.AI;
using UnityEngine.Events;
using Valve.VR.InteractionSystem;

public static class Helpers
{
    public static List<float> GetPosition(this Transform t)
    {
        return new List<float>() { t.position.x, t.position.y, t.position.z };
    }
    public static List<float> GetRotation(this Transform t)
    {
        var euler = t.rotation.eulerAngles;
        return new List<float>() { euler.x, euler.y, euler.z };
    }
    public static Vector3 GetVector(this List<float> l)
    {
        if (l == null)
        {
            return Vector3.zero;
        }
        return new Vector3(l[0], l[1], l[2]);
    }
    public static Quaternion GetEuler(this List<float> l)
    {
        return Quaternion.Euler(l[0], l[1], l[2]);
    }
    public static T Random<T>(this List<T> l)
    {
        return l[Mathf.RoundToInt(UnityEngine.Random.value * (l.Count - 1))];
    }

    public static GameObject GetObject(this Collider c)
    {
        if (c.attachedRigidbody != null)
        {
            return c.attachedRigidbody.gameObject;
        } else
        {
            return c.gameObject;
        }
    }
    public static List<LineRenderer> rays = new List<LineRenderer>();
    public static Modifier CopyModifier(this Modifier mod)
    {
        var m = new Modifier()
        {
            effect = mod.effect,
            effectCurve = mod.effectCurve,
            duration = mod.duration,
            startTime = 0f,
            _animationDuration = mod._animationDuration,
            _relativeAnimationStartTime = mod._relativeAnimationStartTime,
        };
        return m;
    }
    public static BrightnessModifier CopyModifier(this BrightnessModifier mod)
    {
        var m = new BrightnessModifier()
        {
            controlCleanup = mod.controlCleanup,
            effect = mod.effect,
            effectCurve = mod.effectCurve,
            duration = mod.duration,
            startTime = 0f,
            _animationDuration = mod._animationDuration,
            _relativeAnimationStartTime = mod._relativeAnimationStartTime,
        };
        return m;
    }
    public static Transform GetChildWithTag(this Transform t, string tag)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            if (child.CompareTag(tag))
            {
                return child;
            }
            else
            {
                var tagged = child.GetChildWithTag(tag);
                if (tagged != null)
                {
                    return tagged;
                }
            }
        }
        return null;
    }
    public static List<Transform> GetChildrenWithTag(this Transform t, string tag)
    {
        List<Transform> transforms = new List<Transform>();
        for (int i = 0; i < t.childCount; i++)
        {
            var child = t.GetChild(i);
            if (child.CompareTag(tag))
            {
                transforms.Add(child);
            }
            else
            {
                var taggedChildren = child.GetChildrenWithTag(tag);
                if (taggedChildren.Count > 0)
                {
                    transforms = transforms.Concat(taggedChildren).ToList();
                }
            }
        }
        return transforms;
    }
    public static Dictionary<GameObject, int> SetLayerRecursively(this GameObject obj, int layer, int mask = -1)
    {
        Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();
        //Debug.Log($"SetLayerRecursively layer={LayerMask.LayerToName(layer)} !{LayerMask.LayerToName(obj.layer)} ~{LayerMask.LayerToName(mask)} mask={LayerMask.LayerToName(mask)} match={MatchLayerToMask(obj.layer, mask)}");
        if (MatchLayerToMask(obj.layer, mask))
        {
            obj.layer = layer;
            layers[obj] = layer;
        }
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            var dict = child.gameObject.SetLayerRecursively(layer, mask);
            foreach (var gOLayer in dict)
            {
                layers.Add(gOLayer.Key, gOLayer.Value);
            }
        }
        return layers;
    }
    /// <summary>
    /// Vector3 extension, returns inversion of vector v.
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    public static Vector3 Invert(this Vector3 v)
    {
        return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
    }
    public static bool NavPosition(this Vector3 v, out Vector3 newPos, float sampleDistance = 100f, int area = NavMesh.AllAreas)
    {
        if (NavMesh.SamplePosition(v, out NavMeshHit hit, sampleDistance, area))
        {
            newPos = hit.position;
            return true;
        }
        newPos = v;
        return false;
    }
    public static bool CanSee(this Transform t, Transform other, float FoV, float maxDistance = Mathf.Infinity, int fovLayer = Physics.AllLayers)
    {
        Vector3 dir = t.position - other.position;
        float distance = Mathf.Min(maxDistance, dir.magnitude);
        if (distance < dir.magnitude)
        {
            return false;
        }
        bool inSight = Quaternion.Angle(Quaternion.Euler(t.forward), Quaternion.Euler(other.position - t.position)) < FoV;
        if (!inSight)
        {
            return false;
        }
        if (Physics.Raycast(
            t.position,
            dir,
            out RaycastHit hitInfo,
            distance,
            fovLayer,
            QueryTriggerInteraction.Ignore
        ))
        {
            if (hitInfo.collider.attachedRigidbody != null)
            {
                return hitInfo.collider.attachedRigidbody.transform == other;
            }
            else
            {
                return hitInfo.collider.transform == other;
            }
        }
        return false;
    }
    public static bool CanSee(this Transform t, Vector3 other, float FoV, float maxDistance = Mathf.Infinity, int fovLayer = Physics.AllLayers)
    {
        Vector3 dir = t.position - other;
        float distance = Mathf.Min(maxDistance, dir.magnitude);
        if (distance < dir.magnitude)
        {
            return false;
        }
        bool inSight = Quaternion.Angle(Quaternion.Euler(t.forward), Quaternion.Euler(other - t.position)) <= FoV;
        if (!inSight)
        {
            return false;
        }
        if (Physics.Raycast(
            t.position,
            dir,
            out RaycastHit hitInfo,
            distance,
            fovLayer,
            QueryTriggerInteraction.Ignore
        ))
        {
            return false;
        }
        return true;
    }
    public static void TestRayCast(Vector3 origin, Vector3 destination, MonoBehaviour parent)
    {
        rays.Clear();
        GameObject gO = new GameObject("lr");
        gO.transform.parent = parent.transform;
        LineRenderer lr = gO.AddComponent<LineRenderer>();
        lr.SetPositions(new Vector3[2] { origin, destination });
        lr.startWidth = 0.1f;
        lr.startColor = Color.red;
    }
    public static void CopyToDestinationTransform(this Transform self, Transform dest)
    {
        //if (scaleToParent)
        //{
        //    var localPosition = self.localPosition;
        //    var localScale = self.localScale;
        //    var localRotation = self.localRotation;
        //}

        //var parentScale = self.parent.lossyScale;
        //var localScale = self.localScale;
        //var lossyScale = self.lossyScale;
        self.parent = dest;
        //var lossyScaleAfter = self.lossyScale;
        //self.localScale = Vector3.Scale(self.localScale, Vector3.Scale(lossyScale, Invert(lossyScaleAfter)));

        //if (scaleToParent)
        //{
        //    self.localPosition = localPosition;
        //    self.localRotation = localRotation;
        //    self.localScale = localScale;
        //}

    }
    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
    public static T AddComponent<T>(this GameObject go, PropertyContainer pc) where T : Component
    {
        return go.AddComponent<T>().CopyFromContainer<T>(pc) as T;
    }
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }
    public struct PropertyContainer
    {
        public Dictionary<PropertyInfo, object> pinfos;
        public Dictionary<FieldInfo, object> finfos;
    }
    public static T CopyFromContainer<T>(this Component comp, PropertyContainer pc) where T : Component
    {
        Type type = comp.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        foreach (var kvp in pc.pinfos)
        {
            var pinfo = kvp.Key;
            var value = kvp.Value;
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, value, null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var kvp in pc.finfos)
        {
            var finfo = kvp.Key;
            var value = kvp.Value;
            finfo.SetValue(comp, value);
        }
        return comp as T;
    }

    public static PropertyContainer CopyToContainer(this Component comp)
    {
        var pc = new PropertyContainer()
        {
            pinfos = new Dictionary<PropertyInfo, object>(),
            finfos = new Dictionary<FieldInfo, object>(),
        };

        Type type = comp.GetType();
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pc.pinfos.Add(pinfo, pinfo.GetValue(comp, null));
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            pc.finfos.Add(finfo, finfo.GetValue(comp));
        }
        return pc;
    }
    public static bool MatchLayerToMask(int layer, string mask_name)
    {
        int mask = LayerMask.GetMask(mask_name);
        return MatchLayerToMask(layer, mask);
    }
    public static bool MatchLayerToMask(int layer, int mask)
    {
        return (mask == (mask | (1 << layer)));
    }
    //public static List<T> GetNearbyComponents<T>(this GameObject gameObject, Vector3 center, float radius, int mask)
    //{
    //    Collider[] hitColliders = Physics.OverlapSphere(center, radius, mask);
    //    Debug.Log($"GameObject [GetNearbyComponents] GameObject={gameObject.name} Collider count={hitColliders.Length}");
    //    List<T> detected = new List<T>();
    //    foreach (Collider collision in hitColliders)
    //    {
    //        dynamic go = null;
    //        if (collision.attachedRigidbody != null)
    //        {
    //            go = collision.attachedRigidbody.gameObject.GetComponent<T>();
    //        }
    //        else
    //        {
    //            go = collision.gameObject.GetComponent<T>();
    //        }
    //        if (go != null &&
    //            collision.gameObject != gameObject &&
    //            !detected.Contains(go))
    //        {
    //            detected.Add((T)go);
    //        }
    //    }

    //    return detected;
    //}
    public static RaycastHit? FindGround(this Transform transform, Vector3 up, float height = 2f, float depth = 5f)
    {
        RaycastHit hit;
        var rayOrigin = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);

        if (Physics.Raycast(rayOrigin, up * -1, out hit, depth, LayerMask.GetMask("Surface")))
        {
            Debug.DrawRay(rayOrigin, up * -1 * hit.distance, Color.yellow);
        }
        else
        {
            return null;
        }
        return hit;
    }
    public static RaycastHit? FindGround(this Vector3 pos, Vector3 up, float height = 2f, float depth = 5f)
    {
        RaycastHit hit;
        var rayOrigin = new Vector3(pos.x, pos.y + height, pos.z);

        if (Physics.Raycast(rayOrigin, up * -1, out hit, depth, LayerMask.GetMask("Surface")))
        {
            Debug.DrawRay(rayOrigin, up * -1 * hit.distance, Color.yellow);
        }
        else
        {
            return null;
        }
        return hit;
    }
    public static GameObject GetParentObject(this Collider collider)
    {
        if (collider.attachedRigidbody)
        {
            return collider.attachedRigidbody.gameObject;
        }
        return collider.gameObject;
    }

    /// <summary>
    /// Sets position, and children position so they will not move in world space.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public static void SetUninheritedPosition(this Transform t, Vector3 position)
    {
        Dictionary<Transform, Vector3> childPositions = new Dictionary<Transform, Vector3>();
        for (var i = 0; i < t.childCount; i++)
        {
            childPositions.Add(t, t.GetChild(i).position);
        }
        t.position = position;
        foreach (var child in childPositions)
        {
            child.Key.position = child.Value;
        }
    }
}
public interface IHittable
{
    void OnHit(string type, GameObject agent, GameObject weapon = null);
}
public interface IShooter
{
    GameObject Agent { get; }
    string Type { get; }

    void Shoot();


}

public interface ISensor
{
    GameObject HearsSource { get; }
    void Sensor(GameObject sensed);
}

public interface IAudioStore
{
    public AudioSource GetAudioSource(string queriedAction);
}

[System.Serializable]
public class BrightnessModifier : Modifier
{
    public bool controlCleanup;
    public override bool IsExpired
    {
        get
        {
            return controlCleanup && duration < AliveFor;
        }
    }
}


[System.Serializable]
public class VectorModifier
{
    public float duration = 0;
    public Vector3 effect;
    public float startTime = 0;
    public virtual bool IsExpired
    {
        get
        {
            return duration > 0 && duration < AliveFor;
        }
    }
    public static VectorModifier Default()
    {
        return (new VectorModifier()
        {
            duration = 0,
            effect = Vector3.zero,
        });
    }
    public float AliveFor
    {
        get
        {
            return Time.time - startTime;
        }
    }
    public Vector3 Evaluate()
    {
        if (startTime == 0)
        {
            startTime = Time.time;
        }
        return effect;
    }

}
[System.Serializable]
public class VectorStack<M> where M : VectorModifier
{
    public float minSpeed;
    public float maxSpeed;
    public List<M> modifiers = new List<M>();
    public Vector3 baseEffect = Vector3.zero;
    [Tooltip("Most recent evaluation.")]
    public Vector3 effect;
    /// <summary>
    /// Evaluates active mods, and removes expired mods.
    /// </summary>
    /// <returns></returns>
    public virtual Vector3 Evaluate()
    {
        Vector3 v = baseEffect;
        List<M> expired = new List<M>();
        foreach (var mod in modifiers)
        {
            if (mod.IsExpired)
            {
                expired.Add(mod);
            }
            else
            {
                v += mod.Evaluate();
            }
        }
        foreach (var expiredMod in expired)
        {
            modifiers.Remove(expiredMod);
        }
        effect = v;
        return v;
    }
    public bool Contains(M m)
    {
        return modifiers.Contains(m);
    }
    public static VectorStack<M> operator +(VectorStack<M> modStack, M mod)
    {
        if (!modStack.modifiers.Contains(mod))
        {
            modStack.modifiers.Add(mod);
        }
        return modStack;
    }
    public static VectorStack<M> operator -(VectorStack<M> modStack, M mod)
    {
        modStack.modifiers.Remove(mod);
        return modStack;
    }
}

[Serializable]
public class Modifier
{
    public static AnimationCurve defaultCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public static Modifier Default()
    {
        return (new Modifier()
        {
            duration = 0,
            effect = 0,
        });
    }
    public float duration = 0;
    public float effect;
    public AnimationCurve effectCurve = null;
    public AnimationCurve EffectCurve
    {
        get
        {
            if (effectCurve != null)
            {
                return effectCurve;
            }
            return defaultCurve;
        }
    }
    public float _animationDuration = 0;
    public float AnimationDuration
    {
        get
        {
            if (_animationDuration > 0)
            {
                return _animationDuration;
            }
            else
            {
                return duration;
            }
        }
        set
        {
            _animationDuration = value;
        }
    }

    public float startTime = 0f;
    /// <summary>
    /// Relative to start time
    /// </summary>
    public float _relativeAnimationStartTime = 0f;
    public float AnimationStartTime
    {
        get
        {
            return _relativeAnimationStartTime + startTime;
        }
        set
        {
            _relativeAnimationStartTime = value - startTime;
        }
    }
    public float AnimationTime
    {
        get
        {
            return Time.time - AnimationStartTime;
        }
    }
    public float AliveFor
    {
        get
        {
            return Time.time - startTime;
        }
    }
    public bool HasAnimation
    {
        get
        {
            return AnimationDuration > 0 && (effectCurve != null || _animationDuration > 0);
        }
    }
    public virtual bool IsExpired
    {
        get
        {
            return duration > 0 && duration < AliveFor;
        }
    }
    protected float EvaluateAnimation()
    {
        if (AnimationDuration > 0)
        {
            if (AnimationDuration < AnimationTime)
            {
                return EffectCurve.Evaluate(1) * effect;
            }
            else
            {
                return EffectCurve.Evaluate(AnimationTime / AnimationDuration) * effect;
            }
        }
        else
        {
            return EffectCurve.Evaluate(AnimationTime) * effect;
        }
    }
    public float Evaluate()
    {
        if (startTime == 0)
        {
            startTime = Time.time;
        }
        if (HasAnimation)
        {
            return EvaluateAnimation();
        }
        return effect;
    }
    /// <summary>
    /// Replaces animation, and starts.
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="animationDuration"></param>
    public void StartAnimation(AnimationCurve curve, float animationDuration = 1)
    {
        AnimationStartTime = Time.time;
        effectCurve = curve;
        AnimationDuration = animationDuration;
    }
    /// <summary>
    /// Sets effect to value, and restarts current animation
    /// </summary>
    /// <param name="val"></param>
    public void StartAnimation(float val)
    {
        effect = val;
        StartAnimation(effectCurve, _animationDuration);
    }
}

[System.Serializable]
public class ModifierStack<M> where M : Modifier
{
    public List<M> modifiers = new List<M>();
    public float baseEffect = 0;
    [Tooltip("Most recent evaluation.")]
    public float effect;
    /// <summary>
    /// Invoked when adding modifier
    /// </summary>
    public UnityEvent<M> OnAddModifier = new UnityEvent<M>();
    /// <summary>
    /// Invoked when removing modifier
    /// </summary>
    public UnityEvent<M> OnRemoveModifier = new UnityEvent<M>();
    /// <summary>
    /// Invoked only when removed during Evaluation
    /// </summary>
    public UnityEvent<M> OnExpireModifier = new UnityEvent<M>();
    /// <summary>
    /// Evaluates active mods, and removes expired mods.
    /// </summary>
    /// <returns></returns>
    public virtual float Evaluate()
    {
        float v = baseEffect;
        List<M> expired = new List<M>();
        foreach (var mod in modifiers)
        {
            if (mod.IsExpired)
            {
                expired.Add(mod);
            }
            else
            {
                v += mod.Evaluate();
            }
        }
        foreach (var expiredMod in expired)
        {
            OnExpireModifier.Invoke(expiredMod);
            OnRemoveModifier.Invoke(expiredMod);
            modifiers.Remove(expiredMod);
        }
        effect = v;
        return v;
    }
    public bool Contains(M m)
    {
        return modifiers.Contains(m);
    }
    public static ModifierStack<M> operator +(ModifierStack<M> modStack, M mod)
    {
        if (!modStack.modifiers.Contains(mod))
        {
            modStack.modifiers.Add(mod);
            modStack.OnAddModifier.Invoke(mod);
        }
        return modStack;
    }
    public static ModifierStack<M> operator -(ModifierStack<M> modStack, M mod)
    {
        modStack.modifiers.Remove(mod);
        modStack.OnRemoveModifier.Invoke(mod);
        return modStack;
    }
    public static float operator +(ModifierStack<M> modStack, float effect)
    {
        modStack.baseEffect += effect;
        return modStack.baseEffect;
    }
    public static float operator -(ModifierStack<M> modStack, float effect)
    {
        modStack.baseEffect -= effect;
        return modStack.baseEffect;
    }
}




[System.Serializable]
public class NodeStateTier
{

    public float start;
    public float end;

    public Color light_color;
    public Color mat_color;
    public Color highlight_color;
    public float light_intensity;
    public float highlight_intensity;
    public float[] custom_properties;

}
[System.Serializable]
public class CheckpointState
{
    public float start;
    public float end;


    public Color mat_color;
    public Color highlight_color;
    public float highlight_intensity;

    public float[] custom_properties;
}
public enum CreatureState
{
    Default,
    Falling,
    Paused,
}

[Serializable]
public class Page
{
    public bool active = false;
    public string text_asset_path = "";

    public Texture text;

}
public enum AfterShatterAction
{

    Ignore,
    Track,
    Clean

}

public enum RoutineAction
{
    Remove,
    Reset,
    Reverse,
    Complete,
    Nothing,

}
public struct EssentialTransform
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public float distance;
}
public interface IShatter
{
    /// <summary>
    /// Deactivates 'complete' rigidbody, enables shatter prefab, and applies explosive force.
    /// </summary>
    /// <param name="force">explosive force</param>
    /// <param name="origin">the origin of the 'explosion'</param>
    void shatter(float force, Vector3 origin);
    /// <summary>
    /// For all listed components, reassemble
    /// </summary>
    /// <param name="seconds">transition time</param>
    void reassemble(float seconds);

}


public enum RuntimeAnimationEvent
{
    OnStart,
    NewClip,

    /// <summary>
    /// Used if we want to implement a custom event system for the 'lightweight' animator. This would be borderline overengineering though, so going to stop at writing a note for this.
    /// </summary>
    OnCustom,

    OnEnd,
    OnComplete,
    OnLoop,
    OnReverse
}


/// <summary>
/// Is delegated the responsibility of animating a frame.
/// </summary>
public interface IAnimationControl
{

    /// <summary>
    /// Animates a frame given an event
    /// </summary>
    /// <param name="ev">Event Description</param>
    /// <param name="args">Arguments provided for the task</param>
    void animateEvent(RuntimeAnimationEvent ev, params dynamic[] args);

}

[Flags]
public enum AthameFlags
{
    FadeOnCollision = 1 << 0,
    AttachOnGrip = 1 << 1,
    FadeAfterThrow = 1 << 2,
}
[Flags]
public enum AthamePhysicsFlags
{
    ApplyDirectionalDrag = 1 << 0,
    ApplyDirecitonalTorque = 1 << 2,
}

public interface IFlagStore<F> where F : Enum
{
    F Flags
    {
        get;
    }
    bool HasFlag(F flag);
}
[Flags]
public enum AffixFlags
{
    ThrowOnRelease = 1 << 0,
    ChildObject = 1 << 8,
    UseJoint = 1 << 1,
    FixedPosition = 1 << 2,
    ReleaseBySpeed = 1 << 3,
    ReleaseByTrigger = 1 << 4,
    UseRelativeVelocity = 1 << 5,
    ReleaseByCollision = 1 << 6,
    ApplyForceOnCollision = 1 << 7,
    CaptureOnCollision = 1 << 8,
}

[Flags]
public enum RitualFlags
{
    AllowInvertedSpell = 1 << 0,
    ConsumeFuel = 1 << 1,
    ListenToNodeConnections = 1 << 2,
    DestroyOnSpellFinished = 1 << 3,
    DestroyOnEmpty = 1 << 4,
    /// <summary>
    /// idealy implemented from ritualSurface
    /// </summary>
    PreventCircleOverlapFailsafe = 1 << 5,
    ResetTimerOnNodeRegistered = 1 << 6,
    RevealRootsOnActive = 1 << 7,
    HideRootsOnDeactivated = 1 << 8,
    AffixToSurfaceOnStart = 1 << 9,
    TrackSurfaceCollisions = 1 << 10,
    DestroyIsRecycle = 1 << 11,
}
[Flags]
public enum SpellFlags
{
    // If true, registering a new node will cast a spell. If false the spell cast function will simply be invoked in the event.
    CastOnMatch = 1 << 0,
    CastOnStart = 1 << 1,
    CastOnInit = 1 << 2,
    /// Describes whether to cast when circle was inactive until fuel reaches a breakpoint. At breakpoint we should attempt to cast the spell.
    CastOnIntervalInteraction = 1 << 3,
    ExposeCastOnInit = 1 << 4,
    ExposeCastOnRegistered = 1 << 5,
    DrawOnStart = 1 << 6,
    DrawOnInit = 1 << 7,
    DrawOnActivated = 1 << 8,
    CastOnDeactivated = 1 << 9,
}

[System.Serializable]
public struct AffixSettings
{
    [Tooltip("The speed at which we override any other settings and break the connection between the rigidbodies")]
    public float thresholdSpeed;
    public bool IsKinematic
    {
        get
        {
            if (HasAffixFlag(AffixFlags.UseJoint))
            {
                return false;
            }
            if (HasAffixFlag(AffixFlags.FixedPosition))
            {
                return true;
            }
            if (HasAffixFlag(AffixFlags.ChildObject))
            {
                return true;
            }
            return false;
        }
    }
    [Tooltip("Used when not using joints to connect the two rigidbodies programmatically")]
    public Vector3 positionOffset;
    [EnumFlags]
    [SerializeField]
    public AffixFlags affixFlags;

    public bool HasAffixFlag(AffixFlags flag)
    {
        return (affixFlags & flag) == flag;
    }

    public float breakForce;
    public float breakTorque;
}

public interface IAffixStore
{
    AffixSettings AffixConfig
    {
        get;
    }
}
public interface ISpellComponent
{
    void InitSpellComponent(float ritual_strength, float duration);
}
public interface IMono
{
    GameObject Context { get; }
}
public delegate void delayF(params dynamic[] arguments);


public enum ZoneState
{
    Novice,
    Intermediate,
    Lovecraft
}



[Serializable]
public class VRInputEvent : UnityEvent<InputSource, whichHand, List<dynamic>> { }
public enum InputSource
{
    Trigger,
    TouchPad,
    Grip
}
public enum InputType
{
    Trigger,
    ButtonDown,
    ButtonUp,
    Change,
    Update
}
public enum buttonState
{
    Up,
    Down
}
public enum whichHand
{
    Left,
    Right
}
public enum SpellLifeCycle
{
    Cold,
    Init,
    Start,
    Manifest,
    End,
}
public interface ISpellCaster
{
    void cast(Collider collider);
    void cast(Collision collision);
    void cast(Vector3 pos);
    void cast(GameObject focus);
    //void cast(Vector3 pos);
}


public delegate void cast(GameObject origin, GameObject target);

public class InputData
{
    public InputSource InputSource { get; set; }
    public InputType InputType { get; set; }
    public whichHand WhichHand { get; set; }
}

public interface IMarker
{
    Rigidbody rb { get; }
    Collision collision { get; set; }
    Collider collided { get; set; }

    dynamic device { get; }
    void OnDraw(IMarkable markable);
}
public interface IMarkable
{
    /// <summary>
    /// Markable responsible for 'drawing' on surface.
    /// </summary>
    /// <param name="marker"></param>
    void tryDraw(IMarker marker);
}
public interface IActionable<T> where T : MonoBehaviour
{
    void OnActionDestroy(T actionObject);
    void clearTrackedActions();
}
public interface ICatalogItem
{
}
public interface ILighthouseZone
{
    void addFuel(float amount);
}
public interface IEngine
{
    void addFuel(float add);
    void emptyFuel(bool inertify);

}
public interface ICollisionNest
{
    void OnNestedCollisionEnter(MonoBehaviour self, Collision collision);
    void OnNestedCollisionExit(MonoBehaviour self, Collision collision);

    void OnNestedCollisionStay(MonoBehaviour self, Collision collision);
}
public interface ITriggerNest
{
    void OnNestedTriggerEnter(MonoBehaviour self, Collider collider);
    void OnNestedTriggerExit(MonoBehaviour self, Collider collider);
    void OnNestedTriggerStay(MonoBehaviour self, Collider collider);
}
public interface IParticleCollisionNest
{
    void OnNestedParticleCollision(MonoBehaviour self, GameObject other);
    void OnNestedParticleTrigger(MonoBehaviour self);
}
public delegate void onTraceEvent(TraceEvents ev, dynamic arg);
public enum TraceEvents
{

    // dynamic object is function delegate initializespell or null
    OnSpellMatch,

    // dynamic object is instantiated spell game object
    OnCastSpell


}

public interface IStateControl
{

    int track_layer { get; set; }
    float time_of_day { get; set; }
    List<int> registry { get; set; }
}