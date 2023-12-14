using UnityEngine;

using WGRF.Core;
using WGRF.Interactions;

namespace WGRF.BattleSystem
{
    /// <summary>
    /// The layers every bullet gameObject can exist on.
    /// </summary>
    public enum ProjectileLayers
    {
        PlayerProjectile = 12,
        EnemyProjectile = 13,
    }

    public class Bullet : MonoBehaviour
    {
        [Header("Set in inspector")]
        [SerializeField] string bloodImpactFX_Path;

        float bulletSpeed;

        private void Update()
        {
            //Move the bullet to its local forward. 
            transform.position += transform.forward * bulletSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            IInteractable interaction = other.GetComponent<IInteractable>();

            if (interaction != null)
            {
                interaction.AttackInteraction();

                //Spawn the blood impact FX when the bullet hits an entity
                GameObject spawnedFX = UnityAssets.Load(bloodImpactFX_Path, false);
                spawnedFX.transform.position = other.transform.position;
                spawnedFX.transform.rotation = Quaternion.identity;
                spawnedFX.transform.rotation = transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
            }
        }

        /// <summary>
        /// Call to set THIS bullet instances' bullet type.
        /// <para>Bullet type of:</para>
        /// <para>Enemy: Collides with the player and not with AIEntities.</para>
        /// <para>Player: Collides with the AIEntities and not the player.</para>
        /// </summary>
        public void SetBulletType(BulletType type)
        { gameObject.layer = (int)type; }

        /// <summary>
        /// Resets bullet variables when the bullet gets enabled.
        /// </summary>
        private void OnEnable()
        {
            bulletSpeed = BulletStatics.CurrentSpeed;
            Destroy(gameObject, 2f);
        }
    }
}