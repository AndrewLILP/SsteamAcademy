using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
        // Add these fields to the existing [Header("Character Input Values")] section
        [Header("Golf Input Values")]
        public bool enterGolfMode;
        public bool exitGolfMode;
        public bool golfDrive;
        public bool golfChip;
        public bool golfPutt;
        public bool executeShot;
        
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM

        // Add these methods to the existing input methods

        public void OnEnterGolfMode(InputValue value)
        {
            EnterGolfModeInput(value.isPressed);
        }

        public void OnExitGolfMode(InputValue value)
        {
            ExitGolfModeInput(value.isPressed);
        }

        public void OnGolfDrive(InputValue value)
        {
            GolfDriveInput(value.isPressed);
        }

        public void OnGolfChip(InputValue value)
        {
            GolfChipInput(value.isPressed);
        }

        public void OnGolfPutt(InputValue value)
        {
            GolfPuttInput(value.isPressed);
        }

        public void OnExecuteShot(InputValue value)
        {
            ExecuteShotInput(value.isPressed);
        }


        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}
#endif

        // Add these input handling methods
        public void EnterGolfModeInput(bool newState)
        {
            enterGolfMode = newState;
        }

        public void ExitGolfModeInput(bool newState)
        {
            exitGolfMode = newState;
        }

        public void GolfDriveInput(bool newState)
        {
            golfDrive = newState;
        }

        public void GolfChipInput(bool newState)
        {
            golfChip = newState;
        }

        public void GolfPuttInput(bool newState)
        {
            golfPutt = newState;
        }

        public void ExecuteShotInput(bool newState)
        {
            executeShot = newState;
        }


        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
	
}