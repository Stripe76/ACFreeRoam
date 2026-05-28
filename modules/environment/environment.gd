extends Node3D

@onready var sky_dome : SkyDome = $Sky3D/SkyDome
@onready var time_of_day : TimeOfDay = $Sky3D/TimeOfDay


func pause():
	time_of_day.pause()


func resume():
	time_of_day.resume()


func time_of_day_changed(value: float):
	time_of_day.current_time = value


func clouds_changed(value: float):
	if value < 0.5:
		sky_dome.cirrus_coverage = value / 0.5
		sky_dome.cumulus_coverage = 0
	else:
		#if value < 0.75:
			#sky_dome.cirrus_coverage = 1
		#else:
			#sky_dome.cirrus_coverage = 1 - (value-0.75) / 0.25
		sky_dome.cirrus_coverage = 1 - (value-0.5) / 0.5
		sky_dome.cumulus_coverage = (value-0.5) / 0.5


func fog_changed(value: float):
	if is_zero_approx(value):
		sky_dome.fog_visible = false
		sky_dome.environment.fog_enabled = false
	else:
		if value < 0.5:
			sky_dome.fog_visible = true
			sky_dome.environment.fog_enabled = false
			sky_dome.fog_density = (value / 0.5) * 0.005
		else:
			sky_dome.fog_visible = false
			sky_dome.environment.fog_enabled = true
		sky_dome.environment.fog_density = ((value - 0.5) / 0.5) * 0.05
