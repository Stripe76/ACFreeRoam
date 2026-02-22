class_name Track
extends Node3D

const track_scene : String = "res://modules/tracks/ACTrack/ACTrack.tscn"

var _ac_track


func _ready() -> void:
	pass # Replace with function body.


func get_pit_stall(stall: int) -> Node3D:
	if _ac_track:
		return _ac_track.GetPitStall(stall)
	return null
	

func load_track(ac_folder: String,track: String,variant: String):
	if _ac_track:
		remove_child(_ac_track)
		_ac_track.queue_free()
		_ac_track = null
	
	_ac_track = ResourceLoader.load(track_scene).instantiate()
	_ac_track.name = "ACTrack"
	_ac_track.LoadTrack( ac_folder,track,variant )
	add_child(_ac_track)
	_ac_track.owner = self
