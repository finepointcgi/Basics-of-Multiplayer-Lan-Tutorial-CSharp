using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
	[Export]
	public PackedScene Bullet;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();


	public override void _PhysicsProcess(double delta)
	{
		
			Vector2 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
				velocity.Y += gravity * (float)delta;

			// Handle Jump.
			if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
				velocity.Y = JumpVelocity;

			GetNode<Node2D>("GunRotation").LookAt(GetViewport().GetMousePosition());

			if(Input.IsActionJustPressed("fire")){
				Node2D b = Bullet.Instantiate<Node2D>();
				b.RotationDegrees = GetNode<Node2D>("GunRotation").RotationDegrees;
				b.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
				GetTree().Root.AddChild(b);
			}

			// Get the input direction and handle the movement/deceleration.
			// As good practice, you should replace UI actions with custom gameplay actions.
			Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
			if (direction != Vector2.Zero)
			{
				velocity.X = direction.X * Speed;
			}
			else
			{
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			}

			Velocity = velocity;
			MoveAndSlide();
		
	}


	public void SetUpPlayer(string name){
		GetNode<Label>("Label").Text = name;
	}
}
