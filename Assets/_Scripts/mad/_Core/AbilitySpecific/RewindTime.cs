using System;
using UnityEngine;

using WGRF.AI.Entities.Hostile.Generic;
using WGRF.BattleSystem;
using WGRF.Core;
using WGRF.Core.Managers;
using WGRF.Entities.Player;

namespace WGRF.Abilities
{
    public class RewindTime : Ability
    {
        //The Rewinder is created so we have access to runtime MonoBehaviour.
        Rewinder Rewinder { get; set; }
        Action externalCallback;

        public RewindTime(string name, int tier, Sprite abilitySprite, bool isUnlocked, Rewinder rewinder)
        {
            this.AbilityName = name;
            this.AbilityDescription = "UPDATE TEXT PER TIER";

            //Set from json
            this.AbilityTier = tier;

            this.MaxAbilityTier = 3;
            this.ActiveTime = 0;
            this.UsesPerLevel = 0;

            this.AbilitySprite = abilitySprite;

            //Set from json
            this.IsUnlocked = isUnlocked;

            //Ability specific
            this.Rewinder = rewinder;
        }

        /// <summary>
        /// Call to startup any basic ability behaviour
        /// <para>Sets the stats based on the abiltiy tier, caches the uses per level and sets canActivate to true.</para>
        /// </summary>
        public override void Start(Action callback)
        {
            externalCallback = callback;

            UpdateStatsPerTier();

            this.CanActivate = true;
        }

        /// <summary>
        /// Call to set IsUnlocked and CanActivate to true.
        /// </summary>
        protected override void EnableAbility()
        {
            IsUnlocked = true;
            CanActivate = true;
        }

        /// <summary>
        /// Call to Activate 
        /// </summary>
        public override bool TryActivate()
        {
            if (UsesPerLevel <= 0 || !IsUnlocked)
                return false;

            UsesPerLevel--;

            //Update remaining uses UI
            //ManagerHub.S.HUDHandler.UpdateRemainingUsesIcon(UsesPerLevel, cachedUses);

            CanActivate = false;

            timer = ActiveTime;

            //Start the entity positions storing
            Rewinder.RecordEntity(ManagerHub.S.PlayerController.Access<PlayerEntity>("pEntity"), ActiveTime);

            PlayAbilitySound();

            return true;
        }

        protected override void PlayAbilitySound()
        {
            //ManagerHub.S.GameSoundsHandler.PlayOneShot(GameAudioClip.PressPlay);
            //ManagerHub.S.GameSoundsHandler.PlayOneShot(GameAudioClip.PressRewind);
        }

        public override void UpdateAbilityTick()
        {
            //the timer will be used in the UI timer reference
            timer -= Time.deltaTime;
            //ManagerHub.S.HUDHandler.UpdateRemainingTimeIcon(timer, ActiveTime);

            if (timer <= 0f)
            {
                OnAbilityRecordingFinished();
                externalCallback();
            }
        }

        /// <summary>
        /// Call to invoke the AbilitySpecificActions() and the OnRewindStart() of the PlayerEntity.
        /// Then start the actual rewinding!!
        /// </summary>
        void OnAbilityRecordingFinished()
        {
            AbilitySpecificActions();

            Rewinder.CanRecord = false;

            //Initiate the actual rewind here!!!!

            //Set up the player behaviour for rewind
            ManagerHub.S.GameEventHandler.OnPlayerRewind(false);

            //Set notify other entities of the use of the rewind ability
            ManagerHub.S.GameEventHandler.OnAbilityUse(ThrowableSpeeds.RewindTimeSpeed, ThrowableSpeeds.RewindTimeRotation, true);

            //Move the player back in previous positions.
            Rewinder.RewindEntity(ManagerHub.S.PlayerController.Access<PlayerEntity>("pEntity"), OnAbilityFinished);
        }

        /// <summary>
        /// Call to make changes and actions based on the ability behaviour
        /// </summary>
        void AbilitySpecificActions()
        {
            /* foreach (EnemyEntity enemy in GameManager.S.AIEntityManager.GetEnemyEntityRefs())
            {
                if (enemy == null) continue;

                enemy.GetAgent().speed = 0;
                enemy.GetAgent().angularSpeed = 0;

                enemy.EnemyAnimation.SetAnimatorPlaybackSpeed(0f);

                enemy.DisableShootingBehaviour();
            } */
        }

        /// <summary>
        /// Call to reset the bullet speed and enemy behaviour, then call the OnRewindEnd() of the PlayerEntity.
        /// Sets canActivate to true.
        /// </summary>
        protected override void OnAbilityFinished()
        {
            externalCallback();

            timer = ActiveTime;

            BulletStatics.CurrentSpeed = BulletStatics.StartingSpeed;

            Rewinder.ResetRewinderBehaviour();

            /* foreach (EnemyEntity enemy in GameManager.S.AIEntityManager.GetEnemyEntityRefs())
            {
                if (enemy == null) continue;

                enemy.OnPlayerAbilityFinish();
            } */

            ManagerHub.S.GameEventHandler.OnPlayerRewind(true);
            ManagerHub.S.GameEventHandler.OnAbilityEnd();

            CanActivate = true;
        }

        /// <summary>
        /// Call to increase the AbilityTier by 1 level only if the current AbilityTier is smaller than MaxAbilityTier.
        /// </summary>
        public override void UpgradeAbility()
        {
            if (!IsUnlocked)
            {
                EnableAbility();
            }

            AbilityTier++;
            UpdateStatsPerTier();
        }

        /// <summary>
        /// Call after the construction of the Ability to set 
        /// the ActiveTime and UsesPerLevel based on the ability tier.
        /// </summary>
        public override void UpdateStatsPerTier()
        {
            switch (AbilityTier)
            {
                case 1:
                    ActiveTime = 2;
                    UsesPerLevel = 1;
                    break;

                case 2:
                    ActiveTime = 4;
                    UsesPerLevel = 2;
                    break;

                case 3:
                    ActiveTime = 6;
                    UsesPerLevel = 3;
                    break;
            }

            string pointToNext = AbilityTier < MaxAbilityTier ? 1.ToString() : "Maxed out!";
            string uses = UsesPerLevel > 0 ? UsesPerLevel.ToString() + " use(s) per floor." : "Not yet unlocked";
            string activeTime = ActiveTime > 0 ? ActiveTime.ToString() : "2";

            AbilityDescription = $"Points needed for next tier {pointToNext}\nRecord for {activeTime} seconds, after which you return back to position.\n{uses}";
            cachedUses = UsesPerLevel;
        }

        /// <summary>
        /// Call to Reset the ability uses per level.
        /// <para>Called on PlayerHub return.</para>
        /// </summary>
        public override void ResetAbilityUses()
        {
            UsesPerLevel = cachedUses;
            //ManagerHub.S.HUDHandler.UpdateRemainingUsesIcon(0, cachedUses);
        }

        public void AbilityInfoFullReset()
        {
            this.AbilityTier = 0;

            this.MaxAbilityTier = 3;
            this.ActiveTime = 0;
            this.UsesPerLevel = 0;

            this.IsUnlocked = false;
        }

        public override int GetCachedUses()
        {
            return cachedUses;
        }
    }
}