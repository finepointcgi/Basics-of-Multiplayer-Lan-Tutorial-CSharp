using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
	[Export]
	public PackedScene Bullet;

	private Vector2 syncPos = new Vector2(0,0);
	private float syncRotation = 0;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public override void _Ready(){
		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
	}
	public override void _PhysicsProcess(double delta)
	{
		if(GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority() == Multiplayer.GetUniqueId())
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
				Rpc("fire");
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
			syncPos = GlobalPosition;
			syncRotation = GetNode<Node2D>("GunRotation").RotationDegrees;
		}else{
			GlobalPosition = GlobalPosition.Lerp(syncPos, .1f);
			GetNode<Node2D>("GunRotation").RotationDegrees = Mathf.Lerp(GetNode<Node2D>("GunRotation").RotationDegrees, syncRotation, .1f);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
	private void fire(){
		Node2D b = Bullet.Instantiate<Node2D>();
		b.RotationDegrees = GetNode<Node2D>("GunRotation").RotationDegrees;
		b.GlobalPosition = GetNode<Node2D>("GunRotation/BulletSpawn").GlobalPosition;
		GetTree().Root.AddChild(b);
	}

	public void SetUpPlayer(string name){
		GetNode<Label>("Label").Text = name;
	}
}
