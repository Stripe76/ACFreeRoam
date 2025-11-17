class_name Track
extends Node3D

const track_scene : String = "res://modules/tracks/ACTrack/ACTrack.tscn"

var ac_track


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


func get_pit_stall(stall: int) -> Node3D:
	if ac_track:
		return ac_track.GetPitStall(stall)
	return null
	

func load_track(ac_folder: String,track: String,variant: String):
	if ac_track:
		remove_child(ac_track)
		ac_track.queue_free()
		ac_track = null
	
	ac_track = ResourceLoader.load(track_scene).instantiate()
	ac_track.name = "ACTrack"
	ac_track.LoadTrack( ac_folder,track,variant )
	add_child(ac_track)
	ac_track.owner = self
