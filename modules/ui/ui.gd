extends Control


func preview_mode(enable: bool):
	$MainPanel.visible = !enable
	$PreviewPanel.visible = enable
