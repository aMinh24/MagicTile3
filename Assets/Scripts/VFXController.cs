using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public ParticleSystem goodVFX;
    public ParticleSystem PerfectVFX;
    public ParticleSystem xVFX;
    public ParticleSystem[] unitsVFX; // Array of particle systems for different units
    public ParticleSystem[] tensVFX;

    [Button("Play VFX with Multiplier")]
    public void PlayVFXWithMultiplier(string vfxType, int multiplier)
    {
        // Play base VFX (e.g., PerfectVFX, goodVFX)
        switch (vfxType.ToLower())
        {
            case "perfect":
                if (PerfectVFX != null) PerfectVFX.Play();
                break;
            case "good":
                if (goodVFX != null) goodVFX.Play();
                break;
            // Add more cases as needed
            default:
                Debug.LogWarning($"Unknown VFX type: {vfxType}");
                break;
        }

        // Play number VFX based on multiplier
        if (multiplier <= 1)
        {
            return;
        }
        // Play 'x' VFX
        if (xVFX != null) xVFX.Play();


        if (multiplier >= 0 && multiplier <= 9) // Single digit number (0-9)
        {
            if (tensVFX != null && multiplier < tensVFX.Length && tensVFX[multiplier] != null)
            {
                tensVFX[multiplier].Play();
            }
            else
            {
                Debug.LogWarning($"Required VFX tensVFX[{multiplier}] is not set or out of bounds.");
            }
        }
        else if (multiplier >= 10) // Two (or more) digit number. This logic primarily supports up to 99.
        {
            int tensDigit = multiplier / 10;
            int unitsDigit = multiplier % 10;

            // Play tens digit VFX
            if (tensVFX != null && tensDigit < tensVFX.Length && tensVFX[tensDigit] != null)
            {
                tensVFX[tensDigit].Play();
            }
            else
            {
                Debug.LogWarning($"Required VFX tensVFX[{tensDigit}] for multiplier {multiplier} is not set or out of bounds.");
            }

            // Play units digit VFX
            if (unitsVFX != null && unitsDigit < unitsVFX.Length && unitsVFX[unitsDigit] != null)
            {
                unitsVFX[unitsDigit].Play();
            }
            else
            {
                Debug.LogWarning($"Required VFX unitsVFX[{unitsDigit}] for multiplier {multiplier} is not set or out of bounds.");
            }
        }
    }
}
