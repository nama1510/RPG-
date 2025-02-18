using System;
using UnityEngine;
using UnityEngine.AI;

using RPG.CameraUI;
namespace RPG.Characters
{
    public class Character : MonoBehaviour
    {

        Animator animator;
        [Header("Animator Settings")]
        [SerializeField] RuntimeAnimatorController animatorController;
        [SerializeField] AnimatorOverrideController overrideController;
        [SerializeField] Avatar characterAvatar;

        [Header("Collider Settings")]
        [SerializeField] float colliderRadius = .2f;
        [SerializeField] float colliderHeight = 1.8f;
        [SerializeField] Vector3 colliderCenter = new Vector3(0f,0.9f,0f);

        Rigidbody myRigidbody;
        [Header("Rigidbody Settings")]
        [SerializeField] float mass = 1;
        [SerializeField] float drag = 0;
        [SerializeField] float angularDrag = 0.05f;

        [Header("AudioSource Settings")]
        [SerializeField] [Range(0f, 1f)] float spatialBlend = .7f;

        NavMeshAgent agent;
        [Header("NavMeshAgent Settings")]
        [SerializeField] float steeringSpeed = 1;
        [SerializeField] float angularSpeed = 120;
        [SerializeField] float acceleration = 8;
        [SerializeField] float stoppingDistance = 1.3f;
        [SerializeField] float avoidanceRadius = .1f;
        [SerializeField] float avoidanceHeight = 2f;

        [Header("Movement Properties")]
        [SerializeField] float movingTurnSpeed = 360;
        [SerializeField] float stationaryTurnSpeed = 180;
        [SerializeField] float moveThreshold = 1f;
        [SerializeField] float moveSpeedMultiplier = 1.2f;
        [SerializeField] float animatiorSpeedMultiplier = 1f;

        //PlayerControl player;
        CharacterStats characterStats;

        Vector3 currentDestination;

        float turnAmount;
        float forwardAmount;

        bool isAlive = true;

        public void Awake()
        {
            AddRequiredComponents();
        }

        private void AddRequiredComponents()
        {
            animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = animatorController;
            animator.avatar = characterAvatar;

            CapsuleCollider characterCollider = gameObject.AddComponent<CapsuleCollider>();
            characterCollider.radius = colliderRadius;
            characterCollider.height = colliderHeight;
            characterCollider.center = colliderCenter;

            myRigidbody = gameObject.AddComponent<Rigidbody>();
            myRigidbody.mass = mass;
            myRigidbody.drag = drag;
            myRigidbody.angularDrag = angularDrag;
            myRigidbody.useGravity = true;

            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.pitch = 1;
            audioSource.spatialBlend = spatialBlend;

            agent = gameObject.AddComponent<NavMeshAgent>();
            agent.speed = steeringSpeed;
            agent.angularSpeed = angularSpeed;
            agent.acceleration = acceleration;
            agent.stoppingDistance = stoppingDistance;
            agent.radius = avoidanceRadius;
            agent.height = avoidanceHeight;
            
        }

        private void Start()
        {
            characterStats = GetComponent<CharacterStats>();

            ModifyAoERadius();

            myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            animator.applyRootMotion = true; //TODO: Consider if needed.

            currentDestination = transform.position;

            agent.updateRotation = false;
            agent.updatePosition = true;
            agent.stoppingDistance = stoppingDistance;



        }
        private void Update()
        {
            if (agent.remainingDistance > agent.stoppingDistance && isAlive)
                Move(agent.desiredVelocity);
            else
                Move(Vector3.zero);
        }

        public void Kill()
        {
            isAlive = false;
        }

        private void WalkToDestination()
        {
            var playerToClickPoint = currentDestination - transform.position;

            if (playerToClickPoint.magnitude >= 0)
            {
                Move(playerToClickPoint);
            }
            else
            {
                Move(Vector3.zero);
            }
        }

        private Vector3 ShortDestination(Vector3 destination, float shortening)
        {
            Vector3 reductionVector = (destination - transform.position).normalized * shortening;
            return destination - reductionVector;
        }

        public void SetDestination(Vector3 worldPosition)
        {
            agent.SetDestination(worldPosition);
        }

        private void Move(Vector3 movement)
        {
            SetForwardAndTurn(movement);
            ApplyExtraTurnRotation();
            UpdateAnimator();
        }

        private void SetForwardAndTurn(Vector3 movement)
        {
            if (movement.magnitude > moveThreshold)
            {
                movement.Normalize();
            }
            var localMove = transform.InverseTransformDirection(movement);
            turnAmount = Mathf.Atan2(localMove.x, localMove.z);
            forwardAmount = localMove.z;
        }

        private void UpdateAnimator()
        {
            animator.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
            animator.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
            animator.speed = animatiorSpeedMultiplier;
        }
        private void ApplyExtraTurnRotation()
        {
            // help the character turn faster (this is in addition to root rotation in the animation)
            float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
            transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
        }

        void OnAnimatorMove()
        {
            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (Time.deltaTime > 0)
            {
                Vector3 velocity = (animator.deltaPosition * moveSpeedMultiplier) / Time.deltaTime;
                // we preserve the existing y part of the current velocity.
                velocity.y = myRigidbody.velocity.y;
                myRigidbody.velocity = velocity;
            }
        }



        private void ModifyAoERadius()
        {
            AoEBehaviour[] AoEAbilities = GetComponents<AoEBehaviour>();
            if (AoEAbilities.Length > 0)
            {
                foreach (AoEBehaviour aoEability in AoEAbilities)
                {
                    aoEability.SetRadiusModifier(characterStats.GetAoEModifier());
                }
            }
        }


        public bool IsTargetInRange(Character target)
        {
            float distanceToTarget = (target.transform.position - transform.position).magnitude;
            return distanceToTarget <= GetComponent<WeaponSystem>().GetCurrentWeaponConfig().MaxAttackRange;
        }
        //TODO: cleaar this mess, Maybe move to Weapon System??.


        public bool GetIsAlive()
        {
            return isAlive;
        }

        public void SetIsAlive(bool _isAlive) {
            this.isAlive = _isAlive;
        }

        public AnimatorOverrideController GetOverrideController() {
            return overrideController;
        }
        public float GetAnimatorSpeedMultiplier() {
            return animatiorSpeedMultiplier;
        }
    }


}