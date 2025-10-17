using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class AnimationOverrides : MonoBehaviour
{
    [SerializeField] private GameObject character = null;
    [SerializeField] private SO_AnimationType[] soAnimationTypeArray = null;

    private Dictionary<AnimationClip, SO_AnimationType> animationTypeDictionaryByAnimation;
    private Dictionary<string, SO_AnimationType> animationTypeDictionaryByCompositeAttributeKey;

    private void Start()
    {
        // Initialise dictionaries safely
        EnsureInitialized();
    }

     private void EnsureInitialized()
    {
        if (animationTypeDictionaryByAnimation != null && animationTypeDictionaryByCompositeAttributeKey != null)
            return;

        animationTypeDictionaryByAnimation = new Dictionary<AnimationClip, SO_AnimationType>();
        animationTypeDictionaryByCompositeAttributeKey = new Dictionary<string, SO_AnimationType>();

        if (soAnimationTypeArray == null || soAnimationTypeArray.Length == 0)
        {
            UnityEngine.Debug.LogWarning("AnimationOverrides: soAnimationTypeArray is empty. Populate in inspector or load SOs at runtime.");
            return;
        }

        foreach (SO_AnimationType item in soAnimationTypeArray)
        {
            if (item == null)
                continue;

            if (item.animationClip != null && !animationTypeDictionaryByAnimation.ContainsKey(item.animationClip))
                animationTypeDictionaryByAnimation.Add(item.animationClip, item);

            string key = item.characterPart.ToString() + item.partVariantColour.ToString() + item.partVariantType.ToString() + item.animationName.ToString();
            if (!animationTypeDictionaryByCompositeAttributeKey.ContainsKey(key))
                animationTypeDictionaryByCompositeAttributeKey.Add(key, item);
        }

        // Optional debug: list available composite keys
        foreach (var kv in animationTypeDictionaryByCompositeAttributeKey)
        {
            UnityEngine.Debug.Log($"AnimationOverrides: available key='{kv.Key}' -> SO='{kv.Value.name}' clip='{kv.Value.animationClip?.name}'");
        }
    }
    public void ApplyCharacterCustomisationParameters(List<CharacterAttribute> characterAttributesList)
    {
        // Ensure dictionaries are ready
        EnsureInitialized();

        if (character == null)
        {
            UnityEngine.Debug.LogWarning("AnimationOverrides: 'character' GameObject reference is null. Cannot apply overrides.");
            return;
        }

        // Loop through all character attributes and set the animation override controller for each
        foreach (CharacterAttribute characterAttribute in characterAttributesList)
        {
            Animator currentAnimator = null;
            List<KeyValuePair<AnimationClip, AnimationClip>> animsKeyValuePairList = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            string animatorSOAssetName = characterAttribute.characterPart.ToString();

            // Find animators in scene that match scriptable object animator type
            Animator[] animatorsArray = character.GetComponentsInChildren<Animator>();

            foreach (Animator animator in animatorsArray)
            {
                if (animator.name == animatorSOAssetName)
                {
                    currentAnimator = animator;
                    break;
                }
            }

            if (currentAnimator == null)
            {
                UnityEngine.Debug.LogWarning($"AnimationOverrides: animator '{animatorSOAssetName}' not found under character. Skipping attribute '{characterAttribute.characterPart}'.");
                continue;
            }

            if (currentAnimator.runtimeAnimatorController == null)
            {
                UnityEngine.Debug.LogWarning($"AnimationOverrides: animator '{currentAnimator.name}' has no runtime controller. Skipping.");
                continue;
            }

            // Get base current animations for animator
            AnimatorOverrideController aoc = new AnimatorOverrideController(currentAnimator.runtimeAnimatorController);
            List<AnimationClip> animationsList = new List<AnimationClip>(aoc.animationClips);

            foreach (AnimationClip animationClip in animationsList)
            {
                // find animation in dictionary
                SO_AnimationType so_AnimationType;
                bool foundAnimation = animationTypeDictionaryByAnimation.TryGetValue(animationClip, out so_AnimationType);

                if (foundAnimation)
                {
                    string key = characterAttribute.characterPart.ToString() + characterAttribute.partVariantColour.ToString() + characterAttribute.partVariantType.ToString() + so_AnimationType.animationName.ToString();

                    SO_AnimationType swapSO_AnimationType;
                    bool foundSwapAnimation = animationTypeDictionaryByCompositeAttributeKey.TryGetValue(key, out swapSO_AnimationType);

                    if (foundSwapAnimation)
                    {
                        AnimationClip swapAnimationClip = swapSO_AnimationType.animationClip;

                        animsKeyValuePairList.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, swapAnimationClip));
                    }
                    else
                    {
                        // DEBUG: log missing mapping
                        UnityEngine.Debug.Log($"AnimationOverrides: no swap found for key='{key}' (baseClip='{animationClip.name}', baseAnimationName='{so_AnimationType.animationName}', attributePart='{characterAttribute.characterPart}', variant='{characterAttribute.partVariantType}', colour='{characterAttribute.partVariantColour}')");
                    }
                }
            }

            // Apply animation updates to animation override controller and then update animator with the new controller
            aoc.ApplyOverrides(animsKeyValuePairList);
            currentAnimator.runtimeAnimatorController = aoc;
        }

        // s1.Stop();
        // UnityEngine.Debug.Log("Time to apply character customisation : " + s1.Elapsed + "   elapsed seconds");
    }

    public bool TryGetSOAnimationTypeByKey(string key, out SO_AnimationType soAnimationType)
    {
        EnsureInitialized();
        return animationTypeDictionaryByCompositeAttributeKey.TryGetValue(key, out soAnimationType);
    }
}
