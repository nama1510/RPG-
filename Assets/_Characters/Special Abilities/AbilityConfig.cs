﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPG.Core;
namespace RPG.Characters
{

    public abstract class AbilityConfig : ScriptableObject
    {
        [Header("Special Ability General")]
        [SerializeField] float energyCost = 10f;

        [Header("Particle Effect")]
        [SerializeField] GameObject particleSystemPrefab = null;

        protected AbilityBehaviour behaviour;
        [SerializeField] AnimationClip abilityAnimation = null;
        [SerializeField] AudioClip[] abilityAudioClips = null;

        public abstract AbilityBehaviour GetBehaviourComponent(GameObject objectToAttachTo);
        public void AttachAbilityTo(GameObject objectToAttachTo)
        {
            var behaviourComponent = GetBehaviourComponent(objectToAttachTo);
            behaviourComponent.SetConfig(this);
            behaviour = behaviourComponent;
        }
        public void Use(HealthSystem hs) {
            behaviour.Use(hs);
        }
        public float GetEnergyCost() {
            return energyCost;
        }

        public GameObject GetParticlePrefab() {
            return particleSystemPrefab;
        }

        public AudioClip GetRandomAbilityAudioClip() {
            return abilityAudioClips[Random.Range(0, abilityAudioClips.Length)];
        }
        public AnimationClip GetAbilityAnimation() {
            if(abilityAnimation != null)
                abilityAnimation.events = new AnimationEvent[0]; //TODO: Remove this if we finally use animation events.
            return abilityAnimation;
        }
    }
}
