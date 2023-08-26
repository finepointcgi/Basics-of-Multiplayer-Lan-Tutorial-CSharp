using Godot;
using System;
using System.Linq;
using System.Text.Json;


public partial class ServerBrowser : Control
{

	[Export]
	PacketPeerUdp broadcaster;
	[Export]
	PacketPeerUdp listener = new PacketPeerUdp();
	[Export]
	int listenPort = 8911;
	[Export]
	int hostPort = 8912;
	[Export]
	string broadcastAddress = "192.168.1.255"; 
	[Signal]
	public delegate void JoinGameEventHandler(string ip);
	[Export]
	PackedScene ServerInfo;

	Timer broadcastTimer;

	ServerInfo serverInfo;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		broadcastTimer = GetNode<Timer>("BroadcastTimer");
		setUpListener();
	}

	private void setUpListener(){
		var ok = listener.Bind(listenPort);

		if(ok == Error.Ok){
			GD.Print("Bound to listen port " + listenPort.ToString());
			GetNode<Label>("Label").Text = "Bound To Listen Port: true";
		}else{
			GD.Print("Failed to bind to listen port!");
			GetNode<Label>("Label").Text = "Bound To Listen Port: false";
		}
	}

	public void SetUpBroadcast(string name){
		broadcaster = new PacketPeerUdp();
		serverInfo = new ServerInfo(){
			Name = name,
			PlayerCount = GameManager.Players.Count
		};

		GD.Print(serverInfo);

		broadcaster.SetBroadcastEnabled(true);
		broadcaster.SetDestAddress(broadcastAddress, listenPort);

		var ok = broadcaster.Bind(hostPort);

		if(ok == Error.Ok){
			GD.Print("Bound to Broadcast Port " + hostPort.ToString());
		}else{
			GD.Print("Failed To Bind To Broadcast Port");
		}

		broadcastTimer.Start();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(listener.GetAvailablePacketCount() > 0){
			string serverIP = listener.GetPacketIP();
			int serverPort = listener.GetPacketPort();
			byte[] bytes  = listener.GetPacket();
			ServerInfo info = JsonSerializer.Deserialize<ServerInfo>(bytes.GetStringFromAscii());
			GD.Print("server ip " + serverIP + "server port " + serverPort + "server info " + bytes.GetStringFromAscii());

			Node currentNode = GetNode<VBoxContainer>("Panel/VBoxContainer").GetChildren().Where(x => x.Name == info.Name).FirstOrDefault();

			if(currentNode != null){
				currentNode.GetNode<Label>("PlayerCount").Text = info.PlayerCount.ToString();
				currentNode.GetNode<Label>("IP").Text = serverIP;

				return;
			}

			ServerBrowserInfoLine serverInfo = ServerInfo.Instantiate<ServerBrowserInfoLine>();
			serverInfo.Name = info.Name;
			serverInfo.GetNode<Label>("Name").Text = serverInfo.Name;
			serverInfo.GetNode<Label>("IP").Text = serverIP;
			serverInfo.GetNode<Label>("PlayerCount").Text = info.PlayerCount.ToString();
            GetNode<VBoxContainer>("Panel/VBoxContainer").AddChild(serverInfo);

			serverInfo.JoinGame += _on_join_game;

		}
	}

	private void _on_join_game(string ip){
		EmitSignal(SignalName.JoinGame, ip);
	}

	private void _on_broadcast_timer_timeout(){
		GD.Print("Broadcasting Game");
		serverInfo.PlayerCount = GameManager.Players.Count;

		string json  = JsonSerializer.Serialize(serverInfo);
		GD.Print(json);
		var packet = json.ToAsciiBuffer();

		broadcaster.PutPacket(packet);
	}

	public void CleanUp(){
		listener.Close();
		broadcastTimer.Stop();
		if(broadcaster != null){
			broadcaster.Close();
		}
	}
}

