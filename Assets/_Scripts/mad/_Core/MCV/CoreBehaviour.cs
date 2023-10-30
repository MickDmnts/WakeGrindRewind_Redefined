using System;
using UnityEngine;

namespace WGRF.Core
{
    public abstract class CoreBehaviour : MonoBehaviour
    {
        [Header("Set controller component unique ID")]
        /// <summary>The ID of the specific Component registered into the assigned Controller.</summary>
        [SerializeField, Tooltip("The ID of the specific Component registered into the assigned Controller.")]
        string _id = "";

        /// <summary>The Controller that this component will register into.</summary>
        Controller _controller;

        /// <summary>Gives access to the Hub.</summary>
        public Hub Hub => Hub.Instance;
        /// <summary>Gives access to the assigned controller.</summary>
        public Controller Controller => _controller;
        /// <summary>Returns the assigned component ID to the Controller.</summary>
        public string ID => _id;

        /// <summary>
        /// Assigns a component ID to be registered into the assigned Controller.
        /// </summary>
        public void SetID(string setID)
        {
            _id = setID.Trim();
        }

        /// <summary>
        /// Assigns a controller that this component will register into.
        /// </summary>
        public void SetController(Controller setCtr)
        {
            if (setCtr == null)
            {
                Debug.LogError("No controller assigned at " + gameObject.name);
                return;
            }

            _controller = setCtr;
        }

        protected virtual void Awake()
        {
            PreAwake();

            RegisterController(_controller);
            PostAwake();
        }

        /// <summary>
        /// Called on Awake and before the registration of this script to its assigned controller.
        /// Warning: A function that should be used from all the child classes instead of the standard MonoBehaviour.Awake().
        /// </summary>
        protected virtual void PreAwake() { }

        /// <summary>
        /// Called on Awake and after the registration of this script to its assigned controller.
        /// Warning: A function that should be used from all the child classes instead of the standard MonoBehaviour.Awake().
        /// </summary>
        protected virtual void PostAwake() { }

        /// <summary>
        /// Registers this script to an assigned controller.
        /// </summary>
        void RegisterController(Controller cntr)
        {
            Type theType = this.GetType();

            if (cntr == this)
            {
                Debug.LogError("Loop controller assignment detected on " + theType.FullName + " with ID \"" + _id + "\".");
                return;
            }

            if (cntr)
            { cntr.Register(this); }
        }

        protected virtual void OnDestroy()
        {
            PreDestroy();
            if (_controller) { _controller.Clear(this); }
            PostDestroy();
        }

        /// <summary>
        /// Called on Destroy and before this script is cleared from its assigned controller.
        /// Warning: A function that should be used from all the child classes instead of the standard MonoBehaviour.OnDestroy().
        /// </summary>
        protected virtual void PreDestroy() { }

        /// <summary>
        /// Called on Destroy and after this script is cleared from its assigned controller.
        /// Warning: A function that should be used from all the child classes instead of the standard MonoBehaviour.OnDestroy().
        /// </summary>
        protected virtual void PostDestroy() { }
    }
}