extends Control

func button_state_changed(menu_name: String,state: int):
	if menu_name == "Main":
		if state == 1:
			$ExpandCollapse.play_backwards("expand")
		elif state == 2:
			$ExpandCollapse.play("expand")
