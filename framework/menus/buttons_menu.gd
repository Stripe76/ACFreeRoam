extends Control

enum STATE {STATE_NONE = 0,STATE_COLLAPSED = 1,STATE_EXPANDED = 2}

# if set, it'll be filled with MenuItem when "command" not set 
@export var menu_item: MenuItem
@export var sub_menu: Control

@export_category("Collapsing")
@export var expand_state : STATE = STATE.STATE_NONE
@export var spread_y : float = 60:
	set(value):
		spread_y = value
		update_spreading( )
@export_range(0.0,1.0) var opacity = 0.0


func _ready() -> void:
	if menu_item:
		set_buttons(menu_item)


func set_buttons(menuItem: MenuItem,clear : bool = false)-> void:
	if clear:
		clear_buttons()
	var c := 0
	for i : MenuItem in menuItem.get_children():
		var b = Button.new()
		b.name = "Button_%d" % c
		c += 1
		b.text = i.title
		b.anchor_right = 1
		b.size_flags_horizontal = Control.SIZE_FILL
		b.size_flags_vertical = Control.SIZE_SHRINK_CENTER
		b.custom_minimum_size = Vector2(200,50)
		b.pressed.connect(func ():
			if i.command != "":
				if sub_menu:
					sub_menu.clear_buttons()
				if expand_state == STATE.STATE_NONE or expand_state == STATE.STATE_EXPANDED:
					get_tree().call_group("UI",i.command,i.parameter)
				if expand_state == STATE.STATE_EXPANDED:
					expand_state = STATE.STATE_COLLAPSED
				elif expand_state == STATE.STATE_COLLAPSED:
					expand_state = STATE.STATE_EXPANDED
				get_tree().call_group("UI","button_state_changed",name,expand_state)
			elif sub_menu:
				sub_menu.set_buttons(i,true)
			if not expand_state == 2:
				var node = get_node(NodePath(b.name))
				if node:
					move_child(node,0)
		)
		add_child(b)
		b.owner = self


func clear_buttons()-> void:
	var children = get_children()
	for c in children:
		c.free()


func update_spreading()-> void:
	for i in get_child_count():
		if get_child(i) is Button:
			var button : Button = get_child(i)
			button.modulate.a = opacity if i > 0 else 1.0
			button.position.y = spread_y * i
