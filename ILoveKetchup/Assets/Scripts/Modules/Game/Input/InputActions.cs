using UnityEngine;
using System;

namespace Game.Input
{
    [CreateAssetMenu(fileName = "InputActions", menuName = "Game/InputActions", order = 1)]
    public class InputActions : ScriptableObject
    {
        public Action<Vector2> OnMousePositionUpdate;
        public Action<Vector2> OnGamepadDirectionUpdate;
        
        public Action<Vector2> OnSelectDown;

        public Action OnSelect;
        public Action OnCancel;
        public Action OnReturn;

        /// <summary>
        /// Mouse & touch being dragged.
        /// </summary>
        public Action OnDragStarted;
        public Action OnDragEnd;

        public Action OnSelectUp;
        public Action OnCancelUp;
        public Action OnReturnUp;

        /// <summary>
        /// Manually triggers select button. Used for touch screen maybe?
        /// </summary>
        public void ManuallyTriggerSelect() {
            Development.Log("[InputActions] ManuallyTriggerSelect ()");
            OnSelect?.Invoke();
        }

        /// <summary>
        /// Manually triggers cancel button. Used for touch screen maybe?
        /// </summary>
        public void ManuallyTriggerCancel () {
            Development.Log("[InputActions] ManuallyTriggerCancel ()");
            OnCancel?.Invoke();
            OnCancelUp?.Invoke();
        }

        /// <summary>
        /// Manually triggers return button. Used for touch screen maybe?
        /// </summary>
        public void ManuallyTriggerReturn () {
            Development.Log("[InputActions] ManuallyTriggerReturn ()");
            OnReturn?.Invoke();
        }
    }
}
