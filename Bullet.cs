using Godot;
using System;

public partial class Bullet : CharacterBody2D
{
	public const float Speed = 500.0f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	private Vector2 direction = new Vector2();
	public override void _Ready(){
		direction = new Vector2(1,0).Rotated(Rotation);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		velocity = Speed * direction;
		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y += gravity * (float)delta;

		Velocity = velocity;
		MoveAndSlide();
	}

	private void _on_timer_timeout(){
		QueueFree();
	}
}
