using UnityEngine;

[DisallowMultipleComponent]
public class PrototypeEffectsTuning : MonoBehaviour
{
    [Header("Center Glow")]
    public float centerGlowWidth = 14f;

    [Header("Wall Pulse")]
    public float leftShadowPulseSpeed = 1.22f;
    public float rightShadowPulseSpeed = 1.38f;
    public float leftPulseSpeed = 1.68f;
    public float rightPulseSpeed = 1.86f;
    public float wallPulseAlpha = 0.16f;
    public float wallShadowPulseAlpha = 0.06f;

    [Header("Jet Trails")]
    public float sideTrailTime = 0.22f;
    public float sideTrailGlowTime = 0.28f;
    public float centerTrailTime = 0.28f;
    public float centerTrailGlowTime = 0.34f;

    [Header("Round Trail Particles")]
    public float particleLifetime = 0.25f;
    public float particleStartSize = 0.16f;
    public int particleMaxCount = 14;
    public float particleRateOverTime = 32f;
    public float particleSpawnWidth = 0.96f;
}
